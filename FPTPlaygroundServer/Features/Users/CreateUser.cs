using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Common.Settings;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Auth.Mappers;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using FPTPlaygroundServer.Services.Auth.Models;
using FPTPlaygroundServer.Features.Users.Models;
using FPTPlaygroundServer.Features.Users.Mappers;

namespace FPTPlaygroundServer.Features.Users;

[ApiController]
[JwtValidationFilter]
[RequestValidation<Request>]
public class CreateUser : ControllerBase
{
    public new record Request(
        Guid ServerId,
        string UserName,
        string Name
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.UserName)
                .NotEmpty().WithMessage("UserName cannot be empty")
                .MinimumLength(1).WithMessage("UserName must be between 1 and 25 characters long")
                .MaximumLength(25).WithMessage("UserName must be between 1 and 25 characters long")
                .Matches(@"^(?=.*[a-z])[a-z0-9_]{1,25}$") // Chỉ cho phép chữ cái English, không khoảng trắng, không số, không ký tự đặc biệt
                .WithMessage("UserName can only contain English letters (A-Z, a-z), no spaces, no numbers, and no special characters");

            RuleFor(r => r.Name)
                .NotEmpty().WithMessage("Name cannot be empty")
                .MinimumLength(1).WithMessage("Name must be between 1 and 35 characters long")
                .MaximumLength(35).WithMessage("Name must be between 1 and 35 characters long")
                .Matches(@"^[\p{L} ]+$")
                .WithMessage("Name cannot contain numbers or special characters");
        }
    }

    [HttpPost("user")]
    [Tags("Users")]
    [SwaggerOperation(
        Summary = "Create User",
        Description = "This API is for create user."
    )]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService, IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings.Value.SigningKey));
        var userRequest = httpContextAccessor.HttpContext?.Request;
        var authHeader = userRequest?.Headers.Authorization.ToString();
        var token = authHeader?.Replace("Bearer ", string.Empty);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = key,
            ValidIssuer = jwtSettings.Value.Issuer,
            ValidAudience = jwtSettings.Value.Audience,
            ClockSkew = TimeSpan.Zero
        };

        if (string.IsNullOrEmpty(token))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPA_00)
                .AddReason("token", "Missing Token")
                .Build();
        }

        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        var userInfoJson = principal.Claims.FirstOrDefault(c => c.Type == "UserInfo")?.Value;

        if (string.IsNullOrEmpty(userInfoJson))
            throw FPTPlaygroundException.NewBuilder()
            .WithCode(FPTPlaygroundErrorCode.FPA_00)
            .AddReason("token", "Don't have user info in Token.")
            .Build();

        var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("server", "Server not exist")
                .Build();

        var desrializeSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
        };

        var jObject = JObject.Parse(userInfoJson);

        if (jObject["UserId"]?.Type != JTokenType.Null) //TH có userId trong token thì phải check coi userId này có nằm cùng server không, phòng TH truyền vào token có userId thì cũng tạo mới user nếu khác server
        {
            var isUserExist = await context.Users.AnyAsync(u => u.Id == jObject["UserId"]!.ToObject<Guid?>() && u.ServerId == server.Id);
            if (isUserExist) //TH nếu user đã tồn tại ở server này rồi thì báo lỗi
            {
                throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("token", "User already created.")
                .Build();
            }
        }

        var tokenInfo = new TokenRequest { UserId = jObject["UserId"]?.ToObject<Guid?>(), Email = jObject["Email"]!.ToString() };
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == tokenInfo.Email);

        var isUserNameExist = await context.Users.AnyAsync(u => u.UserName == request.UserName && u.ServerId == server.Id);
        if (isUserNameExist)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("user", "UserName exist")
                .Build();
        }

        User newUser = new();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                DateTime currentTime = DateTime.UtcNow;
                newUser = new()
                {
                    AccountId = account!.Id,
                    ServerId = request.ServerId,
                    UserName = request.UserName,
                    Name = request.Name,
                    Gender = Gender.Other,
                    Status = UserStatus.Active,
                    LastSeenAt = currentTime,
                    CreatedAt = currentTime,
                    UpdatedAt = currentTime
                };
                await context.Users.AddAsync(newUser);
                await context.SaveChangesAsync(); // Lưu để có User.Id

                CoinWallet newCoinWallet = new()
                {
                    UserId = newUser.Id,
                    Amount = 0
                };
                await context.CoinWallets.AddAsync(newCoinWallet);

                DiamondWallet newDiamondWallet = new()
                {
                    UserId = newUser.Id,
                    Amount = 0
                };
                await context.DiamondWallets.AddAsync(newDiamondWallet);

                var levelPass = await context.LevelPasses.FirstOrDefaultAsync(lp => lp.Level == 0);
                Data.Entities.UserLevelPass userLevelPass = new()
                {
                    UserId = newUser.Id,
                    LevelPassId = levelPass!.Id,
                    Experience = 0,
                    IsClaim = true,
                };
                await context.UserLevelPasses.AddAsync(userLevelPass);

                DateTime today = DateTime.UtcNow.Date;

                // Tìm ngày đầu tuần hiện tại, bắt đầu từ thứ Hai
                int diff = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
                DateTime startOfWeek = today.AddDays(-diff); // Bắt đầu từ 7h sáng của ngày đầu tuần. Lưu ý 7h sáng VN tức là 0h UTC. Và lưu xuống DB là 7h +7 => 7 - 7 = 0h UTC

                for (int i = 0; i < 7; i++)
                {
                    DateTime checkpoint = startOfWeek.AddDays(i);

                    DailyCheckpoint dailyCheckpoint = new()
                    {
                        UserId = newUser.Id,
                        CoinValue = 200,
                        DiamondValue = i == 6 ? 50 : null,
                        CheckInDate = checkpoint,
                        Status = DailyCheckpointStatus.Unchecked,
                    };
                    await context.DailyCheckpoints.AddAsync(dailyCheckpoint);
                }

                // Lưu tất cả vào database
                await context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                // Rollback nếu có lỗi
                await transaction.RollbackAsync();
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPS_00)
                    .AddReason("server", "Something wrong with the server")
                    .Build();
            }
        });

        string tokenResonse = tokenService.CreateToken(newUser.ToTokenRequest()!);
        string refreshToken = tokenService.CreateRefreshToken(newUser.ToTokenRequest()!);

        return Ok(new CreateUserResponse
        {
            UserResponse =  newUser.ToUserResponse()!,
            Token = tokenResonse,
            RefreshToken = refreshToken
        });
    }
}

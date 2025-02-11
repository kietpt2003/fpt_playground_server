using FluentValidation;
using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Filters;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Features.Auth.Models;
using FPTPlaygroundServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;

namespace FPTPlaygroundServer.Features.Users;

[ApiController]
[RequestValidation<Request>]
public class CreateUser : ControllerBase
{
    private const int SaltSize = 16; // 128 bit 
    private const int KeySize = 32;  // 256 bit
    private const int Iterations = 10000; // Number of PBKDF2 iterations

    public new record Request(
        string Email,
        string Password,
        Guid ServerId,
        string UserName,
        string Name,
        Gender Gender
    );

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email cannot be empty")
                .EmailAddress()
                .WithMessage("Invalid email");

            RuleFor(r => r.Password)
                .NotEmpty().WithMessage("Password cannot be empty")
                .MinimumLength(8).WithMessage("Password must be between 8 and 15 characters long")
                .MaximumLength(15).WithMessage("Password must be between 8 and 15 characters long")
                .Matches(@"^(?=.*[A-Z])(?=.*\W)(?=.*\d).{8,15}$")
                .WithMessage("Password must contain at least 1 uppercase letter, 1 special character, and 1 digit.");

            RuleFor(r => r.UserName)
                .NotEmpty().WithMessage("UserName cannot be empty")
                .MinimumLength(1).WithMessage("UserName must be between 1 and 35 characters long")
                .MaximumLength(35).WithMessage("UserName must be between 1 and 35 characters long")
                .Matches(@"^[A-Za-z]+$") // Chỉ cho phép chữ cái English, không khoảng trắng, không số, không ký tự đặc biệt
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
    [Tags("User")]
    [SwaggerOperation(
        Summary = "Create User",
        Description = "This API is for create user."
    )]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(FPTPlaygroundErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Handler([FromBody] Request request, [FromServices] AppDbContext context, [FromServices] TokenService tokenService)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_00)
                .AddReason("account", "Account not exist")
                .Build();

        if (!VerifyHashedPassword(account.Password!, request.Password))
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("password", "Incorrect password")
                .Build();
        }

        var server = await context.Servers.FirstOrDefaultAsync(s => s.Id == request.ServerId) ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("server", "Server not exist")
                .Build();

        var user = await context.Users.FirstOrDefaultAsync(u => u.AccountId == account.Id && u.ServerId == request.ServerId);
        if (user != null)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_01)
                .AddReason("user", "User exist")
                .Build();
        }
        else
        {
            var isUserNameExist = await context.Users.AnyAsync(u => u.UserName == request.UserName);
            if (isUserNameExist)
            {
                throw FPTPlaygroundException.NewBuilder()
                    .WithCode(FPTPlaygroundErrorCode.FPB_01)
                    .AddReason("user", "UserName exist")
                    .Build();
            }

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    DateTime currentTime = DateTime.UtcNow;
                    User newUser = new()
                    {
                        AccountId = account.Id,
                        ServerId = request.ServerId,
                        UserName = request.UserName,
                        Name = request.Name,
                        Gender = request.Gender,
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
        }
        return Created();
    }

    private static bool VerifyHashedPassword(string hashedPassword, string passwordToCheck)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        using (var algorithm = new Rfc2898DeriveBytes(passwordToCheck, salt, Iterations, HashAlgorithmName.SHA256))
        {
            var keyToCheck = algorithm.GetBytes(KeySize);
            for (int i = 0; i < KeySize; i++)
            {
                if (hashBytes[i + SaltSize] != keyToCheck[i])
                {
                    return false;
                }
            }
        }

        return true;
    }
}

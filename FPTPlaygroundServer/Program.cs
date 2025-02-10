using FluentValidation;
using FPTPlaygroundServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllerServices();
builder.Services.AddSwaggerServices();

builder.Services.AddBackgroundServices();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddConfigureSettings(builder.Configuration);
builder.Services.AddDbContextConfiguration(builder.Configuration);
builder.Services.AddServices();
builder.Services.AddCorsPolicy();
builder.Services.AddConfigureApiBehavior();
builder.Services.AddFPTPlaygroundRateLimiter();

var app = builder.Build();

app.UseFPTPlaygroundExceptionHandler();
app.UseFPTPlaygroundRateLimiter();
app.UseCors();
app.UseSwaggerServices();
app.UseAuthentication();
app.UseAuthorization();
app.ApplyMigrations();

app.MapControllers().RequireRateLimiting("concurrency");

app.Run();

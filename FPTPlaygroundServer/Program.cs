using FluentValidation;
using FPTPlaygroundServer.Extensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(dbConnectionString))
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnectionString;
    }

    var jwtIssuerString = Environment.GetEnvironmentVariable("JWT_ISSUER");
    if (!string.IsNullOrEmpty(jwtIssuerString))
    {
        builder.Configuration["JWT:Issuer"] = jwtIssuerString;
    }

    var jwtAudienceString = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    if (!string.IsNullOrEmpty(jwtAudienceString))
    {
        builder.Configuration["JWT:Audience"] = jwtAudienceString;
    }

    var jwtSigningKey = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
    if (!string.IsNullOrEmpty(jwtSigningKey))
    {
        builder.Configuration["JWT:SigningKey"] = jwtSigningKey;
    }

    var smtpMailString = Environment.GetEnvironmentVariable("SMTP_MAIL");
    if (!string.IsNullOrEmpty(smtpMailString))
    {
        builder.Configuration["SmtpClient:Mail"] = smtpMailString;
    }

    var smtpPasswordString = Environment.GetEnvironmentVariable("SMTP_PASWORD");
    if (!string.IsNullOrEmpty(smtpPasswordString))
    {
        builder.Configuration["SmtpClient:Password"] = smtpPasswordString;
    }

    var payOSClientIdString = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID");
    if (!string.IsNullOrEmpty(payOSClientIdString))
    {
        builder.Configuration["Payment:PayOS:ClientID"] = payOSClientIdString;
    }

    var payOSApiKeyString = Environment.GetEnvironmentVariable("PAYOS_API_KEY");
    if (!string.IsNullOrEmpty(payOSApiKeyString))
    {
        builder.Configuration["Payment:PayOS:ApiKey"] = payOSApiKeyString;
    }

    var payOSChecksumKeyString = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY");
    if (!string.IsNullOrEmpty(payOSChecksumKeyString))
    {
        builder.Configuration["Payment:PayOS:ChecksumKey"] = payOSChecksumKeyString;
    }

    var googleStorageBucketString = Environment.GetEnvironmentVariable("GOOGLE_STORAGE_BUCKET");
    if (!string.IsNullOrEmpty(googleStorageBucketString))
    {
        builder.Configuration["GoogleStorage:Bucket"] = googleStorageBucketString;
    }
}

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

app.MapControllers().RequireRateLimiting("concurrency");

app.Run();

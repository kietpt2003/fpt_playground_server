using FPTPlaygroundServer.Common.Exceptions;
using FPTPlaygroundServer.Common.Utils;
using FPTPlaygroundServer.Data;
using FPTPlaygroundServer.Data.Entities;
using FPTPlaygroundServer.Services.Mail;
using Microsoft.EntityFrameworkCore;

namespace FPTPlaygroundServer.Services.VerifyCode;

public class VerifyCodeService(MailService mailService, AppDbContext context)
{
    private static readonly string EmailTitle = @"Xác thực tài khoản - {0}";
    private static readonly string EmailBody = @"
                <html>
                    <body>
                        <p>Chào mừng bạn đến với FPT Plaground!</p>
                        <br />
                        <p>Hãy lướt qua lại một vài thông tin cơ bản nhé:</p>
                        <ul>
                            <li>Email: {0}</li>
                        </ul>
                        <br />
                        <p>Để tiếp tục trải nghiệm mọi thứ mà ứng dụng chúng tôi cung cấp, mời bạn nhập mã xác thực sau đây vào
                        ứng dụng của chúng tôi:</p>
                        <br />
                        <p><strong>Mã xác thực: {1}</strong></p>
                    </body>
                </html>
                ";

    private readonly MailService _mailService = mailService;
    private readonly long VerificationDuration = 5L * 60;
    private readonly AppDbContext _context = context;

    public async Task SendVerifyCodeAsync(Account account)
    {
        var code = VerifyCodeGenerator.Generate();

        var accountVerify = new AccountVerify
        {
            VerifyCode = code,
            VerifyStatus = VerifyStatus.Pending,
            Account = account,
            CreatedAt = DateTime.UtcNow,
        };

        _context.AccountVerifies.Add(accountVerify);
        await _context.SaveChangesAsync();

        var emailBody = string.Format(EmailBody, account.Email, code);

        await _mailService.SendMail(string.Format(EmailTitle, account.Email), account.Email, emailBody);
    }

    public async Task ResendVerifyCodeAsync(Account account)
    {
        await _context.AccountVerifies
               .Where(a => a.AccountId == account.Id)
               .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.VerifyStatus, VerifyStatus.Expired));

        await SendVerifyCodeAsync(account);
    }

    public async Task VerifyUserAsync(Account account, string code)
    {
        if (account.Status != AccountStatus.Pending)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("account", "Account is not in verification status")
                .Build();
        }

        var accountVerify = await _context.AccountVerifies
                .Where(a => a.VerifyCode == code && a.AccountId == account.Id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync() ?? throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("verifyCode", "Incorrect verify code")
                .Build();

        var maxTime = accountVerify.CreatedAt.AddSeconds(VerificationDuration);
        if (maxTime < DateTime.UtcNow)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("verifyCode", "Verify code expired")
                .Build();
        }
        await _context.AccountVerifies
                .Where(a => a.Id == accountVerify.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.VerifyStatus, VerifyStatus.Verified));

        await _context.Accounts
                .Where(a => a.Id == account.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.Status, AccountStatus.Active));
    }

    public async Task VerifyUserChangePasswordAsync(Account account, string code)
    {
        if (account.Status != AccountStatus.Active)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_03)
                .AddReason("account", "Account is not activated or account is locked.")
                .Build();
        }

        var accountVerify = await _context.AccountVerifies
                .Where(a => a.VerifyCode == code && a.AccountId == account.Id)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

        if (accountVerify == null)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("verifyCode", "Mã xác thực không hợp lệ")
                .Build();
        }
        var maxTime = accountVerify.CreatedAt.AddSeconds(VerificationDuration);
        if (maxTime < DateTime.UtcNow)
        {
            throw FPTPlaygroundException.NewBuilder()
                .WithCode(FPTPlaygroundErrorCode.FPB_02)
                .AddReason("verifyCode", "Verify code expired")
                .Build();
        }
        await _context.AccountVerifies
                .Where(a => a.Id == accountVerify.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.VerifyStatus, VerifyStatus.Verified));
    }
}

namespace FPTPlaygroundServer.Data.Entities;

public class AccountVerify
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string VerifyCode { get; set; } = default!;
    public VerifyStatus VerifyStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account Account { get; set; } = default!;
}

public enum VerifyStatus
{
    Pending, Verified, Expired
}

using System.ComponentModel.DataAnnotations.Schema;

namespace FPTPlaygroundServer.Data.Entities;

public class Report
{
    public Guid Id { get; set; }
    [ForeignKey(nameof(Sender))]
    public Guid SenderId { get; set; }
    [ForeignKey(nameof(Suspect))]
    public Guid SuspectId { get; set; }
    public string Content { get; set; } = default!;
    public ReportType Type { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Sender { get; set; } = default!;
    public User Suspect { get; set; } = default!;
}

public enum ReportType
{
    Game, Chat
}

public enum ReportStatus
{
    Checked, Unchecked
}

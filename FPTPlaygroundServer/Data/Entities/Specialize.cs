namespace FPTPlaygroundServer.Data.Entities;

public class Specialize
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;

    public Specialize? SpecializeEntity { get; set; }
}

using FPTPlaygroundServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FPTPlaygroundServer.Data.Configurations;

public class UserLevelPassConfiguration : IEntityTypeConfiguration<UserLevelPass>
{
    public void Configure(EntityTypeBuilder<UserLevelPass> builder)
    {
        builder.HasKey(x => new { x.UserId, x.LevelPassId });
    }
}

using FPTPlaygroundServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FPTPlaygroundServer.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<AccountVerify> AccountVerifies { get; set; } = default!;
    public DbSet<Achievement> Achievements { get; set; } = default!;
    public DbSet<CoinWallet> CoinWallets { get; set; } = default!;
    public DbSet<Conversation> Conversations { get; set; } = default!;
    public DbSet<ConversationMember> ConversationMembers { get; set; } = default!;
    public DbSet<DailyCheckpoint> DailyCheckpoints { get; set; } = default!;
    public DbSet<Device> Devices { get; set; } = default!;
    public DbSet<DiamondWallet> DiamondWallets { get; set; } = default!;
    public DbSet<FaceValue> FaceValues { get; set; } = default!;
    public DbSet<Friendship> Friendships { get; set; } = default!;
    public DbSet<LeaderBoard> LeaderBoards { get; set; } = default!;
    public DbSet<LevelPass> LevelPasses { get; set; } = default!;
    public DbSet<MaskedAvatar> MaskedAvatars { get; set; } = default!;
    public DbSet<Mate> Mates { get; set; } = default!;
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<MessageStatus> MessageStatuses { get; set; } = default!;
    public DbSet<Notification> Notifications { get; set; } = default!;
    public DbSet<Report> Reports { get; set; } = default!;
    public DbSet<Server> Servers { get; set; } = default!;
    public DbSet<Specialize> Specializes { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<UserAchievement> UserAchievements { get; set; } = default!;
    public DbSet<UserIncome> UserIncomes { get; set; } = default!;
    public DbSet<UserLevelPass> UserLevelPasses { get; set; } = default!;
    public DbSet<UserMasked> UserMaskeds { get; set; } = default!;
    public DbSet<WalletTracking> WalletTrackings { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }
}

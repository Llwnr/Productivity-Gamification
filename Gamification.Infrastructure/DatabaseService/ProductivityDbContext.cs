using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Gamification.Core.Models;
using Gamification.Core.GameModels;

namespace Gamification.Infrastructure.DatabaseService;

public class ProductivityDbContext(DbContextOptions<ProductivityDbContext> options) : DbContext(options){
    public DbSet<Site> Sites{ get; set; }
    public DbSet<AnalysisResult> AnalysisResults{ get; set; }
    public DbSet<UserSiteVisit> UserSiteVisits{ get; set; }
    public DbSet<User> Users{ get; set; }
    public DbSet<GameStat> GameStats{ get; set; }
    public DbSet<Achievement> Achievements{ get; set; }
    public DbSet<UserAchievement> UserAchievements{ get; set; }

    //Defines the schema constraints, namings etc
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        SetupUsersTable(modelBuilder);
        SetupSitesTable(modelBuilder);
        SetupAnalysisResultsTable(modelBuilder);
        SetupUserSiteVisitsTable(modelBuilder);
        SetupGameStatsTable(modelBuilder);
        SetupAchievementTable(modelBuilder);
    }

    void SetupUsersTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<User>();

        entity.HasKey(u => u.UserId);
        entity.Property(u => u.UserId).HasDefaultValueSql("gen_random_uuid()");

        entity.HasIndex(u => u.Username).IsUnique();
        entity.HasIndex(u => u.Email).IsUnique();

        entity
            .HasOne(u => u.GameStat)
            .WithOne(gameStat => gameStat.User)
            .HasForeignKey<GameStat>(gameStat => gameStat.UserId);
    }

    void SetupSitesTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<Site>();

        entity.HasKey(p => p.SiteId);
        entity.Property(s => s.SiteId).HasDefaultValueSql("gen_random_uuid()");
        entity.HasIndex(s => s.Url).IsUnique();
    }
    
    void SetupAnalysisResultsTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<AnalysisResult>();
        
        entity.HasKey(p => p.AnalysisId);
        entity.Property(p => p.AnalysisId).HasDefaultValueSql("gen_random_uuid()");
        
        entity
            .HasOne(ar => ar.Site)
            .WithMany(s => s.AnalysisResults)
            .HasForeignKey(ar => ar.SiteId)
            .IsRequired();
        
        //There can only be 1 analysis for a site with the same goal.
        //However, same site can have multiple analysis results based on different goals
        entity.HasIndex(a => new {a.SiteId, a.UserGoal}).IsUnique();
    }

    void SetupUserSiteVisitsTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<UserSiteVisit>();

        entity.HasKey(usv => usv.VisitId);
        entity.Property(usv => usv.VisitId).HasDefaultValueSql("gen_random_uuid()");

        entity.HasIndex(usv => usv.VisitStartDate);
        entity
            .HasIndex(usv => usv.ProcessedAt)
            .HasFilter("\"processed_at\" IS NULL");
        
        entity
            .HasOne(usv => usv.Analysis)
            .WithMany(a => a.UserSiteVisit)
            .HasForeignKey(usv => usv.AnalysisId);
        
        entity
            .HasOne(usv => usv.User)
            .WithMany(u => u.UserSiteVisits)
            .HasForeignKey(usv => usv.UserId);
        
        entity
            .HasOne(usv => usv.Site)
            .WithMany(u => u.UserSiteVisits)
            .HasForeignKey(usv => usv.SiteId);
    }

    void SetupGameStatsTable(ModelBuilder modelBuilder){
        var entity = modelBuilder.Entity<GameStat>();

        entity.HasKey(gs => gs.StatId);
        entity.Property(gs=>gs.StatId).HasDefaultValueSql("gen_random_uuid()");
        
        entity.Property(gs => gs.ProductivityMetrics)
            .HasConversion(
                data => JsonSerializer.Serialize(data, (JsonSerializerOptions)null),
                data => JsonSerializer.Deserialize<Dictionary<GameStat.TimeFrequency, TimeSpan>>(data, (JsonSerializerOptions)null)
                )
            .HasColumnType("jsonb");
    }

    void SetupAchievementTable(ModelBuilder modelBuilder){
        var userAchievement = modelBuilder.Entity<UserAchievement>();
        userAchievement.HasKey(ua => ua.UserAchievementId);
        userAchievement.Property(ua => ua.UserAchievementId)
            .HasDefaultValueSql("gen_random_uuid()");

        userAchievement
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAchievements)
            .HasForeignKey(ua => ua.UserId);

        userAchievement
            .HasOne(ua => ua.Achievement)
            .WithMany(a => a.AchievedUsers)
            .HasForeignKey(ua => ua.AchievementId);

        var achievementTable = modelBuilder.Entity<Achievement>();
        achievementTable.HasKey(a => a.AchievementId);
        achievementTable.Property(a => a.AchievementId)
            .HasDefaultValueSql("gen_random_uuid()");

        achievementTable
            .HasIndex(a => a.Key)
            .IsUnique();
    }

    public AnalysisResult? GetAnalysisOfSite(string siteId, string userId){
        string userGoal = Users.FirstOrDefault(u => u.UserId == userId)?.Goal ?? throw new Exception("User not found");
        return AnalysisResults.FirstOrDefault(s => s.SiteId == siteId && s.UserGoal == userGoal);
    }
}


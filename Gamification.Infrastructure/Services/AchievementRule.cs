using Gamification.Core.GameModels;
using Gamification.Core.Models;
using Gamification.Infrastructure.Events;
using Gamification.Infrastructure.Interfaces;

namespace Gamification.Infrastructure.Services;

public class AchievementRule<TEvent> : IAchievementRule where TEvent : GameEvent{
    private readonly string _achievementKey;
    private readonly Predicate<TEvent> _condition;

    public AchievementRule(string achievementKey, Predicate<TEvent> condition){
        _achievementKey = achievementKey;
        _condition = condition;
    }
    
    public void Evaluate(GameEvent gameEvent, Action<User, string> grantAchievement){
        if (gameEvent is TEvent myGameEvent && _condition(myGameEvent)){
            grantAchievement(myGameEvent.User, _achievementKey);
            // Console.WriteLine("Achievement given for: " + _condition);
        }
    }
}

public static class AchievementRules
{
    // The list now correctly holds the non-generic interface IAchievementRule.
    public static readonly List<IAchievementRule> Rules = new()
    {
        // --- Growth Achievements (Reacts to a single large XP gain) ---
        new AchievementRule<ExpGainedEvent>("growth_10000", e => e.GainedExp > 10000),
        new AchievementRule<ExpGainedEvent>("growth_50000", e => e.GainedExp > 50000),

        // --- Level Up Achievements ---
        new AchievementRule<LevelUpEvent>("level_5", e => e.NewLevel >= 5),
        new AchievementRule<LevelUpEvent>("level_10", e => e.NewLevel >= 10),
        new AchievementRule<LevelUpEvent>("level_25", e => e.NewLevel >= 25),
        new AchievementRule<LevelUpEvent>("level_50", e => e.NewLevel >= 50),
        new AchievementRule<LevelUpEvent>("level_100", e => e.NewLevel >= 100),
        
        // Daily Time
        new AchievementRule<ProcessingFinishedEvent>("daily_1_hour", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Daily, out var time) && time.TotalHours >= 1),
        new AchievementRule<ProcessingFinishedEvent>("daily_4_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Daily, out var time) && time.TotalHours >= 4),
        new AchievementRule<ProcessingFinishedEvent>("daily_8_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Daily, out var time) && time.TotalHours >= 8),

        // Daily Streaks
        new AchievementRule<ProcessingFinishedEvent>("daily_streak_3", e => e.User.GameStat.DailyStreakCount >= 3),
        new AchievementRule<ProcessingFinishedEvent>("daily_streak_7", e => e.User.GameStat.DailyStreakCount >= 7),
        new AchievementRule<ProcessingFinishedEvent>("daily_streak_30", e => e.User.GameStat.DailyStreakCount >= 30),
        new AchievementRule<ProcessingFinishedEvent>("daily_streak_100", e => e.User.GameStat.DailyStreakCount >= 100),

        // Lifetime Hours
        new AchievementRule<ProcessingFinishedEvent>("lifetime_100_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Lifetime, out var time) && time.TotalHours >= 100),
        new AchievementRule<ProcessingFinishedEvent>("lifetime_1000_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Lifetime, out var time) && time.TotalHours >= 1000),

        // Monthly Hours
        new AchievementRule<ProcessingFinishedEvent>("monthly_50_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Monthly, out var time) && time.TotalHours >= 50),
        new AchievementRule<ProcessingFinishedEvent>("monthly_100_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Monthly, out var time) && time.TotalHours >= 100),

        // Weekly Hours
        new AchievementRule<ProcessingFinishedEvent>("weekly_10_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Weekly, out var time) && time.TotalHours >= 10),
        new AchievementRule<ProcessingFinishedEvent>("weekly_25_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Weekly, out var time) && time.TotalHours >= 25),
        new AchievementRule<ProcessingFinishedEvent>("weekly_40_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Weekly, out var time) && time.TotalHours >= 40),

        // Weekly Streaks
        new AchievementRule<ProcessingFinishedEvent>("weekly_streak_4", e => e.User.GameStat.WeeklyStreakCount >= 4),
        new AchievementRule<ProcessingFinishedEvent>("weekly_streak_12", e => e.User.GameStat.WeeklyStreakCount >= 12),
        new AchievementRule<ProcessingFinishedEvent>("weekly_streak_52", e => e.User.GameStat.WeeklyStreakCount >= 52),

        // Total XP
        new AchievementRule<ProcessingFinishedEvent>("xp_10000", e => e.User.GameStat.ExperiencePoints >= 10000),
        new AchievementRule<ProcessingFinishedEvent>("xp_50000", e => e.User.GameStat.ExperiencePoints >= 50000),
        new AchievementRule<ProcessingFinishedEvent>("xp_100000", e => e.User.GameStat.ExperiencePoints >= 100000),
        new AchievementRule<ProcessingFinishedEvent>("xp_250000", e => e.User.GameStat.ExperiencePoints >= 250000),
        new AchievementRule<ProcessingFinishedEvent>("xp_500000", e => e.User.GameStat.ExperiencePoints >= 500000),

        // Yearly Hours
        new AchievementRule<ProcessingFinishedEvent>("yearly_500_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Yearly, out var time) && time.TotalHours >= 500),
        new AchievementRule<ProcessingFinishedEvent>("yearly_1000_hours", e => e.User.GameStat.ProductivityMetrics.TryGetValue(GameStat.TimeFrequency.Yearly, out var time) && time.TotalHours >= 1000),
    };
}
namespace Gamification.Core.GameModels;

public class Achievement{
    public string AchievementId{ get; set; }
    public string Key{ get; set; }
    public ResetTimeEnum ResetTime{ get; set; }
    public string ImageUrl{ get; set; }
    public string Title{ get; set; }
    public string Description{ get; set; }
    
    public enum ResetTimeEnum{
        Never,
        Daily,
        Weekly,
        Monthly
    }
    
    public ICollection<UserAchievement>? AchievedUsers{ get; set; }
}

public static class AchievementDefinition{
    public static List<Achievement> GetAchievementDefinitions() =>
    new List<Achievement>{
        // Productivity Metrics (Time-Based) Achievements
        new Achievement{
            Key = "daily_1_hour",
            ResetTime = Achievement.ResetTimeEnum.Daily,
            ImageUrl = "/images/achievements/novice_navigator.png",
            Title = "Novice Navigator",
            Description = "Log 1 hour of productive time in a single day."
        },
        new Achievement{
            Key = "daily_4_hours",
            ResetTime = Achievement.ResetTimeEnum.Daily,
            ImageUrl = "/images/achievements/focused_finisher.png",
            Title = "Focused Finisher",
            Description = "Log 4 hours of productive time in a single day."
        },
        new Achievement{
            Key = "daily_8_hours",
            ResetTime = Achievement.ResetTimeEnum.Daily,
            ImageUrl = "/images/achievements/productivity_pro.png",
            Title = "Productivity Pro",
            Description = "Log 8 hours of productive time in a single day."
        },
        new Achievement{
            Key = "weekly_10_hours",
            ResetTime = Achievement.ResetTimeEnum.Weekly,
            ImageUrl = "/images/achievements/weekly_warrior.png",
            Title = "Weekly Warrior",
            Description = "Accumulate 10 hours of productive time in a week."
        },
        new Achievement{
            Key = "weekly_25_hours",
            ResetTime = Achievement.ResetTimeEnum.Weekly,
            ImageUrl = "/images/achievements/consistent_contender.png",
            Title = "Consistent Contender",
            Description = "Accumulate 25 hours of productive time in a week."
        },
        new Achievement{
            Key = "weekly_40_hours",
            ResetTime = Achievement.ResetTimeEnum.Weekly,
            ImageUrl = "/images/achievements/week_dominator.png",
            Title = "Week Dominator",
            Description = "Accumulate 40 hours of productive time in a week."
        },
        new Achievement{
            Key = "monthly_50_hours",
            ResetTime = Achievement.ResetTimeEnum.Monthly,
            ImageUrl = "/images/achievements/monthly_master.png",
            Title = "Monthly Master",
            Description = "Reach 50 hours of productive time in a month."
        },
        new Achievement{
            Key = "monthly_100_hours",
            ResetTime = Achievement.ResetTimeEnum.Monthly,
            ImageUrl = "/images/achievements/centurion_of_focus.png",
            Title = "Centurion of Focus",
            Description = "Reach 100 hours of productive time in a month."
        },
        new Achievement{
            Key = "yearly_500_hours",
            ResetTime = Achievement.ResetTimeEnum.Never, // Yearly achievements are often better as 'Never' reset to show lifetime yearly bests
            ImageUrl = "/images/achievements/yearly_yeoman.png",
            Title = "Yearly Yeoman",
            Description = "Log 500 productive hours in a year."
        },
        new Achievement{
            Key = "yearly_1000_hours",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/thousand_hour_club.png",
            Title = "The Thousand Hour Club",
            Description = "Log 1000 productive hours in a year."
        },
        new Achievement{
            Key = "lifetime_100_hours",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/time_titan.png",
            Title = "Time Titan",
            Description = "Reach 100 lifetime productive hours."
        },
        new Achievement{
            Key = "lifetime_1000_hours",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/productivity_legend.png",
            Title = "Productivity Legend",
            Description = "Reach 1000 lifetime productive hours."
        },
    
        // Daily & Weekly Streaks Achievements
        new Achievement{
            Key = "daily_streak_3",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/triple_threat.png",
            Title = "Triple Threat",
            Description = "Maintain a 3-day productive streak."
        },
        new Achievement{
            Key = "daily_streak_7",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/week_of_wonder.png",
            Title = "Week of Wonder",
            Description = "Maintain a 7-day productive streak."
        },
        new Achievement{
            Key = "daily_streak_30",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/month_of_momentum.png",
            Title = "Month of Momentum",
            Description = "Maintain a 30-day productive streak."
        },
        new Achievement{
            Key = "daily_streak_100",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/the_centurion.png",
            Title = "The Centurion",
            Description = "Maintain a 100-day productive streak."
        },
        new Achievement{
            Key = "weekly_streak_4",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/four_week_focus.png",
            Title = "Four Week Focus",
            Description = "Maintain a 4-week productive streak."
        },
        new Achievement{
            Key = "weekly_streak_12",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/quarter_century.png",
            Title = "Quarter Century",
            Description = "Maintain a 12-week productive streak."
        },
        new Achievement{
            Key = "weekly_streak_52",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/year_of_zeal.png",
            Title = "Year of Zeal",
            Description = "Maintain a 52-week productive streak."
        },
    
        // Experience Points (XP) & Leveling Up Achievements
        new Achievement{
            Key = "level_5",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/level_5.png",
            Title = "Level 5 Reached",
            Description = "Reach level 5."
        },
        new Achievement{
            Key = "level_10",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/level_10.png",
            Title = "Double Digits",
            Description = "Reach level 10."
        },
        new Achievement{
            Key = "level_25",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/level_25.png",
            Title = "Quarter Master",
            Description = "Reach level 25."
        },
        new Achievement{
            Key = "level_50",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/level_50.png",
            Title = "Half-Century Mark",
            Description = "Reach level 50."
        },
        new Achievement{
            Key = "level_100",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/level_100.png",
            Title = "Level 100 Legend",
            Description = "Reach level 100."
        },
        new Achievement{
            Key = "xp_1000",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/xp_collector.png",
            Title = "XP Collector",
            Description = "Earn a total of 1,000 Experience Points."
        },
        new Achievement{
            Key = "xp_10000",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/xp_enthusiast.png",
            Title = "XP Enthusiast",
            Description = "Earn a total of 10,000 Experience Points."
        },
        new Achievement{
            Key = "xp_100000",
            ResetTime = Achievement.ResetTimeEnum.Never,
            ImageUrl = "/images/achievements/xp_overlord.png",
            Title = "XP Overlord",
            Description = "Earn a total of 100,000 Experience Points."
        }
    };
}

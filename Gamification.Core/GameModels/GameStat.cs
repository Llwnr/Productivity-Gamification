using Gamification.Core.Models;

namespace Gamification.Core.GameModels;

public class GameStat{
    public string StatId { get; set; }
    
    public string UserId { get; set; }
    public User? User{ get; set; }
    
    public int Coin{ get; set; }
    public float ExperiencePoints{ get; set; }
    public int Level{ get; set; }

    public Dictionary<TimeFrequency, TimeSpan> ProductivityMetrics{ get; set; } = new Dictionary<TimeFrequency, TimeSpan>{
        {TimeFrequency.Daily, TimeSpan.Zero},
        {TimeFrequency.Weekly, TimeSpan.Zero},
        {TimeFrequency.Monthly, TimeSpan.Zero},
        {TimeFrequency.Yearly, TimeSpan.Zero},
        {TimeFrequency.Lifetime, TimeSpan.Zero}
    };
    public int DailyStreakCount{ get; set; } //If user is able to be productive for a given amount per day, this streak goes up
    public int WeeklyStreakCount{ get; set; }
    
    public enum TimeFrequency{
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Lifetime // Permanenet or lifetime
    }
}
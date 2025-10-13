using Gamification.Core.Models;

namespace Gamification.Core.GameModels;

public class GameStat{
    public enum TimeFrequency{
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Lifetime // Permanenet or lifetime
    }
    public string StatId { get; set; }
    
    public string UserId { get; set; }
    public User? User{ get; set; }
    
    public int Coin{ get; set; }
    public float ExperiencePoints{ get; set; }
    public int Level{ get; set; }
    
    // public Dictionary<TimeFrequency, TimeSpan> TimeSpent{ get; set; }
    // public int DailyStreakCount{ get; set; }
}
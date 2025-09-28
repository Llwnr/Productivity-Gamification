using Gamification.Core.Models;

namespace Gamification.Core.GameModels;

public class GameStat{
    public string StatId { get; set; }
    
    public string UserId { get; set; }
    public User? User{ get; set; }
    
    public int Coin{ get; set; }
    public float ExperiencePoints{ get; set; }
    public int Level{ get; set; }
    // public int Streak{ get; set; }
}
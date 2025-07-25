namespace Gamification.Core.Models;

public class GameStat{
    public int StatId { get; set; }
    
    public int UserId { get; set; }
    public User? User{ get; set; }
    
    public int Coin{ get; set; }
    public float ExperiencePoints{ get; set; }
    public int Level{ get; set; }
    // public int Streak{ get; set; }
}
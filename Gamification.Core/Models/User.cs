namespace Gamification.Core.Models;

public class User{
    public int UserId{ get; set; }
    
    public string? Username{ get; set; }
    public string? Email{ get; set; }
    public string? Password{ get; set; }
    
    public ICollection<UserSiteVisit>? UserSiteVisits{ get; set; }
    public ICollection<GameStat>?  GameStats{ get; set; }
}
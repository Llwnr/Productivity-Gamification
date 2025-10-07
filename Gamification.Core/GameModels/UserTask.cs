using Gamification.Core.Models;

namespace Gamification.Core.GameModels;

public abstract class UserTask{
    public string Id{ get; set; }
    public string UserId{ get; set; }
    public string Title{ get; set; }
    public string? Notes{ get; set; }
    public int? RewardPoints{ get; set; }
    public string? Tag{ get; set; }
    
    public User? User{ get; set; }
}
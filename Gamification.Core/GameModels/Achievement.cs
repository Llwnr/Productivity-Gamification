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

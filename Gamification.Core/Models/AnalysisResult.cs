namespace Gamification.Core.Models;


public class AnalysisResult{
    public int AnalysisId{ get; set; }
    public int SiteId{ get; set; }
    public Site? Site{ get; set; } //Foreign key navigation property

    public string UserGoal{ get; set; }
    
    public List<string> Category{ get; set; }
    public int IntrinsicScore{ get; set; }
    public float RelevanceScore{ get; set; }
    
    public ICollection<UserSiteVisit>? UserSiteVisit{ get; set; }
}
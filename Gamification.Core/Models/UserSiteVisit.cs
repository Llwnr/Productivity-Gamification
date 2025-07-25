namespace Gamification.Core.Models;

public class UserSiteVisit{
    public int SiteVisitId{ get; set; }
    
    public int UserId{ get; set; }
    public User? User{ get; set; }
    
    public int AnalysisId{ get; set; }
    public AnalysisResult? Analysis{ get; set; }
    
    public DateTime VisitDate{ get; set; }
    public DateTime? ProcessedAt{ get; set; } //Determines at what time the score was calculated to award points to the user.
}
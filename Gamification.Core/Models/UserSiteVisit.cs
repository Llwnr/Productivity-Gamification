namespace Gamification.Core.Models;

public class UserSiteVisit{
    public string VisitId{ get; set; }
    
    public string UserId{ get; set; }
    public User? User{ get; set; }

    public string? SiteId{ get; set; }
    public Site? Site{ get; set; }

    public string? AnalysisId{ get; set; }
    public AnalysisResult? Analysis{ get; set; }
    
    public DateTime VisitStartDate{ get; set; }
    public DateTime? VisitEndDate{ get; set; }
    
    public DateTime? ProcessedAt{ get; set; } //Determines at what time the score was calculated to award points to the user.
}
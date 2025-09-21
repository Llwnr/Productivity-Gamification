namespace Gamification.Core.Models;

public class SiteVisitDTO{
    public string SiteUrl{ get; set; }
    public float TimeSpent{ get; set; }//In seconds
    public float BaseProductiveScore{ get; set; }
    public string MainCategory{ get; set; }
    public DateTime VisitDate{ get; set; }
}
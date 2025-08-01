using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamification.Core.Models;

public class Site{
    public string SiteId { get; set; }
    public string? Url { get; set; }
    public string? Title{ get; set; }
    public string? Description{ get; set; }
    
    public ICollection<AnalysisResult>? AnalysisResults { get; set; }
    public ICollection<UserSiteVisit>? UserSiteVisits { get; set; }
}

namespace Gamification.WebAPI.Models;

public class SiteVisitDTO{
    public required string Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
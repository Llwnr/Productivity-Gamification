namespace Gamification.Core.Models;

public class Prompt{
    public required string Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string UserId { get; set; }
    public DateTime VisitStartTime { get; set; }
    public DateTime? VisitEndTime { get; set; }
    
    public override string ToString(){
        return $"\nUrl: {Url}\nTitle: {Title}\nDescription: {Description}\nVisited On: {VisitStartTime}";
    }
}
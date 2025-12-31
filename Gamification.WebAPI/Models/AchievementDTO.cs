namespace Gamification.WebAPI.Models;

public class AchievementDTO {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime EarnedAt { get; set; }
}
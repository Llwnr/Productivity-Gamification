namespace Gamification.Core.Models;

public class ProductivityLog{
    public string ProductivityLogId{ get; set; }
    
    public string UserId{ get; set; }
    public User User{ get; set; }
    
    //The day this log represents. Is always at Year-Month-Day : 00:00:00
    public DateTime LogDate{ get; set; }
    
    public TimeSpan ProductiveTime{ get; set; } = TimeSpan.Zero;
    public TimeSpan UnproductiveTime{ get; set; } = TimeSpan.Zero;
}
namespace Gamification.Core.GameModels;

public class Habit : UserTask{
    public bool IsPositive{ get; set; }
    public bool IsNegative{ get; set; }
    public int PositiveCount{ get; set; }
    public int NegativeCount{ get; set; }
    public DateTime ResetInterval{ get; set; }
}
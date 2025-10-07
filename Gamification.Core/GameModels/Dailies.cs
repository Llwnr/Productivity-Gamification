namespace Gamification.Core.GameModels;

public class Dailies : UserTask{
    public DateTime StartDate { get; set; }
    public string RepeatFrequency { get; set; }
    public string RepeatInterval{ get; set; }
    public string RepeatEvery{ get; set; }
    public List<string> Checklist{ get; set; }
}
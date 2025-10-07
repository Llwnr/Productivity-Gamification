namespace Gamification.Core.GameModels;

public class Todo : UserTask{
    public DateTime DueDate{ get; set; }
    public bool IsCompleted{ get; set; }
    public List<string> Checklist{ get; set; }//The list of things to complete for the to-do. 
}
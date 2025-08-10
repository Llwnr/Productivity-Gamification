namespace Gamification.Core.Models;

public record Prompt(string UserGoal, string Url, string Title, string Description){
    public override string ToString(){
        return $"User goal: {UserGoal}\nUrl: {Url}\nTitle: {Title}\nDescription: {Description}";
    }
}
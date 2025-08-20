namespace Gamification.Core.Models;

public record Prompt(string Url, string Title, string? Description){
    public override string ToString(){
        return $"\nUrl: {Url}\nTitle: {Title}\nDescription: {Description}";
    }
}
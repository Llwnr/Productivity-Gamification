namespace Gamification.Core.Interfaces;

public interface ISiteAnalysisService{
    Task<bool> AnalyzeSite(string userGoal, string url, string title, string desc);
}
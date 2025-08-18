namespace Gamification.Infrastructure.Interfaces;

public interface IContentAnalysisFilter{
    bool IsAnalysisRequired(string content);
}
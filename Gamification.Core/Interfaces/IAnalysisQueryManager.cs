using Gamification.Core.Models;

namespace Gamification.Core.Interfaces;

public interface IAnalysisQueryManager{
    public Task EnqueueAnalysisQuery(Prompt prompt, string userId);
}
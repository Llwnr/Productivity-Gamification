using Gamification.Core.Models;

namespace Gamification.Core.Interfaces;

public interface ISiteAnalysisService{
    Task<bool> AnalyzeSite(Prompt prompt, string userId, DateTime visitTime);
}
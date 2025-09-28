using Gamification.Core.Models;

namespace Gamification.Core.Interfaces;

public interface ISiteAnalysisService{
    Task<bool> AnalyzeSites(List<Prompt> prompts);
}
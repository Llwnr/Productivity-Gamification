using Gamification.Core.Interfaces;

namespace Gamification.Core.Services;
/// <summary>
/// Provides services for calculation of productivity score when analyzing sites
/// </summary>
public class ScoreCalculationService : IScoreCalculationService{
    public float GetFinalScore(int intrinsicScore, float relevanceScore){
        return (0.5f * intrinsicScore) * relevanceScore;
    }
}
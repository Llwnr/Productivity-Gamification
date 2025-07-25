namespace Gamification.Core.Interfaces;

public interface IScoreCalculationService{
    public float GetFinalScore(int intrinsicScore, float relevanceScore);
}
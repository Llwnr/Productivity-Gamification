namespace Gamification.Core.Interfaces;

public interface IProductivityService{
    public float GetFinalScore(int intrinsicScore, float relevanceScore);
}
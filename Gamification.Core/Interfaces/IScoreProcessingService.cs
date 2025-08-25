namespace Gamification.Core.Interfaces;

public interface IScoreProcessingService{
    public int ProcessScore(string userId); //Processes the user's scores, then returns num. of rows/records processed
}
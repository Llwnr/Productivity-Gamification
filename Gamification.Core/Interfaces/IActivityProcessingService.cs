namespace Gamification.Core.Interfaces;

public interface IActivityProcessingService{
    public Task<int> ProcessUserActivityAsync(); //Processes the user's scores, then returns num. of rows/records processed
}
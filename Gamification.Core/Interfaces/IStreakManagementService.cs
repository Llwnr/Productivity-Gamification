namespace Gamification.Core.Interfaces;

public interface IStreakManagementService{
    public Task<int> ManageDailyStreak();
    public Task<int> ManageWeeklyStreak();
}
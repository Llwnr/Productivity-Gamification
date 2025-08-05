namespace Gamification.Core.Interfaces;

/// <summary>
/// Manages when the user is inactive/ has stopped browsing a site.
/// It properly records data about the user, i.e. when the user has stopped browsing.
/// </summary>
public interface IInactivityRecordingService{
    public void RecordAsInactive(string userId);
    public void RecordAsInactive(string userId, DateTime lastActiveTime);
}
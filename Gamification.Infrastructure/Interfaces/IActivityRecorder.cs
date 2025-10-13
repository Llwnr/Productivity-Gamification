using Gamification.Core.Models;

namespace Gamification.Infrastructure.Interfaces;

public interface IActivityRecorder{
    public void AddSiteVisit(Site visitedSite, string userId);
}
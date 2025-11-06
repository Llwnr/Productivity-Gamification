using Gamification.Core.Models;
using Gamification.Infrastructure.Events;

namespace Gamification.Infrastructure.Interfaces;

public interface IAchievementRule{
    void Evaluate(GameEvent gameEvent, Action<User, string> grantAchievement);
}
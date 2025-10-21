namespace Gamification.Infrastructure.Events;

public record LevelUpEvent(string UserId, int NewLevel) : GameEvent;
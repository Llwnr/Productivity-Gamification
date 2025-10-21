namespace Gamification.Infrastructure.Events;

public record ExpGainedEvent(string UserId, float GainedExp, float TotalExp) : GameEvent;
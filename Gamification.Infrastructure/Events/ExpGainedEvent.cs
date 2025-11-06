using Gamification.Core.Models;

namespace Gamification.Infrastructure.Events;

public record ExpGainedEvent(User User, float GainedExp, float TotalExp) : GameEvent(User);
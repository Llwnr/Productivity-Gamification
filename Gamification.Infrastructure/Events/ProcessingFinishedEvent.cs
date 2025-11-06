using Gamification.Core.Models;

namespace Gamification.Infrastructure.Events;

public record ProcessingFinishedEvent(User User) : GameEvent(User);
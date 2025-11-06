using Gamification.Core.Models;

namespace Gamification.Infrastructure.Events;

public abstract record GameEvent(User User);
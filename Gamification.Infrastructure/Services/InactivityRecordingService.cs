using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;

namespace Gamification.Infrastructure.Services;

public class InactivityRecordingService : IInactivityRecordingService{
    private readonly ProductivityDbContext _dbContext;

    public InactivityRecordingService(ProductivityDbContext dbContext){
        _dbContext = dbContext;
    }
    
    public void RecordAsInactive(string userId){
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitDate = DateTime.UtcNow,
        };

        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }
    public void RecordAsInactive(string userId, DateTime lastActiveTime){
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitDate = lastActiveTime
        };
        
        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }
}
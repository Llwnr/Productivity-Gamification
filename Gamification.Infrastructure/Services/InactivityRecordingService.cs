using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;

namespace Gamification.Infrastructure.Services;

public class InactivityRecordingService : IInactivityRecordingService{
    private readonly ProductivityDbContext _dbContext;

    public InactivityRecordingService(ProductivityDbContext dbContext){
        _dbContext = dbContext;
    }

    public void EndVisit(string userId, DateTime? endDate = null){
        UserSiteVisit? lastInsertedItem = _dbContext.UserSiteVisits
            .Where(u => u.UserId == userId && u.VisitEndDate == null && (u.AnalysisId != null && u.SiteId != null))
            .OrderByDescending(u => u.VisitStartDate)
            .FirstOrDefault();
        if (lastInsertedItem != null) lastInsertedItem.VisitEndDate = endDate ?? DateTime.UtcNow;
        _dbContext.SaveChanges();
    }
    
    public void RecordAsInactive(string userId){
        if (AlreadyRecordedInactive()){
            // Console.WriteLine("User is already recorded as inactive");
            return;
        }
        EndVisit(userId);
        return;
        
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitStartDate = DateTime.UtcNow,
        };

        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }
    public void RecordAsInactive(string userId, DateTime lastActiveTime){
        if (AlreadyRecordedInactive()){
            // Console.WriteLine("User is already recorded as inactive");
            return;
        }
        
        EndVisit(userId, lastActiveTime);
        return;
        
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitStartDate = lastActiveTime
        };
        
        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }

    //Checks if the last recorded activity is of user's inactivity i.e. notifying user has stopped browsing or smth
    bool AlreadyRecordedInactive(){
        UserSiteVisit? lastActivity = _dbContext.UserSiteVisits.OrderByDescending(usv =>  usv.VisitStartDate).FirstOrDefault();
        return lastActivity is not {VisitEndDate: null};
    }
}
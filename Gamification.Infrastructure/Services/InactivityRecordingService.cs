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
        if (AlreadyRecordedInactive()){
            Console.WriteLine("User is already recorded as inactive");
            return;
        }
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitDate = DateTime.UtcNow,
        };

        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }
    public void RecordAsInactive(string userId, DateTime lastActiveTime){
        if (AlreadyRecordedInactive()){
            Console.WriteLine("User is already recorded as inactive");
            return;
        }
        UserSiteVisit newActivity = new UserSiteVisit{
            UserId = userId,
            VisitDate = lastActiveTime
        };
        
        _dbContext.Add(newActivity);
        _dbContext.SaveChanges();
    }

    //Checks if the last recorded activity is of user's inactivity i.e. notifying user has stopped browsing or smth
    bool AlreadyRecordedInactive(){
        UserSiteVisit? lastActivity = _dbContext.UserSiteVisits.OrderByDescending(usv =>  usv.VisitDate).FirstOrDefault();
        return lastActivity is {AnalysisId: null, SiteId: null};
    }
}
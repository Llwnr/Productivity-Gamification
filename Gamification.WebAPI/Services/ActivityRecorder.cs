using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Interfaces;

namespace Gamification.WebAPI.Services;

public class ActivityRecorder : IActivityRecorder{
    private readonly ProductivityDbContext _dbContext;

    public ActivityRecorder(ProductivityDbContext dbContext){
        _dbContext = dbContext;
    }

    public void AddSiteVisit(Site visitedSite, string userId){
        UserSiteVisit siteVisit = new UserSiteVisit();
        var existingSite = _dbContext.Sites.FirstOrDefault(s => s.Url == visitedSite.Url);
        if (existingSite != null){
            siteVisit.Site = existingSite;
        }
        else{
            _dbContext.Sites.Add(visitedSite);
            siteVisit.Site = visitedSite;
        }

        User? user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
        if(user == null) throw new Exception("User not found");
        
        siteVisit.User = user;
        siteVisit.VisitStartDate = DateTime.UtcNow;
        
        _dbContext.UserSiteVisits.Add(siteVisit);
        _dbContext.SaveChanges();
    }
}
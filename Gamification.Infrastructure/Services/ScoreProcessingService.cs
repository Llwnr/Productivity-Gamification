using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;
/// <summary>
/// Provides services for calculation of productivity score when analyzing sites
/// </summary>
public class ScoreProcessingService : IScoreProcessingService{

    private readonly ProductivityDbContext _dbContext;
    private readonly ILogger<ScoreProcessingService> _logger;
    
    public ScoreProcessingService(ProductivityDbContext dbContext, ILogger<ScoreProcessingService> logger){
        _dbContext = dbContext;
        _logger = logger;

        ProcessScore("ok");
    }

    public int ProcessScore(string userId){
        UserSiteVisit[] siteVisits = _dbContext.UserSiteVisits
            .Include(u => u.Site)
            .Where(u => u.ProcessedAt == null)
            .OrderBy(u => u.VisitDate)
            .ToArray();
        for (int i = 0; i < siteVisits.Length - 1; i++){
            //Set as processed for all types of records
            siteVisits[i].ProcessedAt = DateTime.UtcNow;
            
            //Only record time spent on active sites, not inactivity recording dummy records.
            if (IsInactiveRecord(siteVisits[i])) continue;
            _logger.LogInformation(
                "Site {site} was visisted for {duration} hours", 
                siteVisits[i].Site.Title,
                (siteVisits[i+1].VisitDate - siteVisits[i].VisitDate).TotalSeconds);
        }
        
        if(siteVisits.Length > 0) siteVisits[^1].ProcessedAt = DateTime.UtcNow;
        _dbContext.SaveChanges();
        return siteVisits.Length;
        
        bool IsInactiveRecord(UserSiteVisit siteVisit) => siteVisit.AnalysisId == null && siteVisit.SiteId == null;
    }
}
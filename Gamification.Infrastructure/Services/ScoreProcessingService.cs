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

        ProcessScore("4420f420-2f98-4cac-a1ab-578c3c2d4b19");
    }

    public int ProcessScore(string userId){
        if(!_dbContext.GameStats.Any(stats => stats.UserId == userId)) CreateNewStats(userId);
        
        UserSiteVisit[] siteVisits = _dbContext.UserSiteVisits
            .Include(u => u.Site)
            .Where(u => u.ProcessedAt == null && u.AnalysisId != null)
            .OrderBy(u => u.VisitStartDate)
            .ToArray();
        for (int i = 0; i < siteVisits.Length; i++){
            //Set as processed for all types of records
            siteVisits[i].ProcessedAt = DateTime.UtcNow;
            //Only record time spent on active sites, not inactivity recording dummy records.
            if (IsInactiveRecord(siteVisits[i])) continue;

            float timeSpent = (float)(siteVisits[i]?.VisitEndDate - siteVisits[i]?.VisitStartDate).Value.TotalSeconds;

            GameStat userStat =  _dbContext.GameStats.Where(stats => stats.UserId == userId).First();
            AnalysisResult? analysis = _dbContext.GetAnalysisOfSite(siteVisits[i].SiteId, userId);
            if (analysis == null) throw new Exception();
            
            userStat.ExperiencePoints += timeSpent * (float)(analysis.IntrinsicScore * 0.5 * analysis.RelevanceScore);
            
            _logger.LogInformation(
                "Site {site} was visisted for {duration} seconds", 
                siteVisits[i].Site.Title,
                timeSpent);
        }
        
        _dbContext.SaveChanges();
        return siteVisits.Length;
        
        bool IsInactiveRecord(UserSiteVisit siteVisit) => siteVisit.AnalysisId == null && siteVisit.SiteId == null;
    }

    //Creates a new stat table for user if its not already created
    void CreateNewStats(string userId){
        GameStat newStat = new GameStat{
            UserId = userId,
            Coin = 0,
            ExperiencePoints = 0,
            Level = 1
        };

        _dbContext.Add(newStat);
        _dbContext.SaveChanges();
    }
}
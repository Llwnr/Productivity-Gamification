using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Core.GameModels;
using Gamification.Infrastructure.DatabaseService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;
/// <summary>
/// Provides services for calculation of productivity score when analyzing sites
/// as well as calculating time spent on productive activities
/// </summary>
public class ActivityProcessingService : IActivityProcessingService{

    private readonly ProductivityDbContext _dbContext;
    private readonly ILogger<ActivityProcessingService> _logger;
    
    public ActivityProcessingService(ProductivityDbContext dbContext, ILogger<ActivityProcessingService> logger){
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> ProcessUserActivityAsync(string userId){
        if(!_dbContext.GameStats.Any(stats => stats.UserId == userId)) await CreateNewStats(userId);
        
        UserSiteVisit[] visitsToProcess = await _dbContext.UserSiteVisits
            .Include(u => u.Site)
            .Include(u => u.Analysis)
            .Include(u => u.User)
            //Pick visits that isn't processed, and has complete duration info i.e. start time and end time
            .Where(u => u.ProcessedAt == null && u.UserId == userId && u.VisitEndDate != null)
            .OrderBy(u => u.VisitStartDate)
            .ToArrayAsync();

        UserSiteVisit[]? processableVisits = FindAndConnectAnalysis(visitsToProcess);
        if (processableVisits == null){
            if (visitsToProcess.Length > 0){
                _logger.LogInformation("Analysis isn't available for any visit to process. Stopped processing.");
            }
            else{
                _logger.LogInformation("No UserSiteVisits processing needed. All are upto date");
            }
            return 0;
        }

        await ProcessScore(processableVisits);
        await ProcessProductivity(processableVisits);
        int processedRows = SetAsProcessed(processableVisits);
        _dbContext.SaveChanges();

        return processedRows;
    }

    async Task ProcessScore(UserSiteVisit[] siteVisits){
        for (int i = 0; i < siteVisits.Length; i++){
            if (siteVisits[i].VisitEndDate == null){
                _logger.LogWarning("A site does not have VisitEndDate set yet. So it is not possible to calculate its score");
                continue;
            }
            if (siteVisits[i].Analysis == null){
                _logger.LogWarning("A site does not have an analysis yet. So it is not possible to calculate its score");
                continue;
            }
            float timeSpent = (float)((siteVisits[i].VisitEndDate - siteVisits[i].VisitStartDate)?.TotalSeconds 
                                      ?? throw new Exception("Visit start date/end date is missing while calculating time spent."));

            GameStat userStat =  await _dbContext.GameStats.
                Where(stats => stats.UserId == siteVisits[i].UserId).FirstOrDefaultAsync()
                ?? throw new Exception("User not found.");
            
            AnalysisResult? analysis = _dbContext.GetAnalysisOfSite(siteVisits[i].SiteId ?? throw new Exception("No site id"), siteVisits[i].UserId);
            if (analysis == null) throw new Exception();
            
            userStat.ExperiencePoints += timeSpent * (float)(analysis.IntrinsicScore * 0.5 * (0.5f + analysis.RelevanceScore));
            
            _logger.LogInformation(
                "Site {site} was visisted for {duration} seconds", 
                siteVisits[i].Site.Title,
                timeSpent);
        }
    }

    async Task ProcessProductivity(UserSiteVisit[] visitsToProcess){
        int productivityThreshold = 50; //Any site which has a productivity score of over 50 is to be selected.
        UserSiteVisit[] productiveVisits =  visitsToProcess
            .Where(visit => visit.Analysis?.IntrinsicScore > productivityThreshold)
            .ToArray();
        
        foreach (UserSiteVisit? visit in productiveVisits){
            GameStat userStat = visit.User?.GameStat ?? throw new Exception("No User or gamestat table found");
            
            TimeSpan productiveTime = TimeSpan.FromSeconds((visit.VisitEndDate - visit.VisitStartDate)?.TotalSeconds ?? 0);
            userStat.ProductivityMetrics[GameStat.TimeFrequency.Daily] += productiveTime;
            userStat.ProductivityMetrics[GameStat.TimeFrequency.Weekly] += productiveTime;
            userStat.ProductivityMetrics[GameStat.TimeFrequency.Monthly] += productiveTime;
            userStat.ProductivityMetrics[GameStat.TimeFrequency.Yearly] += productiveTime;
            userStat.ProductivityMetrics[GameStat.TimeFrequency.Lifetime] += productiveTime;
            
            _dbContext.Entry(userStat).Property(u => u.ProductivityMetrics).IsModified = true;
            
            _logger.LogInformation("Productive time spent is: " + productiveTime);
            _logger.LogInformation("User's new daily productivity is: " + userStat.ProductivityMetrics[GameStat.TimeFrequency.Daily]);
        }
    }

    int SetAsProcessed(UserSiteVisit[] visitsToProcess){
        foreach (var visit in visitsToProcess){
            visit.ProcessedAt = DateTime.UtcNow;
        }

        return visitsToProcess.Length;
    }

    UserSiteVisit[]? FindAndConnectAnalysis(UserSiteVisit[] siteVisits){
        List<UserSiteVisit> analysisIncludedVisits =  new List<UserSiteVisit>();
        foreach (var visit in siteVisits){
            AnalysisResult associatedAnalysis = _dbContext.AnalysisResults
                .FirstOrDefault(analysis => analysis.Site == visit.Site && analysis.UserGoal == visit.User.Goal);
            if (associatedAnalysis == null) continue;

            visit.Analysis = associatedAnalysis;
            analysisIncludedVisits.Add(visit);
        }
        return analysisIncludedVisits.ToArray();
    }

    //Creates a new stat table for user if its not already created
    async Task CreateNewStats(string userId){
        GameStat newStat = new GameStat{
            UserId = userId,
            Coin = 0,
            ExperiencePoints = 0,
            Level = 1
        };

        _dbContext.Add(newStat);
        await _dbContext.SaveChangesAsync();
    }
}
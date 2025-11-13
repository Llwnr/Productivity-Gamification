using System.Threading.Channels;
using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Core.GameModels;
using Gamification.Infrastructure.Events;
using Gamification.Infrastructure.DatabaseService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;
/// <summary>
/// Provides services for calculation of productivity score when analyzing sites
/// as well as calculating time spent on productive activities
/// </summary>
public class ActivityProcessingService : IActivityProcessingService, IStreakManagementService{

    private readonly ProductivityDbContext _dbContext;
    private readonly ILogger<ActivityProcessingService> _logger;

    private readonly Channel<GameEvent> _gameChannel;
    
    public ActivityProcessingService(ProductivityDbContext dbContext, ILogger<ActivityProcessingService> logger, Channel<GameEvent> gameChannel){
        _dbContext = dbContext;
        _logger = logger;
        _gameChannel = gameChannel;
    }

    public async Task<int> ProcessUserActivityAsync(){
        List<string>? allUserIds = _dbContext.Users.Select(u => u.UserId).ToList();
        if (allUserIds.Count == 0){
            _logger.LogInformation("No user exists");
            return 0;
        }

        //If the user doesn't have a GameStat table, make it first before processing scores
        foreach (var userId in allUserIds){
            if(!_dbContext.GameStats.Any(stats => stats.UserId == userId)) await CreateNewStats(userId);
        }
        
        UserSiteVisit[] visitsToProcess = await _dbContext.UserSiteVisits
            .Include(u => u.Site)
            .Include(u => u.Analysis)
            .Include(u => u.User)
            .ThenInclude(user => user.GameStat)
            //Pick visits that isn't processed, and has complete duration info i.e. start time and end time
            .Where(u => u.ProcessedAt == null && u.VisitEndDate.HasValue)
            .OrderBy(u => u.VisitStartDate)
            .ToArrayAsync();

        UserSiteVisit[]? processableVisits = FindAndConnectAnalysis(visitsToProcess);
        if (processableVisits == null || processableVisits.Length == 0){
            if (visitsToProcess.Length > 0){
                _logger.LogInformation("Analysis isn't available for any visit to process. Stopped processing.");
            }
            else{
                _logger.LogInformation("No UserSiteVisits processing needed. All are upto date");
            }
            return 0;
        }

        var visitsByUser = processableVisits.GroupBy(v => v.User);
        int totalProcessedRows = 0;
        foreach (var userVisits in visitsByUser){
            User user = userVisits.Key;
            UserSiteVisit[] visits = userVisits.ToArray();
            
            await ProcessScoreForUser(user, visits);
            await CalculateProductivityTime(user, visits);
            totalProcessedRows += SetAsProcessed(visits);

            await NotifyGameEvent(new ProcessingFinishedEvent(user));

        }
        _dbContext.SaveChanges();

        return totalProcessedRows;
    }

    //Processes the user's activity to provide score metrics such as exp, coins
    async Task ProcessScoreForUser(User user, UserSiteVisit[] siteVisits){
        float totalExpGained = 0;
        for (int i = 0; i < siteVisits.Length; i++){
            if (siteVisits[i].VisitEndDate == null){
                _logger.LogWarning("A site does not have VisitEndDate set yet. So it is not possible to calculate its score");
                continue;
            }
            if (siteVisits[i].Analysis == null){
                _logger.LogWarning("A site does not have an analysis yet. So it is not possible to calculate its score");
                continue;
            }
            float timeSpent = (float)((siteVisits[i].VisitEndDate - siteVisits[i].VisitStartDate)?.TotalSeconds * 0.0167f
                                      ?? throw new Exception("Visit start date/end date is missing while calculating time spent."));

            GameStat userStat =  await _dbContext.GameStats
                                     .Where(stats => stats.UserId == siteVisits[i].UserId).FirstOrDefaultAsync()
                ?? throw new Exception("User not found.");
            
            AnalysisResult? analysis = _dbContext.GetAnalysisOfSite(siteVisits[i].SiteId ?? throw new Exception("No site id"), siteVisits[i].UserId);
            if (analysis == null) throw new Exception();
            
            float expGainAmt = timeSpent * (float)(analysis.IntrinsicScore * 0.5 * (0.5f + analysis.RelevanceScore));
            userStat.ExperiencePoints += expGainAmt;
            totalExpGained += expGainAmt;
            
            for (int expIndex = 0; expIndex < ExperienceTableProgressionRule.ExpTable.Length; expIndex++){
                if (userStat.ExperiencePoints < ExperienceTableProgressionRule.ExpTable[expIndex]){
                    if (userStat.Level != expIndex){
                        userStat.Level = expIndex;
                        await NotifyGameEvent(new LevelUpEvent(user, expIndex));
                    }

                    break;
                }
            }
            // _logger.LogInformation(
            //     "Site {site} was visisted for {duration} seconds", 
            //     siteVisits[i].Site.Title,
            //     timeSpent);
        }

        if (totalExpGained > 0){
            await NotifyGameEvent(new ExpGainedEvent(user, totalExpGained, user.GameStat.ExperiencePoints));
        }
    }

    //Calculates the user's productivity time
    async Task CalculateProductivityTime(User user, UserSiteVisit[] visitsToProcess){
        const int productivityThreshold = 50;

        // Group all visits by their calendar date (ignoring time)
        var visitsByDay = visitsToProcess.GroupBy(visit => visit.VisitStartDate.Date);

        foreach (var dayGroup in visitsByDay){
            DateTime logDate = dayGroup.Key;
        
            // Find or create the log for the specific date of this group of visits
            ProductivityLog userLog = FindOrCreateLogForDate(user.UserId, logDate);

            // Calculate productive time just for this day's visits
            TimeSpan productiveTimeForDay = TimeSpan.FromTicks(
                dayGroup
                    .Where(visit => visit.Analysis?.IntrinsicScore >= productivityThreshold && visit.VisitEndDate.HasValue)
                    .Sum(visit => (visit.VisitEndDate.Value - visit.VisitStartDate).Ticks)
            );
            userLog.ProductiveTime += productiveTimeForDay;

            // Calculate unproductive time just for this day's visits
            TimeSpan unproductiveTimeForDay = TimeSpan.FromTicks(
                dayGroup
                    .Where(visit => visit.Analysis?.IntrinsicScore < productivityThreshold && visit.VisitEndDate.HasValue)
                    .Sum(visit => (visit.VisitEndDate.Value - visit.VisitStartDate).Ticks)
            );
            userLog.UnproductiveTime += unproductiveTimeForDay;
        }
        // _dbContext.SaveChanges() will be called later in the main processing method.
    }
    
    ProductivityLog FindOrCreateLogForDate(string userId, DateTime targetDate){
        // Use the .Date property to strip the time component, ensuring we work with whole days.
        var logForDate = targetDate.Date;

        ProductivityLog? log = _dbContext.ProductivityLogs
            .FirstOrDefault(log => log.LogDate == logForDate && log.UserId == userId);

        if (log == null){
            log = new ProductivityLog{
                UserId = userId,
                LogDate = logForDate, // Set the LogDate to the specific date provided.
            };
            _dbContext.ProductivityLogs.Add(log);
            // Note: The calling function is expected to call SaveChanges().
        }
        return log;
    }

    async Task NotifyGameEvent(GameEvent gameEvent){
        await _gameChannel.Writer.WriteAsync(gameEvent);
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
            Level = 0
        };

        _dbContext.Add(newStat);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> ManageDailyStreak(){
        User[] users = await _dbContext.Users.Include(u => u.GameStat).ToArrayAsync();
        foreach (var user in users){
            TimeSpan dailyProductivityTime = user.GameStat.ProductivityMetrics[GameStat.TimeFrequency.Daily];
            
            if (dailyProductivityTime >= user.DailyTargetHours){
                //Increment the streak by one.
                user.GameStat.DailyStreakCount += 1;
            }

            _logger.LogInformation($"Daily productivity time: {dailyProductivityTime}, target:  {user.DailyTargetHours}");
        }
        await _dbContext.SaveChangesAsync();
        return users.Length;
    }

    public async Task<int> ManageWeeklyStreak(){
        User[] users = await _dbContext.Users.Include(u => u.GameStat).ToArrayAsync();
        foreach (var user in users){
            TimeSpan weeklyProductivityTime = user.GameStat.ProductivityMetrics[GameStat.TimeFrequency.Weekly];
            if (weeklyProductivityTime >= user.DailyTargetHours * 7){
                user.GameStat.WeeklyStreakCount += 1;
            }
            
            _logger.LogInformation($"Weekly productivity time: {weeklyProductivityTime}, target:  {user.DailyTargetHours*7}");
        }

        await _dbContext.SaveChangesAsync();
        return users.Length;
    }
}
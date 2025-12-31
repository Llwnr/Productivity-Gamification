using System.Security.Claims;
using Gamification.Core.GameModels;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DashboardController : ControllerBase{
    private readonly ProductivityDbContext _dbContext;
    private readonly ILogger<DashboardController> _logger;
    
    // public string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public DashboardController(ProductivityDbContext dbContext, ILogger<DashboardController> logger){
        _dbContext = dbContext;
        _logger = logger;
    }
    
    [Authorize]
    [HttpGet("UserStat")]
    public IActionResult SendUserStat(){
        _logger.LogInformation("Sent user's stats");
        GameStat userGameStat = _dbContext.GameStats.First(u => u.UserId == UserId);
        float expForNextLevel = ExperienceTableProgressionRule.ExpTable[userGameStat.Level+1];
        GameStatDTO gameStatDto = new GameStatDTO {
            Username = _dbContext.Users.First(u => u.UserId == UserId).Username,
            Coin = userGameStat.Coin,
            ExperiencePoints = userGameStat.ExperiencePoints,
            Level = userGameStat.Level,
            NextLvlPercentage = (userGameStat.ExperiencePoints/expForNextLevel).ToString("F00"),
            TotalAchievements = _dbContext.UserAchievements.Count(u => u.UserId == UserId),
            DailyStreak = userGameStat.DailyStreakCount,
            WeeklyStreak = userGameStat.WeeklyStreakCount
        };
        
        return Ok(gameStatDto);
    }

    [Authorize]
    [HttpGet("ProductivityLogs")]
    public IActionResult SendProductivityLogs(){
        List<ProductivityLog> logs = _dbContext.ProductivityLogs.Where(u => u.UserId == UserId).ToList();
        List<ProductivityLogDTO> logsDTO = new();
        foreach (ProductivityLog log in logs){
            logsDTO.Add(new ProductivityLogDTO{
                Date = log.LogDate.ToString("yyyy-MM-dd"),
                ProductiveTime = log.ProductiveTime.TotalHours
            });
        }
        _logger.LogInformation("Sent productivity logs");
        return Ok(logsDTO.ToArray());
    }
    
    [Authorize]
    [HttpGet("Analytics")]
    public IActionResult SendUserSiteVisits(){
        // 1. Fetch all necessary data for the user
        List<UserSiteVisit> userSiteVisits = _dbContext.UserSiteVisits
            .Include(s => s.Analysis)
            .Include(s => s.Site)
            .Where(u => u.UserId == UserId && u.VisitEndDate.HasValue && u.ProcessedAt.HasValue)
            .OrderBy(v => v.VisitStartDate) // Order by date to be safe
            .ToList();

        // 2. Group all visits by the calendar date (ignoring the time part)
        var visitsGroupedByDay = userSiteVisits.GroupBy(v => v.VisitStartDate.Date);

        // 3. Create a list to hold the final daily analytics
        var dailyAnalytics = new List<DailyAnalyticsDTO>();

        // 4. Process each day's group of visits
        foreach (var dayGroup in visitsGroupedByDay){
            var dailyDto = new DailyAnalyticsDTO{
                Date = dayGroup.Key.ToString("yyyy-MM-dd"),
                SiteVisits = new List<SiteVisitRecordDTO>()
            };

            // 5. Within each day, group by Site URL and aggregate the time
            var sitesVisitedOnThisDay = dayGroup
                .GroupBy(v => v.Site?.Url)
                .Select(siteGroup => {
                    // Ensure analysis and category exist before accessing
                    var firstVisit = siteGroup.First();
                    var analysis = firstVisit.Analysis;
                    string mainCategory = (analysis?.Category != null && analysis.Category.Any()) 
                                          ? analysis.Category[0] 
                                          : "Unknown";
                    float baseProductiveScore = (analysis != null)
                                                // ? (float)(analysis.IntrinsicScore * 0.5 * (0.5f + analysis.RelevanceScore))
                                                ? (float)analysis.IntrinsicScore
                                                : 0;

                    return new SiteVisitRecordDTO{
                        SiteUrl = siteGroup.Key,
                        // Sum the time spent for this site ON THIS DAY ONLY
                        TimeSpent = (float)siteGroup.Sum(v => (v.VisitEndDate.Value - v.VisitStartDate).TotalSeconds),
                        BaseProductiveScore = baseProductiveScore,
                        MainCategory = mainCategory
                    };
                }).ToList();
            
            dailyDto.SiteVisits = sitesVisitedOnThisDay;
            dailyAnalytics.Add(dailyDto);
        }

        return Ok(dailyAnalytics);
    }
    
    [Authorize]
    [HttpGet("Achievements")]
    public IActionResult GetUserAchievements() {
        var achievements = _dbContext.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == UserId)
            .OrderByDescending(ua => ua.EarnedAt)
            .Select(ua => new AchievementDTO
            {
                Title = ua.Achievement.Title,
                Description = ua.Achievement.Description,
                EarnedAt = ua.EarnedAt
            })
            .ToList();

        _logger.LogInformation($"Sent {achievements.Count} achievements to user.");
        return Ok(achievements);
    }
}
using System.Security.Claims;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
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
        return Ok(_dbContext.GameStats.Where(u => u.UserId == UserId));
    }
    
    [Authorize]
    [HttpGet("Analytics")]
    public IActionResult SendUserSiteVisits(){
        List<UserSiteVisit> userSiteVisits = _dbContext.UserSiteVisits
            .Include(s => s.Analysis)
            .Include(s => s.Site)
            .Where(u => u.UserId == UserId && u.VisitEndDate != null)
            .ToList();
        List<SiteVisitRecordDTO> siteVisitDtos = new();

        foreach (var visit in userSiteVisits){
            SiteVisitRecordDTO? sameSiteVisit = siteVisitDtos.FirstOrDefault(v => v.SiteUrl == visit.Site?.Url);
            if (sameSiteVisit != null){
                sameSiteVisit.TimeSpent += (float)(visit.VisitEndDate - visit.VisitStartDate).Value.TotalSeconds;
            }
            else{
                siteVisitDtos.Add(new SiteVisitRecordDTO{
                    SiteUrl = visit.Site.Url,
                    BaseProductiveScore = (float)(visit.Analysis.IntrinsicScore * 0.5 * visit.Analysis.RelevanceScore),
                    TimeSpent = (float)(visit.VisitEndDate - visit.VisitStartDate).Value.TotalSeconds,
                    MainCategory = visit.Analysis.Category[0],
                    VisitDate = visit.VisitStartDate,
                });
            }
        }

        _logger.LogInformation("{Score}", siteVisitDtos[0].BaseProductiveScore);
        return Ok(siteVisitDtos);
    }
}
using System.Security.Claims;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class DashboardController : ControllerBase{
    private readonly ProductivityDbContext _dbContext;
    
    // public string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserId => "4420f420-2f98-4cac-a1ab-578c3c2d4b19";

    public DashboardController(ProductivityDbContext dbContext){
        _dbContext = dbContext;
    }
    
    [HttpGet("UserStat")]
    public IActionResult SendUserStat(){
        Console.WriteLine("Sent user's stats");
        return Ok(_dbContext.GameStats.Where(u => u.UserId == UserId));
    }
    
    [HttpGet("Analytics")]
    public IActionResult SendUserSiteVisits(){
        List<UserSiteVisit> userSiteVisits = _dbContext.UserSiteVisits
            .Include(s => s.Analysis)
            .Include(s => s.Site)
            .Where(u => u.UserId == UserId && u.VisitEndDate != null)
            .ToList();
        List<SiteVisitDTO> siteVisitDtos = new();

        foreach (var visit in userSiteVisits){
            SiteVisitDTO? sameSiteVisit = siteVisitDtos.FirstOrDefault(v => v.SiteUrl == visit.Site?.Url);
            if (sameSiteVisit != null){
                sameSiteVisit.TimeSpent += (float)(visit.VisitEndDate - visit.VisitStartDate).Value.TotalSeconds;
            }
            else{
                siteVisitDtos.Add(new SiteVisitDTO{
                    SiteUrl = visit.Site.Url,
                    BaseProductiveScore = (float)(visit.Analysis.IntrinsicScore * 0.5 * visit.Analysis.RelevanceScore),
                    TimeSpent = (float)(visit.VisitEndDate - visit.VisitStartDate).Value.TotalSeconds,
                    MainCategory = visit.Analysis.Category[0]
                });
            }
        }

        Console.WriteLine(siteVisitDtos[0].BaseProductiveScore);
        return Ok(siteVisitDtos);
    }
}
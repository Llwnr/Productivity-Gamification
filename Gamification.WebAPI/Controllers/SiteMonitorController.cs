using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Gamification.Infrastructure.Externals;
using Gamification.Core.Models;
using Gamification.Core.Interfaces;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteMonitorController : ControllerBase{
    private readonly ISiteAnalysisService _siteAnalysisService;
    private readonly IInactivityRecordingService _inactivityRecordingService;
    
    public string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public SiteMonitorController(ISiteAnalysisService siteAnalysisService, IInactivityRecordingService inactivityRecordingService){
        _siteAnalysisService = siteAnalysisService;
        _inactivityRecordingService = inactivityRecordingService;
    }
    
    /// <summary>
    /// Will take in the site's information & user's goals then prompt the LLM to analyze site for productivity scores.
    /// </summary>
    [Authorize]
    [HttpGet("AnalyzeSite")]
    public async Task AnalyzeSite(string userGoal, string url, string title, string desc){
        bool success = await _siteAnalysisService.AnalyzeSite(userGoal, url, title, desc, UserId);
        if(success) Console.WriteLine("Successfully analyzed site");
        else Console.WriteLine("Failed to analyze site");
    }

    [Authorize]
    [HttpGet("BrowsingStopped")]
    public void NotifyBrowserClosed(){
        return;
        Console.WriteLine("User has stopped browsing.");
        if (!string.IsNullOrWhiteSpace(UserId)){
            _inactivityRecordingService.RecordAsInactive(UserId);
        }
    }

    [Authorize]
    [HttpGet("BrowserCrashed")]
    public void RecordLastActiveState(string lastActiveTimeStr){
        if (!string.IsNullOrWhiteSpace(UserId)){
            if (DateTime.TryParse(lastActiveTimeStr, out var lastActiveTime)){
                lastActiveTime = lastActiveTime.ToUniversalTime();
                Console.WriteLine("Crashed time: " + lastActiveTime + " Curr time:" + DateTime.UtcNow);
                _inactivityRecordingService.RecordAsInactive(UserId, lastActiveTime);
            }
            Console.Error.WriteLine("Failed to parse time.");
        }
    }

    [HttpGet("TestSite")]
    public void LogRandom(){
        Console.WriteLine("Working");
    }
}
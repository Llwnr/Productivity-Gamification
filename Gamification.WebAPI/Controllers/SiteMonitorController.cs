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
    private readonly IAnalysisQueryManager _analysisQueryManager;
    private readonly IInactivityRecordingService _inactivityRecordingService;
    
    public string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public SiteMonitorController(IAnalysisQueryManager analysisQueryManager, IInactivityRecordingService inactivityRecordingService){
        _analysisQueryManager = analysisQueryManager;
        _inactivityRecordingService = inactivityRecordingService;
    }
    
    /// <summary>
    /// Will take in the site's information & user's goals then prompt the LLM to analyze site for productivity scores.
    /// </summary>
    [Authorize]
    [HttpPost("AnalyzeSite")]
    public void AnalyzeSite([FromBody] Prompt siteVisitDetails){
        Console.WriteLine("Received request to analyze site.");
        _analysisQueryManager.EnqueueAnalysisQuery(siteVisitDetails, UserId);
    }

    [Authorize]
    [HttpGet("BrowsingStopped")]
    public void NotifyBrowserClosed(){
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
                _inactivityRecordingService.RecordAsInactive(UserId, lastActiveTime);
                return;
            }
            Console.Error.WriteLine("Failed to parse time.");
        }
    }

    [HttpGet("Talk")]
    public void LogRandom(string msg){
        Console.WriteLine(msg);
    }
}
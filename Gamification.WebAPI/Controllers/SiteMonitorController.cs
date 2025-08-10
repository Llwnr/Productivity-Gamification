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
    [HttpGet("AnalyzeSite")]
    public void AnalyzeSite(string userGoal, string url, string title, string desc){
        Prompt prompt = new Prompt(userGoal, url, title, desc);
        _analysisQueryManager.EnqueueAnalysisQuery(prompt, UserId);
    }

    [Authorize]
    [HttpGet("BrowsingStopped")]
    public void NotifyBrowserClosed(){
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
                _inactivityRecordingService.RecordAsInactive(UserId, lastActiveTime);
                return;
            }
            Console.Error.WriteLine("Failed to parse time.");
        }
    }

    [HttpGet("TestSite")]
    public void LogRandom(){
        Console.WriteLine("Working");
    }
}
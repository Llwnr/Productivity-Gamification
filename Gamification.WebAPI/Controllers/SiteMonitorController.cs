using System.Security.Claims;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Gamification.Infrastructure.Externals;
using Gamification.Core.Models;
using Gamification.Core.Interfaces;
using Gamification.Infrastructure.Interfaces;
using Gamification.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteMonitorController : ControllerBase{
    private readonly IAnalysisQueryManager _analysisQueryManager;
    private readonly IInactivityRecordingService _inactivityRecordingService;
    private readonly IActivityRecorder _activityRecorder;
    private readonly ILogger<SiteMonitorController> _logger;
    
    private string? GetAuthorizedUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public SiteMonitorController(
        IAnalysisQueryManager analysisQueryManager, 
        IInactivityRecordingService inactivityRecordingService,
        IActivityRecorder activityRecorder,
        ILogger<SiteMonitorController> logger){
        _analysisQueryManager = analysisQueryManager;
        _inactivityRecordingService = inactivityRecordingService;
        _activityRecorder = activityRecorder;
        _logger = logger;
    }
    
    /// <summary>
    /// Will take in the site's information & user's goals then prompt the LLM to analyze site for productivity scores.
    /// </summary>
    [Authorize]
    [HttpPost("AnalyzeSite")]
    public IActionResult AnalyzeSite([FromBody] SiteVisitDTO siteVisitDetails){
        _logger.LogInformation("Received request to analyze site.");
        Prompt prompt = new Prompt{
            Url = siteVisitDetails.Url,
            Title = siteVisitDetails.Title,
            Description = siteVisitDetails.Description,
            UserId = GetAuthorizedUserId
        };
        _analysisQueryManager.EnqueueAnalysisQuery(prompt);
        Site site = new Site{
            Url = siteVisitDetails.Url,
            Title = siteVisitDetails.Title,
            Description = siteVisitDetails.Description
        };
        _activityRecorder.AddSiteVisit(site, GetAuthorizedUserId);
        return Ok("Received");
    }

    [Authorize]
    [HttpGet("BrowsingStopped")]
    public void NotifyBrowserClosed(){
        if (!string.IsNullOrWhiteSpace(GetAuthorizedUserId)){
            _inactivityRecordingService.EndVisit(GetAuthorizedUserId);
        }
    }

    [Authorize]
    [HttpGet("BrowserCrashed")]
    public void RecordLastActiveState(string lastActiveTimeStr){
        if (!string.IsNullOrWhiteSpace(GetAuthorizedUserId)){
            if (DateTime.TryParse(lastActiveTimeStr, out var lastActiveTime)){
                lastActiveTime = lastActiveTime.ToUniversalTime();
                _inactivityRecordingService.EndVisit(GetAuthorizedUserId, lastActiveTime);
                _logger.LogInformation("Last active datetime: {LastActiveTime}", lastActiveTime);
                return;
            }
            Console.Error.WriteLine("Failed to parse time.");
        }
    }

    [Authorize]
    [HttpPost("ChangeVisit")]
    public void OnVisitChanged(){
        _inactivityRecordingService.EndVisit(GetAuthorizedUserId);
    }

    [HttpGet("Talk")]
    public void LogRandom(string msg){
        _logger.LogInformation(msg);
    }
}
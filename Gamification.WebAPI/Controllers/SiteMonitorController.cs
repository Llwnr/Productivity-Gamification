using Microsoft.AspNetCore.Mvc;
using Gamification.Infrastructure.Externals;
using Gamification.Core.Models;
using Gamification.Core.Interfaces;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Services;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteMonitorController : ControllerBase{
    private readonly ISiteAnalysisService _siteAnalysisService;
    
    public SiteMonitorController(ISiteAnalysisService siteAnalysisService){
        _siteAnalysisService = siteAnalysisService;
    }
    
    /// <summary>
    /// Will take in the site's information & user's goals then prompt the LLM to analyze site for productivity scores.
    /// </summary>
    [HttpGet("AnalyzeSite")]
    public async Task AnalyzeSite(string userGoal, string url, string title, string desc){
        bool success = await _siteAnalysisService.AnalyzeSite(userGoal, url, title, desc);
        if(success) Console.WriteLine("Successfully analyzed site");
        else Console.WriteLine("Failed to analyze site");
    }

    [HttpGet("BrowsingStopped")]
    public void NotifyBrowserClosed(){
        Console.WriteLine("User has stopped browsing.");
    }
}
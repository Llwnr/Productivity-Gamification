using Microsoft.AspNetCore.Mvc;
using Gamification.Infrastructure.Externals;
using Gamification.Core.Models;
using Gamification.Core.Interfaces;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteMonitorController : ControllerBase{
    private readonly IProductivityService _productivityService;
    
    public SiteMonitorController(IProductivityService service){
        _productivityService = service;
    }
    
    /// <summary>
    /// Will take in the site's information & user's goals then prompt the LLM to analyze site for productivity scores.
    /// </summary>
    [HttpGet("AnalyzeSite")]
    public async Task AnalyzeSite(string userGoal, string url, string title, string desc){
        string fullPrompt = $"\nUserGoal: {userGoal}\nSiteUrl: {url}\nDescription: {desc}\nTitle:{title}.";
        Console.WriteLine("Url browsed: " + url);

        GoogleApi googleApi = new GoogleApi();
        try{
            SiteAnalysis? analysis = await googleApi.Generate(fullPrompt);
            if (analysis != null){
                float finalScore = _productivityService.GetFinalScore(analysis.IntrinsicScore, analysis.RelevanceScore);
                Console.WriteLine($"Score: {finalScore}");
                Console.WriteLine($"Visited time: {DateTime.Now}");
            }
            else{
                Console.Error.WriteLine("Analysis is null");
            }
        }
        catch (Exception e){
            await Console.Error.WriteLineAsync("Exception while generating site analysis response from LLM: \n" + e);
            throw;
        }
    }

    [HttpGet("BrowserClosed")]
    public void NotifyBrowserClosed(){
        Console.WriteLine("Browser is closed");
    }
    
    public void TrackDurationOnUrl(string url){
        Console.WriteLine("Tracking duration on: " + url);
    }
}
using Microsoft.AspNetCore.Mvc;
using System;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SiteMonitorController : ControllerBase{
    [HttpGet("SiteBrowsed")]
    public void GetSiteUrl(string url, string title, string desc){
        Console.WriteLine($"Title: {title}");
        Console.WriteLine($"Description: {desc}");
        Console.WriteLine("Url browsed: " + url);
        Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------");
    }

    [HttpGet("BrowserClosed")]
    public void OnBrowserClosed(){
        Console.WriteLine("Browser is closed");
    }

    [HttpGet("ExtensionState")]
    public void GetExtensionState(bool value){
        //Note that this value is never false
        if (value){
            Console.WriteLine("Extension is alive, don't stop increasing the time");
        }
    }
    public void TrackDurationOnUrl(string url){
        Console.WriteLine("Tracking duration on: " + url);
    }
}
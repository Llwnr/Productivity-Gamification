using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Gamification.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class SiteAnalysisService : ISiteAnalysisService{
    private readonly ProductivityDbContext _dbContext;
    private readonly IContentAnalysisFilter _analysisFilter;
    private readonly GoogleApi _googleApi;
    private readonly IInactivityRecordingService _inactivityRecordingService;
    private readonly ILogger<SiteAnalysisService> _logger;

    public SiteAnalysisService(
        IActivityProcessingService activityProcessingService, 
        ProductivityDbContext dbContext, 
        IContentAnalysisFilter analysisFilter, 
        GoogleApi googleApi,
        IInactivityRecordingService inactivityRecordingService,
        ILogger<SiteAnalysisService> logger){
        _dbContext = dbContext;
        _analysisFilter = analysisFilter;
        _googleApi = googleApi;
        _inactivityRecordingService = inactivityRecordingService;
        _logger = logger;
    }
    
    public async Task<bool> AnalyzeSites(List<Prompt> prompts){
        prompts = prompts.Distinct().ToList();
        if (prompts.Count <= 0){
            _logger.LogInformation("Prompt is empty");
            return false;
        }
        //Clear up prompts that have already been analyzed
        ClearRedundantPrompts(prompts);

        List<SiteAnalysis>? analysisResults = (await _googleApi.Generate(prompts.ToList())).Analyses;
        _logger.LogInformation("Total no. of analyses: " + analysisResults.Count);
        if (analysisResults == null || analysisResults.Count <= 0){
            _logger.LogInformation("Error, analysis result is empty");
            return false;
        }
        
        for (int i = 0; i < analysisResults.Count; i++){
            Prompt prompt = prompts[i];
            User user = _dbContext.Users.First(u => u.UserId == prompt.UserId);
            try{
                string associatedUserId = prompt.UserId;
                _logger.LogInformation("Performing analysis");
                SiteAnalysis? analysis = analysisResults[i];
                //Set previous site visit as inactive because site/tab has been switched.
                _inactivityRecordingService.EndVisit(associatedUserId, DateTime.UtcNow);
                // float finalScore = _scoreProcessingService.GetFinalScore(analysis.IntrinsicScore, analysis.RelevanceScore);
                // Console.WriteLine($"Score: {finalScore}");
                Site? site = _dbContext.Sites.FirstOrDefault(s => s.Url == prompt.Url && s.Title == prompt.Title);
                if (site == null){
                    _logger.LogInformation("Analysis for the site that HASN'T BEEN RECORDED is requested.");
                    continue;
                }
                AnalysisResult result = new AnalysisResult{
                    Category = analysis.Category,
                    IntrinsicScore = analysis.IntrinsicScore,
                    RelevanceScore = analysis.RelevanceScore,
                    Site = site,
                    SiteId = site.SiteId,
                    UserGoal = user.Goal
                };
                if (_dbContext.AnalysisResults.Where(ar =>
                        ar.SiteId == result.SiteId && ar.UserGoal == result.UserGoal).ToList().Count <= 0){
                    _dbContext.AnalysisResults.Add(result);
                }
                _dbContext.SaveChanges();
                _logger.LogInformation($"Successfully added site {prompt.Title} to database");
            }
            catch (Exception e){
                _logger.LogError("Exception while generating site analysis response from LLM: \n" + e);
                throw;
            }
            if (i + 1 >= analysisResults.Count) return true;
        }
        // if (!_analysisFilter.IsAnalysisRequired(prompt.Description)){
        //     Console.WriteLine("Skipping analysis");
        //     return false;
        // }

        return false;
    }

    //Checks whether the site is already analyzed, if yes uses that analysis for scoring instead of querying LLM again.
    private bool TryGetCachedAnalysis(string url, string? userGoal, out AnalysisResult result){
        result = new AnalysisResult();
        _logger.LogInformation("Searching for the url in storage: ");
        AnalysisResult? cachedResult = _dbContext.AnalysisResults
            .Include(ar => ar.Site)
            .Where(ar => ar.Site.Url == url && ar.UserGoal == userGoal)
            .FirstOrDefault();

        if (cachedResult == null){
            return false;
        }

        result = cachedResult;
        return true;
    }

    void ClearRedundantPrompts(List<Prompt> prompts){
        for(int i=0; i<prompts.Count; i++){
            User? user = _dbContext.Users.FirstOrDefault(u => u.UserId == prompts[i].UserId);
            if (TryGetCachedAnalysis(prompts[i].Url, user.Goal, out var result)){
                _logger.LogInformation($"Found in database.");
                // _inactivityRecordingService.EndVisit(userId, visitTime);
                
                prompts.RemoveAt(i);
                i--;
            }
        }
        if (prompts.Count <= 0){
            _logger.LogInformation("All prompts of a batch have been cached.");
        }
    }
}
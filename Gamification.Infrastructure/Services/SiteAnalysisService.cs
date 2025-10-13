using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Gamification.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Infrastructure.Services;

public class SiteAnalysisService : ISiteAnalysisService{
    private readonly IScoreProcessingService _scoreProcessingService;
    private readonly ProductivityDbContext _dbContext;
    private readonly IContentAnalysisFilter _analysisFilter;
    private readonly GoogleApi _googleApi;
    private readonly IInactivityRecordingService _inactivityRecordingService;

    public SiteAnalysisService(
        IScoreProcessingService scoreProcessingService, 
        ProductivityDbContext dbContext, 
        IContentAnalysisFilter analysisFilter, 
        GoogleApi googleApi,
        IInactivityRecordingService inactivityRecordingService){
        _scoreProcessingService = scoreProcessingService;
        _dbContext = dbContext;
        _analysisFilter = analysisFilter;
        _googleApi = googleApi;
        _inactivityRecordingService = inactivityRecordingService;
    }
    
    public async Task<bool> AnalyzeSites(List<Prompt> prompts){
        prompts = prompts.Distinct().ToList();
        if (prompts.Count <= 0){
            Console.WriteLine("Prompt is empty");
            return false;
        }
        //Clear up prompts that have already been analyzed
        ClearRedundantPrompts(prompts);

        List<SiteAnalysis>? analysisResults = (await _googleApi.Generate(prompts.ToList())).Analyses;
        Console.WriteLine("Total no. of analyses: " + analysisResults.Count);
        if (analysisResults == null || analysisResults.Count <= 0){
            Console.WriteLine("Error, analysis result is empty");
            return false;
        }
        
        for (int i = 0; i < analysisResults.Count; i++){
            Prompt prompt = prompts[i];
            User user = _dbContext.Users.First(u => u.UserId == prompt.UserId);
            string fullPrompt = $"User goal: {user.Goal} {prompt}";
            try{
                string associatedUserId = prompt.UserId;
                Console.WriteLine("Performing analysis");
                SiteAnalysis? analysis = analysisResults[i];
                //Set previous site visit as inactive because site/tab has been switched.
                _inactivityRecordingService.EndVisit(associatedUserId, DateTime.UtcNow);
                // float finalScore = _scoreProcessingService.GetFinalScore(analysis.IntrinsicScore, analysis.RelevanceScore);
                // Console.WriteLine($"Score: {finalScore}");
                Site site = _dbContext.Sites.First(s => s.Url == prompt.Url && s.Title == prompt.Title);
                AnalysisResult result = new AnalysisResult{
                    Category = analysis.Category,
                    IntrinsicScore = analysis.IntrinsicScore,
                    RelevanceScore = analysis.RelevanceScore,
                    Site = site,
                    UserGoal = user.Goal
                };
                _dbContext.AnalysisResults.Add(result);
                _dbContext.SaveChanges();

                Console.WriteLine($"Successfully added site {prompt.Title} to database");
            }
            catch (Exception e){
                await Console.Error.WriteLineAsync("Exception while generating site analysis response from LLM: \n" + e);
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
        Console.WriteLine("Searching for the url in storage: ");
        List<AnalysisResult>? cachedResults = _dbContext.AnalysisResults
            .Include(ar => ar.Site)
            .Where(ar => ar.Site.Url == url).ToList();
        
        foreach (var cachedResult in cachedResults){
            if (cachedResult.UserGoal == userGoal){
                result = cachedResult;
                return true;
            }
        }

        return false;
    }

    void ClearRedundantPrompts(List<Prompt> prompts){
        for(int i=0; i<prompts.Count; i++){
            User? user = _dbContext.Users.FirstOrDefault(u => u.UserId == prompts[i].UserId);
            if (TryGetCachedAnalysis(prompts[i].Url, user.Goal, out var result)){
                Console.WriteLine($"Found in database.");
                // _inactivityRecordingService.EndVisit(userId, visitTime);
                
                prompts.RemoveAt(i);
                i--;
            }
        }
        if (prompts.Count <= 0){
            Console.WriteLine("All prompts of a batch have been cached.");
        }
    }
}
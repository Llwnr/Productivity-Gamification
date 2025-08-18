using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Gamification.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Infrastructure.Services;

public class SiteAnalysisService : ISiteAnalysisService{
    private readonly IScoreCalculationService _scoreCalculationService;
    private readonly ProductivityDbContext _dbContext;
    private readonly IContentAnalysisFilter _analysisFilter;

    public SiteAnalysisService(IScoreCalculationService scoreCalculationService, ProductivityDbContext dbContext, IContentAnalysisFilter analysisFilter){
        _scoreCalculationService = scoreCalculationService;
        _dbContext = dbContext;
        _analysisFilter = analysisFilter;
    }
    
    public async Task<bool> AnalyzeSite(Prompt prompt, string userId, DateTime visitTime){
        string fullPrompt = prompt.ToString();
        Console.WriteLine("Site browsed: " + prompt.Title);

        if (!_analysisFilter.IsAnalysisRequired(prompt.Description)){
            Console.WriteLine("Skipping analysis");
            return false;
        }
        
        if (TryGetCachedAnalysis(prompt.Url, prompt.UserGoal, out AnalysisResult analysisResult)){
            Console.WriteLine($"Found in database.");
            UserSiteVisit siteVisit = new UserSiteVisit{
                UserId = userId,
                Analysis = analysisResult,
                Site = analysisResult.Site,
                VisitDate = visitTime
            };

            _dbContext.Add(siteVisit);
            _dbContext.SaveChanges();
            
            return true;
        }

        GoogleApi googleApi = new GoogleApi();
        try{
            SiteAnalysis? analysis = await googleApi.Generate(fullPrompt);
            if (analysis != null){
                float finalScore = _scoreCalculationService.GetFinalScore(analysis.IntrinsicScore, analysis.RelevanceScore);
                Console.WriteLine($"Score: {finalScore}");

                Site site = new Site{
                    Url = prompt.Url,
                    Title = prompt.Title,
                    Description = prompt.Description
                };
                
                AnalysisResult result = new AnalysisResult{
                    Category = analysis.Category,
                    IntrinsicScore = analysis.IntrinsicScore,
                    RelevanceScore = analysis.RelevanceScore,
                    Site = site,
                    UserGoal = prompt.UserGoal
                };

                UserSiteVisit siteVisit = new UserSiteVisit{
                    UserId = userId,
                    Analysis = result,
                    Site = site,
                    VisitDate = visitTime
                };
                
                _dbContext.Add(site);
                _dbContext.Add(result);
                _dbContext.Add(siteVisit);
                _dbContext.SaveChanges();

                Console.WriteLine($"Successfully added site {prompt.Title} to database");

                return true;
            }
            return false;
        }
        catch (Exception e){
            await Console.Error.WriteLineAsync("Exception while generating site analysis response from LLM: \n" + e);
            throw;
        }
    }

    //Checks whether the site is already analyzed, if yes uses that analysis for scoring instead of querying LLM again.
    private bool TryGetCachedAnalysis(string url, string userGoal, out AnalysisResult result){
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
}
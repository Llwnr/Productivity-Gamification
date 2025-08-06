using Gamification.Core.Interfaces;
using Gamification.Core.Models;
using Gamification.Infrastructure.DatabaseService;
using Gamification.Infrastructure.Externals;
using Microsoft.EntityFrameworkCore;

namespace Gamification.Infrastructure.Services;

public class SiteAnalysisService : ISiteAnalysisService{
    private readonly IScoreCalculationService _scoreCalculationService;
    private readonly ProductivityDbContext _dbContext;

    public SiteAnalysisService(IScoreCalculationService scoreCalculationService, ProductivityDbContext dbContext){
        _scoreCalculationService = scoreCalculationService;
        _dbContext = dbContext;
    }
    
    public async Task<bool> AnalyzeSite(string userGoal, string url, string title, string desc, string userId){
        string fullPrompt = $"\nUserGoal: {userGoal}\nSiteUrl: {url}\nDescription: {desc}\nTitle:{title}.";
        Console.WriteLine("Url browsed: " + url);
        
        if (TryGetCachedAnalysis(url, userGoal, out AnalysisResult analysisResult)){
            Console.WriteLine($"Found {analysisResult.Site?.Url}");
            UserSiteVisit siteVisit = new UserSiteVisit{
                UserId = userId,
                Analysis = analysisResult,
                Site = analysisResult.Site,
                VisitDate = DateTime.UtcNow
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
                Console.WriteLine($"Visited time: {DateTime.Now}");

                Console.WriteLine($"Writing to database");

                // return true;

                Site site = new Site{
                    Url = url,
                    Title = title,
                    Description = desc
                };
                
                AnalysisResult result = new AnalysisResult{
                    Category = analysis.Category,
                    IntrinsicScore = analysis.IntrinsicScore,
                    RelevanceScore = analysis.RelevanceScore,
                    Site = site,
                    UserGoal = userGoal
                };

                UserSiteVisit siteVisit = new UserSiteVisit{
                    UserId = userId,
                    Analysis = result,
                    Site = site,
                    VisitDate = DateTime.UtcNow
                };
                
                _dbContext.Add(site);
                _dbContext.Add(result);
                _dbContext.Add(siteVisit);
                _dbContext.SaveChanges();

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
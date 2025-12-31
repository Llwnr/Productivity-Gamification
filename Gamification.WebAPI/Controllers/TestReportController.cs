using System.Text;
using Gamification.Core.Models;
using Gamification.Infrastructure.Externals;
using Microsoft.AspNetCore.Mvc;

namespace Gamification.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TestReportController : ControllerBase
{
    private readonly GoogleApi _googleApi;
    private readonly ILogger<TestReportController> _logger;

    public TestReportController(GoogleApi googleApi, ILogger<TestReportController> logger)
    {
        _googleApi = googleApi;
        _logger = logger;
    }

    [HttpGet("GenerateF1Report")]
    public async Task<IActionResult> GenerateReport() {
        return Ok("Skipped");
        _logger.LogInformation("Starting F1 Score Analysis on 50 Sites...");
        
        // 1. Define Ground Truth Data (50 Sites)
        // True = Productive, False = Unproductive
        var testSet = GetTestSet();

        var sb = new StringBuilder();
        sb.AppendLine("APPENDIX A: FULL TEST DATASET FOR CLASSIFICATION ACCURACY");
        sb.AppendLine("-------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine(String.Format("| {0,-3} | {1,-40} | {2,-10} | {3,-15} | {4,-15} | {5,-10} |", "#", "URL", "Score", "AI Class", "Actual", "Result"));
        sb.AppendLine("-------------------------------------------------------------------------------------------------------------------");

        int truePositives = 0;
        int trueNegatives = 0;
        int falsePositives = 0;
        int falseNegatives = 0;

        // 2. Process in batches of 5 to avoid overloading the LLM context window
        int batchSize = 25;
        for (int i = 0; i < testSet.Count; i += batchSize)
        {
            var batch = testSet.Skip(i).Take(batchSize).ToList();
            var prompts = batch.Select(item => new Prompt
            {
                Url = item.Url,
                Title = item.Title,
                Description = item.Description,
                UserId = "TEST_USER"
            }).ToList();

            _logger.LogInformation($"Processing batch {i/batchSize + 1} of {testSet.Count/batchSize}...");

            try
            {
                // Call your existing Google API service
                var result = await _googleApi.Generate(prompts);

                if (result?.Analyses == null || result.Analyses.Count != batch.Count)
                {
                    sb.AppendLine($"ERROR: Batch {i} failed or returned mismatching count.");
                    continue;
                }

                for (int j = 0; j < batch.Count; j++)
                {
                    var input = batch[j];
                    var analysis = result.Analyses[j];

                    // Determine AI Classification (Threshold > 50 is Productive)
                    bool aiIsProductive = analysis.IntrinsicScore >= 50;
                    string aiClass = aiIsProductive ? "Productive" : "Unproductive";
                    string actualClass = input.IsProductive ? "Productive" : "Unproductive";

                    // Determine Result Type
                    string resultType;
                    if (aiIsProductive && input.IsProductive) { resultType = "TP"; truePositives++; }
                    else if (!aiIsProductive && !input.IsProductive) { resultType = "TN"; trueNegatives++; }
                    else if (aiIsProductive && !input.IsProductive) { resultType = "FP"; falsePositives++; }
                    else { resultType = "FN"; falseNegatives++; }

                    // Log the row
                    sb.AppendLine(String.Format("| {0,-3} | {1,-40} | {2,-10} | {3,-15} | {4,-15} | {5,-10} |", 
                        (i + j + 1), 
                        input.Url.Length > 37 ? input.Url.Substring(0, 37) + "..." : input.Url, 
                        analysis.IntrinsicScore, 
                        aiClass, 
                        actualClass, 
                        resultType));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch");
                sb.AppendLine($"EXCEPTION in batch: {ex.Message}");
            }

            // Small delay to be nice to the API
            await Task.Delay(1000);
        }

        // 3. Calculate Final Metrics
        double precision = (double)truePositives / (truePositives + falsePositives);
        double recall = (double)truePositives / (truePositives + falseNegatives);
        double f1 = 2 * ((precision * recall) / (precision + recall));
        double accuracy = (double)(truePositives + trueNegatives) / testSet.Count;

        sb.AppendLine("-------------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("");
        sb.AppendLine("=== FINAL RESULTS ===");
        sb.AppendLine($"Total Sites: {testSet.Count}");
        sb.AppendLine($"True Positives (TP): {truePositives}");
        sb.AppendLine($"True Negatives (TN): {trueNegatives}");
        sb.AppendLine($"False Positives (FP): {falsePositives}");
        sb.AppendLine($"False Negatives (FN): {falseNegatives}");
        sb.AppendLine("");
        sb.AppendLine($"Accuracy:  {accuracy:P2}");
        sb.AppendLine($"Precision: {precision:F4}");
        sb.AppendLine($"Recall:    {recall:F4}");
        sb.AppendLine($"F1 Score:  {f1:F4}");

        // 4. Save to File
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "Report_Output.txt");
        await System.IO.File.WriteAllTextAsync(outputPath, sb.ToString());

        _logger.LogInformation($"Report generated at: {outputPath}");

        return Ok(new { Message = "Report Generated", Path = outputPath, Content = sb.ToString() });
    }

    // Helper class for ground truth
    private class TestSite
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsProductive { get; set; }
    }

    private List<TestSite> GetTestSet()
    {
        return new List<TestSite>
        {
            // --- PRODUCTIVE (Development & Education) ---
            new() { Url = "stackoverflow.com", Title = "Stack Overflow", Description = "Q&A for programmers", IsProductive = true },
            new() { Url = "github.com", Title = "GitHub", Description = "Code hosting platform", IsProductive = true },
            new() { Url = "learn.microsoft.com", Title = "Microsoft Learn", Description = "Documentation for .NET", IsProductive = true },
            new() { Url = "w3schools.com", Title = "W3Schools", Description = "Web dev tutorials", IsProductive = true },
            new() { Url = "udemy.com", Title = "Udemy", Description = "Online courses", IsProductive = true },
            new() { Url = "coursera.org", Title = "Coursera", Description = "Online degrees", IsProductive = true },
            new() { Url = "chatgpt.com", Title = "ChatGPT", Description = "AI Assistant", IsProductive = true },
            new() { Url = "claude.ai", Title = "Claude", Description = "AI Assistant", IsProductive = true },
            new() { Url = "developer.mozilla.org", Title = "MDN Web Docs", Description = "Web documentation", IsProductive = true },
            new() { Url = "geeksforgeeks.org", Title = "GeeksforGeeks", Description = "CS tutorials", IsProductive = true },
            new() { Url = "jira.atlassian.com", Title = "Jira", Description = "Project Tracking", IsProductive = true },
            new() { Url = "trello.com", Title = "Trello", Description = "Kanban boards", IsProductive = true },
            new() { Url = "notion.so", Title = "Notion", Description = "Note taking and productivity", IsProductive = true },
            new() { Url = "figma.com", Title = "Figma", Description = "UI Design tool", IsProductive = true },
            new() { Url = "canva.com", Title = "Canva", Description = "Design tool", IsProductive = true },
            new() { Url = "scholar.google.com", Title = "Google Scholar", Description = "Academic search", IsProductive = true },
            new() { Url = "researchgate.net", Title = "ResearchGate", Description = "Scientific network", IsProductive = true },
            new() { Url = "arxiv.org", Title = "ArXiv", Description = "Scientific papers", IsProductive = true },
            new() { Url = "kaggle.com", Title = "Kaggle", Description = "Data Science competitions", IsProductive = true },
            new() { Url = "leetcode.com", Title = "LeetCode", Description = "Coding interview prep", IsProductive = true },
            new() { Url = "hackerrank.com", Title = "HackerRank", Description = "Coding practice", IsProductive = true },
            new() { Url = "pluralsight.com", Title = "Pluralsight", Description = "Tech skills platform", IsProductive = true },
            new() { Url = "docs.google.com", Title = "Google Docs", Description = "Word processor", IsProductive = true },
            new() { Url = "sheets.google.com", Title = "Google Sheets", Description = "Spreadsheets", IsProductive = true },
            new() { Url = "slack.com", Title = "Slack", Description = "Work communication", IsProductive = true },

            // --- UNPRODUCTIVE (Entertainment, Social, Shopping) ---
            new() { Url = "facebook.com", Title = "Facebook", Description = "Social Media", IsProductive = false },
            new() { Url = "instagram.com", Title = "Instagram", Description = "Photo sharing", IsProductive = false },
            new() { Url = "tiktok.com", Title = "TikTok", Description = "Short videos", IsProductive = false },
            new() { Url = "twitter.com", Title = "X / Twitter", Description = "Social Media", IsProductive = false },
            new() { Url = "reddit.com", Title = "Reddit", Description = "Front page of internet", IsProductive = false },
            new() { Url = "netflix.com", Title = "Netflix", Description = "Streaming movies", IsProductive = false },
            new() { Url = "hulu.com", Title = "Hulu", Description = "Streaming TV", IsProductive = false },
            new() { Url = "disneyplus.com", Title = "Disney+", Description = "Streaming", IsProductive = false },
            new() { Url = "twitch.tv", Title = "Twitch", Description = "Game streaming", IsProductive = false },
            new() { Url = "youtube.com", Title = "YouTube", Description = "Video sharing", IsProductive = false }, // Often classified as productive by AI, but we mark false to test FP
            new() { Url = "steam.com", Title = "Steam", Description = "Video game store", IsProductive = false },
            new() { Url = "roblox.com", Title = "Roblox", Description = "Game platform", IsProductive = false },
            new() { Url = "ign.com", Title = "IGN", Description = "Game reviews", IsProductive = false },
            new() { Url = "gamespot.com", Title = "GameSpot", Description = "Video game news", IsProductive = false },
            new() { Url = "amazon.com", Title = "Amazon", Description = "Shopping", IsProductive = false },
            new() { Url = "ebay.com", Title = "eBay", Description = "Auction site", IsProductive = false },
            new() { Url = "etsy.com", Title = "Etsy", Description = "Handmade goods", IsProductive = false },
            new() { Url = "shein.com", Title = "Shein", Description = "Fast fashion", IsProductive = false },
            new() { Url = "temu.com", Title = "Temu", Description = "Shopping", IsProductive = false },
            new() { Url = "9gag.com", Title = "9GAG", Description = "Memes and fun", IsProductive = false },
            new() { Url = "buzzfeed.com", Title = "BuzzFeed", Description = "Entertainment news", IsProductive = false },
            new() { Url = "tmz.com", Title = "TMZ", Description = "Celebrity news", IsProductive = false },
            new() { Url = "dailymail.co.uk", Title = "Daily Mail", Description = "Tabloid news", IsProductive = false },
            new() { Url = "espn.com", Title = "ESPN", Description = "Sports news", IsProductive = false },
            new() { Url = "bleacherreport.com", Title = "Bleacher Report", Description = "Sports culture", IsProductive = false }
        };
    }
}
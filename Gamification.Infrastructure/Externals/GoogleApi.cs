using System.Diagnostics;
using GenerativeAI;
using Gamification.Core.Models;
using DotNetEnv;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Externals;

public class GoogleApi{
    private readonly GoogleAi? _googleAi;
    private readonly ILogger<GoogleApi> _logger;

    public GoogleApi(ILogger<GoogleApi> logger){
        _logger = logger;
        Env.Load();
        string apiKey = Env.GetString("GEMINI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("Please set the google api key environment variable.");
            return;
        }
        
        _googleAi = new GoogleAi(apiKey);
    }

    public async Task<SiteAnalysisList?> Generate(List<Prompt> prompts){
        if (_googleAi == null){
            _logger.LogWarning("Google AI Api is not set up");
            return null;
        }

        StringBuilder combinedPromptBuilder = new StringBuilder();
        for (int i = 0; i < prompts.Count; i++){
            combinedPromptBuilder.Append($"\nVisit {i+1}:" + prompts[i]);
        }
        string combinedPrompt = combinedPromptBuilder.ToString();
        _logger.LogInformation($"Processing prompts: \n {combinedPrompt}");
        
        var model = _googleAi.CreateGenerativeModel(GoogleAIModels.Gemini25FlashLite);

        model.SystemInstruction =
            "You are an expert web analyst. " +
            "Your task is to perform a two-part analysis " +
            "of multiple website visits based on the specific page title, " +
            "the general site content, and the user's goal for each visit." +
            "This object must contain a key named 'Analyses' which holds an array of analysis objects. " +
            "Each object in the array must correspond to one of the sites from the prompt and have the following structure:" +
            "\n{" +
            "\nCategory: string[] => The categories of the site. Must be one of: Learning, Creation, Research, Social, Entertainment, News, Technology, Other. Can have multiple categories" +
            "\nIntrinsicScore: integer => Inherent productivity value of the site on a fixed integer scale (1–100)." +
            "\nRelevanceScore: float => Relevance score (0.0–1.0) indicating the relevance of the activity to the user's goal." +
            "\n}" +
            "\nDo not return anything other than the single, valid JSON object.";
        
        var analysisResults = await model.GenerateObjectAsync<SiteAnalysisList>(combinedPrompt);
        return analysisResults;
    }
}
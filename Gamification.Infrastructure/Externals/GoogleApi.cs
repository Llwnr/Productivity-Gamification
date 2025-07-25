using System.Diagnostics;
using GenerativeAI;
using Gamification.Core.Models;
using DotNetEnv;
using Gamification.Infrastructure.Interfaces;

namespace Gamification.Infrastructure.Externals;

public class GoogleApi{
    private readonly GoogleAi? _googleAi;

    public GoogleApi(){
        Env.Load();
        string apiKey = Env.GetString("GEMINI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Please set the google api key environment variable.");
            return;
        }
        
        _googleAi = new GoogleAi(apiKey);
    }

    public async Task<SiteAnalysis?> Generate(string prompt){
        if (_googleAi == null){
            Console.WriteLine("Google AI Api is not set up");
            return null;
        }
        
        var model = _googleAi.CreateGenerativeModel(GoogleAIModels.Gemini2Flash);
        model.SystemInstruction =
            "You are an expert web analyst. " +
            "Your task is to perform a two-part analysis " +
            "of a website visit based on the specific page title, " +
            "the general site content, and the user's goal." +
            "\nYou must provide:\ncategory, justification, intrinsicScore, relevanceScore. " +
            "As per the descriptions in the FormatOutputStructure tool." +
            "\nUse the `FormatOutputStructure` tool to structure your response.";
        
        AnalysisStructureTool analysisStructureTool = new AnalysisStructureTool();
        
        model.AddFunctionTool(analysisStructureTool.AsGoogleFunctionTool());
        
        var analysisResult = await model.GenerateObjectAsync<SiteAnalysis>(prompt);

        return analysisResult;
    }
}
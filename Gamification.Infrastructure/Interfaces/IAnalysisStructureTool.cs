using System.ComponentModel;
using CSharpToJsonSchema;
using Gamification.Core.Models;

namespace Gamification.Infrastructure.Interfaces;

[GenerateJsonSchema(GoogleFunctionTool = true)] 
public interface IAnalysisStructureTool{
    [Description("Analyze a site")]
    public SiteAnalysis FormatOutputStructure(
        [Description("The category the site lies in")]
        List<string> category,
        
        [Description("The actual, productive value of the site on its own. Range is 0-100")]
        int intrinsicScore,
        
        [Description("The relevance of the site to the user's goals")]
        float relevanceScore
    );
}
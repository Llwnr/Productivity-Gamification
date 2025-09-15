using System.ComponentModel;
using CSharpToJsonSchema;
using Gamification.Core.Models;

namespace Gamification.Infrastructure.Interfaces;

[GenerateJsonSchema(GoogleFunctionTool = true)]
public interface IAnalysisStructureTool{
    [Description("Analyze a site")]
    public SiteAnalysis FormatOutputStructure(
        [Description("The categories of the site. Must be one of: Learning, Creation, Research, Social, Entertainment, News, Technology, Other. Can have multiple categories.")]
        List<string> category,

        [Description("The actual, productive value of the site on its own. Range is 0-100")]
        int intrinsicScore,

        [Description("The relevance of the site to the user's goals. Range is 0.0-1.0")]
        float relevanceScore
    );
}
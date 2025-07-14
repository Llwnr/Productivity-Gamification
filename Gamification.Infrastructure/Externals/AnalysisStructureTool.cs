using Gamification.Core.Models;
using Gamification.Infrastructure.Interfaces;

namespace Gamification.Infrastructure.Externals;

public class AnalysisStructureTool : IAnalysisStructureTool{
    public SiteAnalysis FormatOutputStructure(List<string> category, int intrinsicScore, float relevanceScore){
        return new SiteAnalysis{
            Category = category,
            IntrinsicScore = intrinsicScore,
            RelevanceScore = relevanceScore
        };
    }
}
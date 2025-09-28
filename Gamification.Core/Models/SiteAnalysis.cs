using System.ComponentModel;

namespace Gamification.Core.Models;
/// <summary>
/// Class that guides the LLM to provide output in this format. NOT FOR DATABASE models.
/// </summary>
public class SiteAnalysis{
    [Description("The categories of the site. Must be one of: Learning, Creation, Research, Social, Entertainment, News, Technology, Other. Can have multiple categories.")]
    public List<string> Category { get; set; }
    // public string Justification{ get; set; }
    [Description("Inherent productivity value of the site on a fixed integer scale (1–100).")]
    public int IntrinsicScore { get; set; }
    [Description("Relevance score (0.0–1.0) indicating the relevance of the activity to the user's goal.")]
    public float RelevanceScore { get; set; }
}
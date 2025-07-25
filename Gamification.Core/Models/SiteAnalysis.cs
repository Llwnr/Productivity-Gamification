namespace Gamification.Core.Models;
/// <summary>
/// Class that guides the LLM to provide output in this format. NOT FOR DATABASE models.
/// </summary>
public class SiteAnalysis{
    public List<string> Category { get; set; }
    // public string Justification{ get; set; }
    public int IntrinsicScore { get; set; }
    public float RelevanceScore { get; set; }
}
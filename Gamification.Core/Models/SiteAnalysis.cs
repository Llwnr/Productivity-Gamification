namespace Gamification.Core.Models;

using System.Text.Json.Serialization;

public class SiteAnalysis{
    public List<string> Category { get; set; }
    // public string Justification{ get; set; }
    public int IntrinsicScore { get; set; }
    public float RelevanceScore { get; set; }
}
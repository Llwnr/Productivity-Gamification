using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gamification.Infrastructure.Models;

public class Sites{
    public int SiteId { get; set; }
    public string? Url { get; set; }
    public string? Title{ get; set; }
    public string? Description{ get; set; }
}
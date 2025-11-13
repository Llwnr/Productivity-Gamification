using Gamification.Core.Models;

namespace Gamification.WebAPI.Models;

public class DailyAnalyticsDTO{
    public string Date{ get; set; }
    public List<SiteVisitRecordDTO> SiteVisits{ get; set; }
}
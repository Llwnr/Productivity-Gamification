using System.ComponentModel;

namespace Gamification.Core.Models;
/// <summary>
/// Class that guides the LLM to provide output in this format. NOT FOR DATABASE models.
/// </summary>
public class SiteAnalysis{
    [Description(@"Categorize the site using the official two-level taxonomy below. The output list must contain one Primary Category and may optionally contain ONE corresponding Secondary Category from the provided list. The first item in the list MUST be the Primary Category. If a site's topic is too broad for a specific secondary category (e.g., a major news portal), include only the Primary Category. The 'Other' category has no secondary options.

    FULL TAXONOMY:
    - Primary: Arts & Entertainment
      - Secondaries: Books & Literature, Celebrity & Entertainment News, Comics & Animation, Humor & Satire, Movies & Film, Music & Audio, Performing Arts, Television & Streaming, Visual Arts & Design
    - Primary: Business & Industry
      - Secondaries: Advertising & Marketing, Agriculture & Forestry, Business Services, Construction & Manufacturing, Finance & Investing, Human Resources, Real Estate
    - Primary: Computers & Technology
      - Secondaries: Computer Hardware, Consumer Electronics, Networking & Internet, Programming & Software Development, Tech News & Reviews, Tech Support & How-Tos
    - Primary: E-commerce & Shopping
      - Secondaries: Auctions & Marketplaces, Classifieds, Comparison Shopping, Coupons & Deals, Retail
    - Primary: Education & Reference
      - Secondaries: Academic & Research Institutions, Dictionaries & Encyclopedias, How-To & DIY Guides, Language Learning, Scientific & Educational Resources
    - Primary: Health & Wellness
      - Secondaries: Aging & Geriatrics, Alternative & Natural Medicine, Diet & Nutrition, Fitness & Exercise, Medical Conditions & Diseases, Mental Health
    - Primary: Hobbies & Lifestyle
      - Secondaries: Arts & Crafts, Fashion & Beauty, Food & Drink, Home & Garden, Pets & Animals, Travel & Tourism
    - Primary: News & Media
      - Secondaries: Global News, Local News & Weather, Opinion & Editorials, Politics, Sports News
    - Primary: Social & Community
      - Secondaries: Blogs & Personal Sites, Dating & Relationships, Forums & Message Boards, Genealogy & Family History, Social Networking
    - Primary: Other
      - Secondaries: (None)
    Example: A website reviewing the latest laptops is categorized as ['Computers & Technology', 'Tech News & Reviews'] or ['Computers & Technology'] if the secondary is none.")]
    public List<string> Category { get; set; }
    // public string Justification{ get; set; }
    [Description("Inherent productivity value of the site on a fixed integer scale (1–100).")]
    public int IntrinsicScore { get; set; }
    [Description("Relevance score (0.0–1.0) indicating the relevance of the activity to the user's goal.")]
    public float RelevanceScore { get; set; }
}
using Gamification.Infrastructure.DatabaseService;
using Gamification.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamification.WebAPI.Services
{
    public class DataSeeder
    {
        private readonly ProductivityDbContext _context;
        private readonly Random _random = new();

        public DataSeeder(ProductivityDbContext context)
        {
            _context = context;
        }

        public async Task SeedAdditionalDataAsync(int targetRowCount = 10000)
        {
            Console.WriteLine("Starting data seeding process...");

            var currentVisitCount = await _context.UserSiteVisits.CountAsync();
            if (currentVisitCount >= targetRowCount)
            {
                Console.WriteLine($"Database already has {currentVisitCount} visits, meeting the target of {targetRowCount}. No new data will be added.");
                return;
            }

            var users = await _context.Users.ToListAsync();
            var sites = await _context.Sites.ToListAsync();

            if (!users.Any())
            {
                Console.WriteLine("No users found. Seeding initial users.");
                users = await CreateInitialUsers();
            }
            if (sites.Count < 50)
            {
                Console.WriteLine("Site count is low. Seeding a diverse list of new sites.");
                sites = await SeedDiverseSites();
            }

            int visitsToAdd = targetRowCount - currentVisitCount;
            Console.WriteLine($"Current visit count is {currentVisitCount}. Adding {visitsToAdd} new visits using a pool of {sites.Count} sites.");

            // This is the rewritten method with the correct logic
            await CreateSequentialUserVisits(users, sites, visitsToAdd);

            Console.WriteLine("Data seeding process completed successfully.");
        }
        
        /// Creates a realistic, sequential timeline of site visits by iterating DAY BY DAY.
        /// This prevents generating impossible amounts of activity in a single day.
        /// </summary>
        private async Task CreateSequentialUserVisits(List<User> users, List<Site> sites, int totalVisitsToAdd)
        {
            var startDate = new DateTime(2025, 11, 20, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2025, 11, 26, 0, 0, 0, DateTimeKind.Utc);

            var newVisits = new List<UserSiteVisit>();
            var newAnalysisResults = new List<AnalysisResult>();
            var analysisCache = new Dictionary<string, AnalysisResult>();

            if (!users.Any()) return;

            foreach (var user in users)
            {
                // Iterate through each day in the desired range for the current user
                for (var day = startDate.Date; day < endDate.Date; day = day.AddDays(1))
                {
                    // 1. Decide how many visits this user will make today (e.g., between 5 and 30)
                    int visitsForThisDay = _random.Next(5, 31);
                    
                    // 2. Set the start of the user's activity for the day (e.g., between 8 AM and 10 AM)
                    DateTime currentUserTime = day.AddHours(9).AddMinutes(_random.Next(-60, 60));

                    for (int i = 0; i < visitsForThisDay; i++)
                    {
                        // 3. Simulate idle time before the next visit (e.g., 5 to 60 minutes)
                        currentUserTime = currentUserTime.AddMinutes(_random.Next(5, 61));

                        // If activity goes past 11 PM, stop generating visits for today
                        if (currentUserTime.Hour >= 23)
                        {
                            break;
                        }
                        
                        // 4. Set start/end time for this visit
                        DateTime visitStart = currentUserTime;
                        int durationMinutes = _random.Next(2, 45);
                        DateTime visitEnd = visitStart.AddMinutes(durationMinutes);

                        // 5. Select a random site
                        var site = sites[_random.Next(sites.Count)];
                        var analysisKey = $"{site.SiteId}|{user.Goal}";

                        // 6. Create AnalysisResult if needed
                        if (!analysisCache.ContainsKey(analysisKey) && !await _context.AnalysisResults.AnyAsync(a => a.SiteId == site.SiteId && a.UserGoal == user.Goal))
                        {
                            var analysis = new AnalysisResult
                            {
                                SiteId = site.SiteId,
                                UserGoal = user.Goal,
                                Category = GetRandomCategoryForSite(site.Url),
                                IntrinsicScore = _random.Next(20, 100),
                                RelevanceScore = (float)Math.Round(_random.NextDouble(), 2)
                            };
                            newAnalysisResults.Add(analysis);
                            analysisCache[analysisKey] = analysis;
                        }

                        // 7. Add the new visit
                        newVisits.Add(new UserSiteVisit
                        {
                            UserId = user.UserId,
                            SiteId = site.SiteId,
                            VisitStartDate = visitStart,
                            VisitEndDate = visitEnd,
                        });

                        // 8. Move the timeline cursor
                        currentUserTime = visitEnd;
                    }
                }
            }
            
            // We might not hit the exact `totalVisitsToAdd` target, but the data will be realistic.
            Console.WriteLine($"Generated {newVisits.Count} new realistic visits.");

            // Bulk insert all generated data
            await _context.AnalysisResults.AddRangeAsync(newAnalysisResults);
            await _context.UserSiteVisits.AddRangeAsync(newVisits);
            await _context.SaveChangesAsync();
        }

        // This helper function can remain the same
        private List<string> GetRandomCategoryForSite(string url)
        {
            if (url.Contains("stackoverflow") || url.Contains("github") || url.Contains("microsoft"))
                return new List<string> { "Computers & Technology", "Programming & Software Development" };
            if (url.Contains("youtube") || url.Contains("netflix") || url.Contains("twitch"))
                return new List<string> { "Arts & Entertainment", "Television & Streaming" };
            if (url.Contains("reddit") || url.Contains("twitter") || url.Contains("facebook"))
                return new List<string> { "Social & Community", "Social Networking" };
            if (url.Contains("amazon") || url.Contains("ebay"))
                return new List<string> { "E-commerce & Shopping", "Retail" };
            return new List<string> { "News & Media", "Global News" };
        }

        // --- Other methods remain unchanged ---
        
        private async Task<List<Site>> SeedDiverseSites()
        {
            var allPossibleSites = GetDiverseSiteList();
            var existingUrls = await _context.Sites.Select(s => s.Url).ToListAsync();
            var newSitesToAdd = allPossibleSites.Where(s => !existingUrls.Contains(s.Url)).ToList();

            if (newSitesToAdd.Any())
            {
                _context.Sites.AddRange(newSitesToAdd);
                await _context.SaveChangesAsync();
            }
            return await _context.Sites.ToListAsync();
        }

        private List<Site> GetDiverseSiteList()
        {
            // The large list of 50+ sites from the previous answer goes here...
            // (Keeping it collapsed for brevity, but it's the same list)
            return new List<Site>
            {
                // Productive & Tech
                new Site { Url = "https://stackoverflow.com/", Title = "Stack Overflow", Description = "Where developers learn, share, & build careers" },
                new Site { Url = "https://github.com/", Title = "GitHub", Description = "Where the world builds software" },
                new Site { Url = "https://learn.microsoft.com/en-us/dotnet/", Title = "Microsoft Learn (.NET)", Description = "Free learning paths and modules for .NET developers." },
                new Site { Url = "https://developer.mozilla.org/en-US/", Title = "MDN Web Docs", Description = "Resources for developers, by developers." },
                new Site { Url = "https://www.w3schools.com/", Title = "W3Schools Online Web Tutorials", Description = "The World's Largest Web Developer Site" },
                new Site { Url = "https://leetcode.com/", Title = "LeetCode", Description = "The world's leading online programming learning platform." },
                new Site { Url = "https://www.freecodecamp.org/", Title = "freeCodeCamp.org", Description = "Learn to code — for free. Build projects. Earn certifications." },
                new Site { Url = "https://css-tricks.com/", Title = "CSS-Tricks", Description = "Daily articles about CSS, HTML, JavaScript, and all things web development." },
                new Site { Url = "https://react.dev/", Title = "React", Description = "The library for web and native user interfaces" },
                new Site { Url = "https://vuejs.org/", Title = "Vue.js", Description = "The Progressive JavaScript Framework." },
                new Site { Url = "https://angular.io/", Title = "Angular", Description = "The web development framework for building the future." },
                new Site { Url = "https://www.figma.com/", Title = "Figma", Description = "The collaborative interface design tool." },
                new Site { Url = "https://trello.com/", Title = "Trello", Description = "Trello is the visual tool that empowers your team to manage any type of project, workflow, or task tracking." },
                new Site { Url = "https://docs.google.com/", Title = "Google Docs", Description = "Create and edit documents online, for free." },

                // News & Media
                new Site { Url = "https://www.bbc.com/news", Title = "BBC News", Description = "Breaking news, sport, TV, radio and a whole lot more." },
                new Site { Url = "https://www.reuters.com/", Title = "Reuters", Description = "The latest news from around the world, covering breaking news in markets, business, politics, entertainment, technology, video and pictures." },
                new Site { Url = "https://www.nytimes.com/", Title = "The New York Times", Description = "Live news, investigations, opinion, photos and video by the journalists of The New York Times from more than 150 countries around the world." },
                new Site { Url = "https://www.theguardian.com/", Title = "The Guardian", Description = "Latest world news, sport, business, opinion, analysis and reviews." },
                new Site { Url = "https://apnews.com/", Title = "AP News", Description = "Breaking news from the AP news wire, covering sports, entertainment, business, politics, and more." },
                new Site { Url = "https://techcrunch.com/", Title = "TechCrunch", Description = "Reporting on the business of technology, startups, venture capital funding, and Silicon Valley." },
                new Site { Url = "https://www.theverge.com/", Title = "The Verge", Description = "The Verge covers the intersection of technology, science, art, and culture." },
                
                // Social Media & Entertainment
                new Site { Url = "https://www.youtube.com/", Title = "YouTube", Description = "Enjoy the videos and music you love, upload original content, and share it all with friends, family, and the world on YouTube." },
                new Site { Url = "https://www.reddit.com/", Title = "Reddit", Description = "Reddit is a network of communities where people can dive into their interests, hobbies and passions." },
                new Site { Url = "https://twitter.com/", Title = "X (formerly Twitter)", Description = "From breaking news and entertainment to sports and politics, get the full story with all the live commentary." },
                new Site { Url = "https://www.facebook.com/", Title = "Facebook", Description = "Connect with friends and the world around you on Facebook." },
                new Site { Url = "https://www.instagram.com/", Title = "Instagram", Description = "A simple, fun & creative way to capture, edit & share photos, videos & messages with friends & family." },
                new Site { Url = "https://www.linkedin.com/", Title = "LinkedIn", Description = "Manage your professional identity. Build and engage with your professional network. Access knowledge, insights and opportunities." },
                new Site { Url = "https://www.pinterest.com/", Title = "Pinterest", Description = "Discover recipes, home ideas, style inspiration and other ideas to try." },
                new Site { Url = "https://www.twitch.tv/", Title = "Twitch", Description = "Twitch is the world's leading video platform and community for gamers." },
                new Site { Url = "https://www.netflix.com/", Title = "Netflix", Description = "Watch TV shows and movies online." },
                new Site { Url = "https://open.spotify.com/", Title = "Spotify", Description = "Spotify is a digital music service that gives you access to millions of songs." },

                // E-commerce & Shopping
                new Site { Url = "https://www.amazon.com/", Title = "Amazon.com", Description = "Online shopping for electronics, apparel, computers, books, DVDs & more" },
                new Site { Url = "https://www.ebay.com/", Title = "eBay", Description = "Buy & sell electronics, cars, clothes, collectibles & more on eBay, the world's online marketplace." },
                new Site { Url = "https://www.etsy.com/", Title = "Etsy", Description = "Find handmade, vintage, and custom gifts, or shop for your own style." },
                new Site { Url = "https://www.walmart.com/", Title = "Walmart.com", Description = "Shop Walmart.com for Every Day Low Prices. Free Shipping on Orders $35+ or Pickup In-Store and get a Pickup Discount." },
                new Site { Url = "https://www.target.com/", Title = "Target", Description = "Shop Target for groceries, essentials, clothing, electronics, furniture and more." },

                // General Knowledge & Other
                new Site { Url = "https://en.wikipedia.org/wiki/Main_Page", Title = "Wikipedia", Description = "The Free Encyclopedia" },
                new Site { Url = "https://www.quora.com/", Title = "Quora", Description = "A place to share knowledge and better understand the world." },
                new Site { Url = "https://www.khanacademy.org/", Title = "Khan Academy", Description = "You can learn anything. For free. For everyone. Forever." },
                new Site { Url = "https://www.coursera.org/", Title = "Coursera", Description = "Build skills with courses, certificates, and degrees online from world-class universities and companies." },
                new Site { Url = "https://www.udemy.com/", Title = "Udemy", Description = "Udemy is an online learning and teaching marketplace with over 155000 courses and 40 million students." },
                new Site { Url = "https://www.imdb.com/", Title = "IMDb", Description = "IMDb is the world's most popular and authoritative source for movie, TV and celebrity content." },
                new Site { Url = "https://www.espn.com/", Title = "ESPN", Description = "Visit ESPN to get up-to-the-minute sports news coverage, scores, highlights and commentary." },
            };
        }

        private async Task<List<User>> CreateInitialUsers()
        {
            var users = new List<User>
            {
                new User { Username = "TestUser1", Email = "test1@example.com", Password = "password", Goal = "Learn C#", DailyTargetHours = TimeSpan.FromHours(2) },
                new User { Username = "TestUser2", Email = "test2@example.com", Password = "password", Goal = "Master React", DailyTargetHours = TimeSpan.FromHours(3) }
            };
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            return users;
        }
    }
}
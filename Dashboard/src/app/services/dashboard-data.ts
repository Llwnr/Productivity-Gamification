import { Injectable } from '@angular/core';
import { SiteVisit, GameStat } from './api';

// --- Type Definitions for Data ---
export interface PointsData {
  title: string;
  value: number;
}

export interface ExpGaugeData {
  title: string;
  percentage: number;
}

export interface LevelData {
  title: string;
  level: number;
}

export interface StreakData {
  title: string;
  value: number;
  trendData: number[];
}

export interface Category {
  name: string;
  value: number;
}

export interface CategoriesData {
  title: string;
  categories: Category[];
}

export interface Site {
  url: string;
  timeSpent: number;
}

export interface TopSitesData {
  title: string;
  sites: Site[];
}

export interface TimeSpentData {
  title: string;
  dates: string[];
  values: number[];
}

export interface DailyUsageData {
  title: string;
  timeLabels: string[];
  dayLabels: string[];
  heatmapData: number[][];
}

@Injectable({
  providedIn: 'root'
})

export class DashboardData {


  getPointsData(data: GameStat): PointsData{
    return {
      title: "POINTS",
      value: data.coin
    }
  }

  getExpData(data: GameStat): ExpGaugeData{
    return {
      title: "EXP",
      percentage: data.experiencePoints
    }
  }

  getLevelData(data: GameStat): LevelData{
    return {
      title: "LEVEL",
      level: data.level
    }
  }

  getStreakData(): StreakData{
    return {
      title: "STREAK",
      value: 9,
      trendData: [1,2,3,4,4,5]
    };
  }

  getCategoriesData(siteVisits: SiteVisit[]): CategoriesData{
    let data: CategoriesData = {
      title: "Category",
      categories: []
    };
    siteVisits.forEach(siteVisit => {
      let cachedData = data.categories.find(d => d.name == siteVisit.mainCategory);
      if(cachedData != null){
        cachedData.value += siteVisit.timeSpent;
      }
      else{
        data.categories.push({
          name: siteVisit.mainCategory,
          value: siteVisit.timeSpent
        })
      }

      
    });
    return data;
  }

  getTopSitesData(siteVisits: SiteVisit[], topK: number): TopSitesData{
    let data: TopSitesData = {
      title: "Top Sites",
      sites: []
    };
    siteVisits.forEach(siteVisit => {
      let domainEndIndex = this.findThirdSlashIndex(siteVisit.siteUrl);
      let cachedData = data.sites.find(s => s.url == siteVisit.siteUrl.slice(0, domainEndIndex));
      if(cachedData != null){
        cachedData.timeSpent += siteVisit.timeSpent;
      }
      else{
        data.sites.push({
          url: siteVisit.siteUrl.slice(0, domainEndIndex),
          timeSpent: siteVisit.timeSpent
        })
      }    
    });

    //Take only top K number of data
    data.sites = data.sites.sort((a,b) => b.timeSpent - a.timeSpent).slice(0, topK);
    data.sites.forEach(element => {
      element.url = new URL(element.url).hostname;
    });

    return data;
  }

  getTimeSpentData(siteVisits: SiteVisit[]): TimeSpentData{
    let data: TimeSpentData = {
      title: "TimeSpent",
      dates: [],
      values: []
    }

    siteVisits.forEach(siteVisit => {
      let cachedDataIndex = data.dates.findIndex(date => {
        return this.dateToDayOnly(date).getTime() === this.dateToDayOnly(siteVisit.visitDate).getTime();
      });
      if(cachedDataIndex != -1){
        data.values[cachedDataIndex] += siteVisit.timeSpent;
      }
      else{
        data.dates.push(this.dateToDayOnly(siteVisit.visitDate).toLocaleDateString('en-US'));
        data.values.push(siteVisit.timeSpent);
      }    
    })

    return data;
  }

  getProductiveTimeSpentData(siteVisits: SiteVisit[]): TimeSpentData{
    let data: TimeSpentData = this.getTimeSpentData(siteVisits);
    data.title = "Productive time spent";
    data.values = [0,0,0,0,0,0,0,0,0,0,0,0,0]
    console.log(data);

    siteVisits.forEach(siteVisit => {
      if(siteVisit.baseProductiveScore > 25){
        let cachedDataIndex = data.dates.findIndex(date => {
          return this.dateToDayOnly(date).getTime() === this.dateToDayOnly(siteVisit.visitDate).getTime();
        });
        if(cachedDataIndex != -1){
          data.values[cachedDataIndex] += siteVisit.timeSpent;
        }
        else{
          data.dates.push(this.dateToDayOnly(siteVisit.visitDate).toLocaleDateString('en-US'));
          data.values.push(siteVisit.timeSpent);
        }    
      }


    })

    return data;
  }


  findThirdSlashIndex(str: string) {
    let firstSlash = str.indexOf('/');
    if (firstSlash === -1) {
      return -1; // No first slash found
    }

    let secondSlash = str.indexOf('/', firstSlash + 1);
    if (secondSlash === -1) {
      return -1; // No second slash found
    }

    let thirdSlash = str.indexOf('/', secondSlash + 1);
    return thirdSlash;
  }

  dateToDayOnly(str: string){
    let date: Date = new Date(str);
    return new Date(date.setHours(0,0,0,0));
  }

  getData(): SiteVisit[]{
    return [
      {
        "siteUrl": "https://mail.google.com/mail/u/0/#inbox",
        "baseProductiveScore": 90,
        "timeSpent": 55.1,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-01T09:15:43.135Z"
      },
      {
        "siteUrl": "https://calendar.google.com/calendar/r",
        "baseProductiveScore": 95,
        "timeSpent": 12.3,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-01T09:22:11.842Z"
      },
      {
        "siteUrl": "https://app.slack.com/client/T012345/C67890",
        "baseProductiveScore": 90,
        "timeSpent": 150.67,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-01T10:05:33.291Z"
      },
      {
        "siteUrl": "https://github.com/features",
        "baseProductiveScore": 95,
        "timeSpent": 45.78,
        "mainCategory": "Tech",
        "visitDate": "2023-10-01T10:48:09.918Z"
      },
      {
        "siteUrl": "https://stackoverflow.com/questions/12345/how-to-center-a-div",
        "baseProductiveScore": 98,
        "timeSpent": 15.21,
        "mainCategory": "Tech",
        "visitDate": "2023-10-01T11:01:14.567Z"
      },
      {
        "siteUrl": "https://developer.mozilla.org/en-US/docs/Web/JavaScript",
        "baseProductiveScore": 95,
        "timeSpent": 62.55,
        "mainCategory": "Tech",
        "visitDate": "2023-10-01T12:15:21.430Z"
      },
      {
        "siteUrl": "https://www.reddit.com/r/programming",
        "baseProductiveScore": 70,
        "timeSpent": 42.88,
        "mainCategory": "Tech",
        "visitDate": "2023-10-01T12:38:44.011Z"
      },
      {
        "siteUrl": "https://www.linkedin.com/feed/",
        "baseProductiveScore": 65,
        "timeSpent": 25.98,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-01T13:05:19.888Z"
      },
      {
        "siteUrl": "https://twitter.com/home",
        "baseProductiveScore": 15,
        "timeSpent": 33.7,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-01T13:21:04.123Z"
      },
      {
        "siteUrl": "https://www.nytimes.com/",
        "baseProductiveScore": 55,
        "timeSpent": 18.9,
        "mainCategory": "News",
        "visitDate": "2023-10-01T14:40:59.254Z"
      },
      {
        "siteUrl": "https://aws.amazon.com/console",
        "baseProductiveScore": 90,
        "timeSpent": 121.89,
        "mainCategory": "Tech",
        "visitDate": "2023-10-01T16:12:18.992Z"
      },
      {
        "siteUrl": "https://open.spotify.com/",
        "baseProductiveScore": 50,
        "timeSpent": 240.15,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-01T17:55:47.631Z"
      },
      {
        "siteUrl": "https://www.facebook.com/",
        "baseProductiveScore": 10,
        "timeSpent": 48.23,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-01T18:10:02.789Z"
      },
      {
        "siteUrl": "https://www.youtube.com/",
        "baseProductiveScore": 20,
        "timeSpent": 95.32,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-02T09:03:15.444Z"
      },
      {
        "siteUrl": "https://jira.atlassian.com/browse/PROJ-123",
        "baseProductiveScore": 95,
        "timeSpent": 65.4,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-02T09:33:29.811Z"
      },
      {
        "siteUrl": "https://docs.google.com/document/d/xyz987",
        "baseProductiveScore": 95,
        "timeSpent": 45.12,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-02T10:11:45.923Z"
      },
      {
        "siteUrl": "https://www.figma.com/files/project/12345/design-system",
        "baseProductiveScore": 97,
        "timeSpent": 180.3,
        "mainCategory": "Design",
        "visitDate": "2023-10-02T11:45:01.555Z"
      },
      {
        "siteUrl": "https://dribbble.com/shots/popular",
        "baseProductiveScore": 60,
        "timeSpent": 22.8,
        "mainCategory": "Design",
        "visitDate": "2023-10-02T12:01:17.345Z"
      },
      {
        "siteUrl": "https://www.khanacademy.org/math/algebra",
        "baseProductiveScore": 100,
        "timeSpent": 49.99,
        "mainCategory": "Education",
        "visitDate": "2023-10-02T13:20:30.102Z"
      },
      {
        "siteUrl": "https://www.udemy.com/course/the-web-developer-bootcamp/",
        "baseProductiveScore": 95,
        "timeSpent": 78.34,
        "mainCategory": "Education",
        "visitDate": "2023-10-02T14:55:49.876Z"
      },
      {
        "siteUrl": "https://www.reddit.com/r/funny",
        "baseProductiveScore": 5,
        "timeSpent": 51.5,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-02T15:15:05.621Z"
      },
      {
        "siteUrl": "https://www.amazon.com/deals",
        "baseProductiveScore": 15,
        "timeSpent": 19.55,
        "mainCategory": "Shopping",
        "visitDate": "2023-10-02T16:01:23.489Z"
      },
      {
        "siteUrl": "https://www.wsj.com/news/markets",
        "baseProductiveScore": 75,
        "timeSpent": 35.6,
        "mainCategory": "Finance",
        "visitDate": "2023-10-03T09:08:11.931Z"
      },
      {
        "siteUrl": "https://www.bloomberg.com/",
        "baseProductiveScore": 80,
        "timeSpent": 41.2,
        "mainCategory": "Finance",
        "visitDate": "2023-10-03T09:42:18.456Z"
      },
      {
        "siteUrl": "https://trello.com/b/boardid/project-alpha",
        "baseProductiveScore": 98,
        "timeSpent": 76.11,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-03T10:30:55.123Z"
      },
      {
        "siteUrl": "https://github.com/torvalds/linux",
        "baseProductiveScore": 95,
        "timeSpent": 33.4,
        "mainCategory": "Tech",
        "visitDate": "2023-10-03T11:10:22.888Z"
      },
      {
        "siteUrl": "https://css-tricks.com/a-guide-to-flexbox/",
        "baseProductiveScore": 98,
        "timeSpent": 28.9,
        "mainCategory": "Tech",
        "visitDate": "2023-10-03T11:35:47.521Z"
      },
      {
        "siteUrl": "https://www.instagram.com/",
        "baseProductiveScore": 5,
        "timeSpent": 68.14,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-03T12:55:10.999Z"
      },
      {
        "siteUrl": "https://www.youtube.com/watch?v=some_educational_video",
        "baseProductiveScore": 80,
        "timeSpent": 28.6,
        "mainCategory": "Education",
        "visitDate": "2023-10-03T14:02:33.410Z"
      },
      {
        "siteUrl": "https://en.wikipedia.org/wiki/Machine_learning",
        "baseProductiveScore": 85,
        "timeSpent": 31.23,
        "mainCategory": "Reference",
        "visitDate": "2023-10-03T14:48:59.777Z"
      },
      {
        "siteUrl": "https://www.coursera.org/learn/machine-learning",
        "baseProductiveScore": 98,
        "timeSpent": 112.8,
        "mainCategory": "Education",
        "visitDate": "2023-10-03T16:20:14.321Z"
      },
      {
        "siteUrl": "https://www.netflix.com/browse",
        "baseProductiveScore": 5,
        "timeSpent": 125.0,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-03T20:30:00.192Z"
      },
      {
        "siteUrl": "https://www.bbc.com/news",
        "baseProductiveScore": 55,
        "timeSpent": 22.45,
        "mainCategory": "News",
        "visitDate": "2023-10-04T09:05:22.813Z"
      },
      {
        "siteUrl": "https://techcrunch.com/",
        "baseProductiveScore": 70,
        "timeSpent": 15.1,
        "mainCategory": "News",
        "visitDate": "2023-10-04T09:18:39.467Z"
      },
      {
        "siteUrl": "https://mail.google.com/mail/u/0/#inbox",
        "baseProductiveScore": 90,
        "timeSpent": 35.8,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-04T09:45:11.582Z"
      },
      {
        "siteUrl": "https://docs.google.com/spreadsheets/d/1a2b3c",
        "baseProductiveScore": 95,
        "timeSpent": 88.43,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-04T11:02:44.134Z"
      },
      {
        "siteUrl": "https://stackoverflow.com/questions/98765/how-to-parse-json-in-python",
        "baseProductiveScore": 98,
        "timeSpent": 19.8,
        "mainCategory": "Tech",
        "visitDate": "2023-10-04T11:33:59.001Z"
      },
      {
        "siteUrl": "https://www.notion.so/Weekly-Sync-Notes-a1b2c3d4e5",
        "baseProductiveScore": 98,
        "timeSpent": 95.2,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-04T13:45:21.876Z"
      },
      {
        "siteUrl": "https://www.expedia.com/",
        "baseProductiveScore": 25,
        "timeSpent": 17.76,
        "mainCategory": "Travel",
        "visitDate": "2023-10-04T14:01:33.921Z"
      },
      {
        "siteUrl": "https://www.airbnb.com/",
        "baseProductiveScore": 25,
        "timeSpent": 30.12,
        "mainCategory": "Travel",
        "visitDate": "2023-10-04T14:28:49.102Z"
      },
      {
        "siteUrl": "https://www.etsy.com/",
        "baseProductiveScore": 15,
        "timeSpent": 24.05,
        "mainCategory": "Shopping",
        "visitDate": "2023-10-04T15:55:01.743Z"
      },
      {
        "siteUrl": "https://www.webmd.com/",
        "baseProductiveScore": 40,
        "timeSpent": 8.95,
        "mainCategory": "Health",
        "visitDate": "2023-10-04T16:10:28.333Z"
      },
      {
        "siteUrl": "https://www.nih.gov/",
        "baseProductiveScore": 85,
        "timeSpent": 29.8,
        "mainCategory": "Health",
        "visitDate": "2023-10-04T16:45:55.918Z"
      },
      {
        "siteUrl": "https://www.tiktok.com/@funnyvideos",
        "baseProductiveScore": 5,
        "timeSpent": 75.8,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-04T17:30:12.456Z"
      },
      {
        "siteUrl": "https://app.slack.com/client/T012345/DABCDE",
        "baseProductiveScore": 90,
        "timeSpent": 112.3,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-05T09:12:34.567Z"
      },
      {
        "siteUrl": "https://vercel.com/dashboard",
        "baseProductiveScore": 90,
        "timeSpent": 44.6,
        "mainCategory": "Tech",
        "visitDate": "2023-10-05T10:01:49.123Z"
      },
      {
        "siteUrl": "https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API",
        "baseProductiveScore": 95,
        "timeSpent": 58.2,
        "mainCategory": "Tech",
        "visitDate": "2023-10-05T11:22:05.876Z"
      },
      {
        "siteUrl": "https://www.linkedin.com/jobs/",
        "baseProductiveScore": 75,
        "timeSpent": 48.9,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-05T12:40:19.345Z"
      },
      {
        "siteUrl": "https://www.youtube.com/watch?v=learning_video_3",
        "baseProductiveScore": 80,
        "timeSpent": 45.9,
        "mainCategory": "Education",
        "visitDate": "2023-10-05T14:15:33.999Z"
      },
      {
        "siteUrl": "https://www.reddit.com/r/dataisbeautiful",
        "baseProductiveScore": 40,
        "timeSpent": 38.2,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-05T14:45:47.111Z"
      },
      {
        "siteUrl": "https://www.edx.org/learn/computer-science",
        "baseProductiveScore": 98,
        "timeSpent": 130.5,
        "mainCategory": "Education",
        "visitDate": "2023-10-05T16:30:02.777Z"
      },
      {
        "siteUrl": "https://www.twitch.tv/",
        "baseProductiveScore": 10,
        "timeSpent": 88.1,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-05T21:05:18.432Z"
      },
      {
        "siteUrl": "https://mail.google.com/mail/u/0/#inbox",
        "baseProductiveScore": 90,
        "timeSpent": 15.2,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-06T09:02:14.987Z"
      },
      {
        "siteUrl": "https://github.com/facebook/react",
        "baseProductiveScore": 95,
        "timeSpent": 110.5,
        "mainCategory": "Tech",
        "visitDate": "2023-10-06T10:45:33.543Z"
      },
      {
        "siteUrl": "https://stackoverflow.com/questions/54321/another-question",
        "baseProductiveScore": 98,
        "timeSpent": 22.3,
        "mainCategory": "Tech",
        "visitDate": "2023-10-06T11:05:49.210Z"
      },
      {
        "siteUrl": "https://medium.com/topic/programming",
        "baseProductiveScore": 75,
        "timeSpent": 39.7,
        "mainCategory": "Tech",
        "visitDate": "2023-10-06T11:50:01.876Z"
      },
      {
        "siteUrl": "https://www.figma.com/community/file/12345",
        "baseProductiveScore": 97,
        "timeSpent": 85.4,
        "mainCategory": "Design",
        "visitDate": "2023-10-06T14:10:25.432Z"
      },
      {
        "siteUrl": "https://www.behance.net/",
        "baseProductiveScore": 60,
        "timeSpent": 31.6,
        "mainCategory": "Design",
        "visitDate": "2023-10-06T14:35:44.998Z"
      },
      {
        "siteUrl": "https://www.amazon.com/",
        "baseProductiveScore": 15,
        "timeSpent": 12.4,
        "mainCategory": "Shopping",
        "visitDate": "2023-10-06T15:01:11.555Z"
      },
      {
        "siteUrl": "https://en.wikipedia.org/wiki/Quantum_computing",
        "baseProductiveScore": 85,
        "timeSpent": 44.1,
        "mainCategory": "Reference",
        "visitDate": "2023-10-07T10:20:30.123Z"
      },
      {
        "siteUrl": "https://www.youtube.com/watch?v=history_documentary",
        "baseProductiveScore": 80,
        "timeSpent": 55.8,
        "mainCategory": "Education",
        "visitDate": "2023-10-07T11:35:49.876Z"
      },
      {
        "siteUrl": "https://www.reddit.com/r/science",
        "baseProductiveScore": 70,
        "timeSpent": 62.3,
        "mainCategory": "News",
        "visitDate": "2023-10-07T13:05:02.432Z"
      },
      {
        "siteUrl": "https://www.hulu.com/",
        "baseProductiveScore": 5,
        "timeSpent": 140.2,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-07T20:15:18.999Z"
      },
      {
        "siteUrl": "https://www.theguardian.com/international",
        "baseProductiveScore": 55,
        "timeSpent": 25.9,
        "mainCategory": "News",
        "visitDate": "2023-10-08T11:00:33.555Z"
      },
      {
        "siteUrl": "https://www.pinterest.com/",
        "baseProductiveScore": 10,
        "timeSpent": 41.7,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-08T14:20:49.111Z"
      },
      {
        "siteUrl": "https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M",
        "baseProductiveScore": 50,
        "timeSpent": 180.0,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-09T09:05:12.777Z"
      },
      {
        "siteUrl": "https://docs.google.com/presentation/d/asdf123",
        "baseProductiveScore": 95,
        "timeSpent": 125.6,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-09T10:55:30.333Z"
      },
      {
        "siteUrl": "https://aws.amazon.com/s3/",
        "baseProductiveScore": 90,
        "timeSpent": 38.8,
        "mainCategory": "Tech",
        "visitDate": "2023-10-09T11:40:48.888Z"
      },
      {
        "siteUrl": "https://www.digitalocean.com/community/tutorials",
        "baseProductiveScore": 95,
        "timeSpent": 51.2,
        "mainCategory": "Tech",
        "visitDate": "2023-10-09T13:10:01.444Z"
      },
      {
        "siteUrl": "https://www.udemy.com/course/python-for-data-science-and-machine-learning-bootcamp/",
        "baseProductiveScore": 95,
        "timeSpent": 92.7,
        "mainCategory": "Education",
        "visitDate": "2023-10-09T15:25:20.999Z"
      },
      {
        "siteUrl": "https://www.reuters.com/",
        "baseProductiveScore": 60,
        "timeSpent": 14.5,
        "mainCategory": "News",
        "visitDate": "2023-10-10T09:03:44.555Z"
      },
      {
        "siteUrl": "https://mint.intuit.com/",
        "baseProductiveScore": 80,
        "timeSpent": 20.1,
        "mainCategory": "Finance",
        "visitDate": "2023-10-10T09:25:01.111Z"
      },
      {
        "siteUrl": "https://www.robinhood.com/",
        "baseProductiveScore": 70,
        "timeSpent": 33.3,
        "mainCategory": "Finance",
        "visitDate": "2023-10-10T09:55:19.666Z"
      },
      {
        "siteUrl": "https://app.slack.com/client/T012345/C67890",
        "baseProductiveScore": 90,
        "timeSpent": 210.2,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-10T11:40:33.222Z"
      },
      {
        "siteUrl": "https://github.com/pulls",
        "baseProductiveScore": 95,
        "timeSpent": 75.9,
        "mainCategory": "Tech",
        "visitDate": "2023-10-10T13:15:50.777Z"
      },
      {
        "siteUrl": "https://stackoverflow.com/questions/246801/how-to-find-the-word-with-most-occurrences-in-a-string",
        "baseProductiveScore": 98,
        "timeSpent": 12.1,
        "mainCategory": "Tech",
        "visitDate": "2023-10-10T13:30:05.333Z"
      },
      {
        "siteUrl": "https://www.facebook.com/messages",
        "baseProductiveScore": 10,
        "timeSpent": 28.4,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-10T14:01:21.888Z"
      },
      {
        "siteUrl": "https://www.bestbuy.com/",
        "baseProductiveScore": 15,
        "timeSpent": 18.8,
        "mainCategory": "Shopping",
        "visitDate": "2023-10-10T16:05:40.444Z"
      },
      {
        "siteUrl": "https://www.ebay.com/",
        "baseProductiveScore": 15,
        "timeSpent": 21.5,
        "mainCategory": "Shopping",
        "visitDate": "2023-10-10T16:30:59.999Z"
      },
      {
        "siteUrl": "https://mail.google.com/mail/u/0/#inbox",
        "baseProductiveScore": 90,
        "timeSpent": 25.6,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-11T09:10:14.555Z"
      },
      {
        "siteUrl": "https://asana.com/a/p/123456789",
        "baseProductiveScore": 98,
        "timeSpent": 105.7,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-11T10:45:33.111Z"
      },
      {
        "siteUrl": "https://www.figma.com/proto/abcdef",
        "baseProductiveScore": 97,
        "timeSpent": 66.8,
        "mainCategory": "Design",
        "visitDate": "2023-10-11T11:30:51.666Z"
      },
      {
        "siteUrl": "https://twitter.com/home",
        "baseProductiveScore": 15,
        "timeSpent": 45.1,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-11T12:55:08.222Z"
      },
      {
        "siteUrl": "https://www.youtube.com/feed/subscriptions",
        "baseProductiveScore": 20,
        "timeSpent": 77.3,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-11T13:40:26.777Z"
      },
      {
        "siteUrl": "https://www.coursera.org/professional-certificates/google-data-analytics",
        "baseProductiveScore": 98,
        "timeSpent": 150.9,
        "mainCategory": "Education",
        "visitDate": "2023-10-11T15:55:45.333Z"
      },
      {
        "siteUrl": "https://www.mayoclinic.org/",
        "baseProductiveScore": 85,
        "timeSpent": 15.3,
        "mainCategory": "Health",
        "visitDate": "2023-10-11T16:20:03.888Z"
      },
      {
        "siteUrl": "https://www.netflix.com/title/80057281",
        "baseProductiveScore": 5,
        "timeSpent": 98.2,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-11T21:01:22.444Z"
      },
      {
        "siteUrl": "https://www.wsj.com/",
        "baseProductiveScore": 75,
        "timeSpent": 29.8,
        "mainCategory": "Finance",
        "visitDate": "2023-10-12T09:02:40.999Z"
      },
      {
        "siteUrl": "https://calendar.google.com/calendar/r",
        "baseProductiveScore": 95,
        "timeSpent": 9.8,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-12T09:18:59.555Z"
      },
      {
        "siteUrl": "https://github.com/microsoft/vscode",
        "baseProductiveScore": 95,
        "timeSpent": 55.4,
        "mainCategory": "Tech",
        "visitDate": "2023-10-12T10:33:18.111Z"
      },
      {
        "siteUrl": "https://developer.mozilla.org/en-US/docs/Web/CSS/grid",
        "baseProductiveScore": 95,
        "timeSpent": 71.3,
        "mainCategory": "Tech",
        "visitDate": "2023-10-12T11:55:36.666Z"
      },
      {
        "siteUrl": "https://www.reddit.com/r/technology",
        "baseProductiveScore": 65,
        "timeSpent": 36.5,
        "mainCategory": "News",
        "visitDate": "2023-10-12T12:30:55.222Z"
      },
      {
        "siteUrl": "https://www.linkedin.com/in/someprofile",
        "baseProductiveScore": 65,
        "timeSpent": 11.9,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-12T13:01:13.777Z"
      },
      {
        "siteUrl": "https://www.khanacademy.org/science/biology",
        "baseProductiveScore": 100,
        "timeSpent": 63.2,
        "mainCategory": "Education",
        "visitDate": "2023-10-12T14:45:32.333Z"
      },
      {
        "siteUrl": "https://www.instagram.com/explore/tags/travel/",
        "baseProductiveScore": 5,
        "timeSpent": 52.1,
        "mainCategory": "Social Media",
        "visitDate": "2023-10-12T15:15:50.888Z"
      },
      {
        "siteUrl": "https://open.spotify.com/browse/podcasts",
        "baseProductiveScore": 50,
        "timeSpent": 135.6,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-13T09:30:09.444Z"
      },
      {
        "siteUrl": "https://trello.com/b/anotherboard/project-beta",
        "baseProductiveScore": 98,
        "timeSpent": 99.1,
        "mainCategory": "Productivity",
        "visitDate": "2023-10-13T10:45:27.999Z"
      },
      {
        "siteUrl": "https://stackoverflow.com/questions/348170/what-is-the-best-comment-in-source-code-you-have-ever-encountered",
        "baseProductiveScore": 30,
        "timeSpent": 25.5,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-13T11:15:46.555Z"
      },
      {
        "siteUrl": "https://www.expedia.com/Flights",
        "baseProductiveScore": 25,
        "timeSpent": 34.2,
        "mainCategory": "Travel",
        "visitDate": "2023-10-13T14:01:05.111Z"
      },
      {
        "siteUrl": "https://en.wikipedia.org/wiki/List_of_cognitive_biases",
        "baseProductiveScore": 85,
        "timeSpent": 48.7,
        "mainCategory": "Reference",
        "visitDate": "2023-10-13T15:25:23.666Z"
      },
      {
        "siteUrl": "https://www.youtube.com/watch?v=funny_cat_video",
        "baseProductiveScore": 10,
        "timeSpent": 15.8,
        "mainCategory": "Entertainment",
        "visitDate": "2023-10-13T17:01:42.222Z"
      }
    ]
  }
  
}

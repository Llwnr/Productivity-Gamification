import { Injectable } from '@angular/core';
import { GameStat } from './api';

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

export interface DailyAnalyticsDTO {
  date: string; // "yyyy-MM-dd"
  siteVisits: SiteVisitRecordDTO[];
}

export interface SiteVisitRecordDTO {
  siteUrl: string;
  timeSpent: number; // in seconds
  baseProductiveScore: number;
  mainCategory: string;
}

export interface CategoriesData {
  title: string;
  categories: { name: string; value: number }[];
}

export interface TopSitesData {
  title: string;
  sites: { url: string; timeSpent: number }[];
}

export interface TimeSpentData {
  title: string;
  dates: Date[];
  values: number[];
}

export interface DailyUsageData {
  title: string;
  timeLabels: string[];
  dayLabels: string[];
  heatmapData: number[][];
}

// Represents the raw productivity data input for a single day.
export interface ProductivityLog{
    date: string; // Expected format: 'YYYY-MM-DD'
    productiveTime: number; // e.g., in hours
}

// Defines the shape of the processed data required by our Plotly function.
export interface HeatmapData {
    xValues: string[];
    yValues: string[];
    zValues: (number | null)[][]; // The raw numeric values for coloring and display
    hoverText: string[][];      // The detailed text for the hover tooltip
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

  getCategoriesData(dailyVisits: SiteVisitRecordDTO[]): CategoriesData {
    const categoryMap = new Map<string, number>();

    dailyVisits.forEach(visit => {
      const currentTotal = categoryMap.get(visit.mainCategory) || 0;
      categoryMap.set(visit.mainCategory, currentTotal + visit.timeSpent);
    });

    const categories = Array.from(categoryMap.entries()).map(([name, value]) => ({ name, value }));

    return {
      title: "Category",
      categories: categories
    };
  }

  getTopSitesData(dailyVisits: SiteVisitRecordDTO[], topK: number): TopSitesData {
    // The logic is already correct for a daily list, just ensure the input type is right.
    const sites = dailyVisits.map(visit => ({
      url: new URL(visit.siteUrl).hostname,
      timeSpent: visit.timeSpent / 60 // Convert seconds to minutes for display
    }));

    // Sort and slice to get the top K
    const topSites = sites.sort((a, b) => b.timeSpent - a.timeSpent).slice(0, topK);

    return {
      title: "Top Sites",
      sites: topSites
    };
  }

  getTimeSpentData(dailyAnalytics: DailyAnalyticsDTO[]): TimeSpentData {
    const sortedAnalytics = [...dailyAnalytics].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

    const dates = sortedAnalytics.map(day => new Date(day.date));
    const values = sortedAnalytics.map(day => day.siteVisits.reduce((total, visit) => total + visit.timeSpent, 0));

    return {
      title: "Time Spent Per Day",
      dates: dates,
      values: values // Values are still in seconds here
    };
  }

  getProductiveTimeSpentData(dailyAnalytics: DailyAnalyticsDTO[]): TimeSpentData {
    const sortedAnalytics = [...dailyAnalytics].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
    
    const dates = sortedAnalytics.map(day => new Date(day.date));
    const values = sortedAnalytics.map(day =>
      day.siteVisits
        .filter(visit => visit.baseProductiveScore >= 25) // Your productivity threshold
        .reduce((total, visit) => total + visit.timeSpent, 0)
    );

    return {
      title: "Productive Time Spent",
      dates: dates,
      values: values // Values are in seconds
    };
  }

  processDataForRolling30Days(productivityLogs: ProductivityLog[]): HeatmapData {
    const today = new Date();
    const startDate = new Date(today);
    startDate.setDate(today.getDate() - 29); // last 30 days

    // Map date string -> hours
    const map = new Map<string, number>();
    productivityLogs.forEach(log => {
        const d = new Date(log.date);
        const key = d.toDateString();
        map.set(key, log.productiveTime);
    });

    const zValues: (number | null)[][] = [];
    const hoverText: string[][] = [];
    let weekZ: (number | null)[] = [];
    let weekHover: string[] = [];

    // Pad before the first weekday
    const firstDay = startDate.getDay();
    for (let i = 0; i < firstDay; i++) {
        weekZ.push(null);
        weekHover.push('');
    }

    const d = new Date(startDate);
    while (d <= today) {
        const key = d.toDateString();
        const hrs = map.get(key) ?? 0;

        weekZ.push(hrs > 0 ? hrs : null);
        weekHover.push(`Date: ${key}<br>Hours: ${hrs.toFixed(2)}`);

        // If end of week (Saturday), push and reset
        if (d.getDay() === 6) {
            zValues.push(weekZ);
            hoverText.push(weekHover);
            weekZ = [];
            weekHover = [];
        }

        d.setDate(d.getDate() + 1);
    }

    // Push trailing partial week and pad to 7 columns
    if (weekZ.length > 0) {
        while (weekZ.length < 7) {
            weekZ.push(null);
            weekHover.push('');
        }
        zValues.push(weekZ);
        hoverText.push(weekHover);
    }

    return {
        xValues: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
        yValues: zValues.map((_, i) => {
          const labelDate = new Date(startDate);
          labelDate.setDate(startDate.getDate() + i * 7);
          return labelDate.toLocaleString('default', { month: 'short', day: 'numeric' });
      }),
        zValues,
        hoverText,
    };
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

  userLogs: ProductivityLog[] = [
      { "date": "2025-10-12", "productiveTime": 7.3 },
      { "date": "2025-10-13", "productiveTime": 11.8 },
      { "date": "2025-10-14", "productiveTime": 2.1 },
      { "date": "2025-10-15", "productiveTime": 14.6 },
      { "date": "2025-10-16", "productiveTime": 0.7 },
      { "date": "2025-10-17", "productiveTime": 9.2 },
      { "date": "2025-10-18", "productiveTime": 15.9 },
      { "date": "2025-10-19", "productiveTime": 4.3 },
      { "date": "2025-10-20", "productiveTime": 8.8 },
      { "date": "2025-10-21", "productiveTime": 1.5 },
      { "date": "2025-10-22", "productiveTime": 13.1 },
      { "date": "2025-10-23", "productiveTime": 6.4 },
      { "date": "2025-10-24", "productiveTime": 10.9 },
      { "date": "2025-10-25", "productiveTime": 3.7 },
      { "date": "2025-10-26", "productiveTime": 12.2 },
      { "date": "2025-10-27", "productiveTime": 5.0 },
      { "date": "2025-10-28", "productiveTime": 14.1 },
      { "date": "2025-10-29", "productiveTime": 0.2 },
      { "date": "2025-10-30", "productiveTime": 8.5 },
      { "date": "2025-10-31", "productiveTime": 11.3 },
      { "date": "2025-11-01", "productiveTime": 2.8 },
      { "date": "2025-11-02", "productiveTime": 15.4 },
      { "date": "2025-11-03", "productiveTime": 7.9 },
      { "date": "2025-11-04", "productiveTime": 9.7 },
      { "date": "2025-11-05", "productiveTime": 1.1 },
      { "date": "2025-11-06", "productiveTime": 13.8 },
      { "date": "2025-11-07", "productiveTime": 4.6 },
      { "date": "2025-11-08", "productiveTime": 10.2 },
      { "date": "2025-11-09", "productiveTime": 6.9 },
      { "date": "2025-11-10", "productiveTime": 12.7 },
      { "date": "2025-11-11", "productiveTime": 3.3 },
      { "date": "2025-11-12", "productiveTime": 15.0 },
      { "date": "2025-11-13", "productiveTime": 0.5 },
      { "date": "2025-11-14", "productiveTime": 8.1 },
      { "date": "2025-11-15", "productiveTime": 11.6 },
      { "date": "2025-11-16", "productiveTime": 5.8 }
  ];  
}

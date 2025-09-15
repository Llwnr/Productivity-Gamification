import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Api, GameStat, SiteVisit } from '../services/api';
import { Observable } from 'rxjs';
import * as Plotly from 'plotly.js-dist-min';

// --- Type Definitions for Data ---
interface PointsData {
  title: string;
  value: number;
}

interface ExpGaugeData {
  title: string;
  percentage: number;
}

interface LevelData {
  title: string;
  level: number;
}

interface StreakData {
  title: string;
  value: number;
  trendData: number[];
}

interface Category {
  name: string;
  value: number;
}

interface CategoriesData {
  title: string;
  categories: Category[];
}

interface Site {
  url: string;
  timeSpent: number;
}

interface TopSitesData {
  title: string;
  sites: Site[];
}

interface TimeSpentData {
  title: string;
  dates: string[];
  values: number[];
}

interface DailyUsageData {
  title: string;
  timeLabels: string[];
  dayLabels: string[];
  heatmapData: number[][];
}

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})

export class Dashboard implements OnInit{
  private apiService = inject(Api);

  chartData: ChartData = new ChartData();

  public stat$?: Observable<GameStat>;
  public siteVisits$? : Observable<SiteVisit[]>;

  public dummyData: SiteVisit[] = getData();

  ngOnInit(): void{
    this.stat$ = this.apiService.getDashboardStat();
    this.siteVisits$ = this.apiService.getUserSiteVisits();

    this.chartData.Labels =  ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'];
    this.chartData.Datas = [5,7,8,3,5,2,29];
    this.chartData.Add([],["Pink"]);

    this.stat$.subscribe(result => {
      createPointsDisplay('pointsCard', getPointsData(result));
      createExpGauge('expCard', getExpData(result));
      createLevelDisplay('levelCard', getLevelData(result));
    })

    createStreakDisplay('streakCard', getStreakData());

    this.siteVisits$.subscribe(result => {
      createCategoriesChart('by-category', getCategoriesData(result));
      createTopSitesChart('by-top-sites', getTopSitesData(result, 5));
    })

  }
}
// --- Charting Functions ---

// Shared dark theme configuration
const darkThemeLayout: Partial<Plotly.Layout> = {
  paper_bgcolor: '#292A2F',
  plot_bgcolor: '#292A2F',
  font: {
    color: '#A0AEC0',
    family: 'Arial, sans-serif',
    size: 12,
  },
  margin: { l: 40, r: 20, t: 40, b: 40 },
  showlegend: false,
};

//#region DATA GET
function getPointsData(data: GameStat): PointsData{
  return {
    title: "POINTS",
    value: data.coin
  }
}

function getExpData(data: GameStat): ExpGaugeData{
  return {
    title: "EXP",
    percentage: data.experiencePoints
  }
}

function getLevelData(data: GameStat): LevelData{
  return {
    title: "LEVEL",
    level: data.level
  }
}

function getStreakData(): StreakData{
  return {
    title: "STREAK",
    value: 9,
    trendData: [1,2,3,4,4,5]
  };
}

function getCategoriesData(siteVisits: SiteVisit[]): CategoriesData{
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

function getTopSitesData(siteVisits: SiteVisit[], topK: number): TopSitesData{
  let data: TopSitesData = {
    title: "Top Sites",
    sites: []
  };
  siteVisits.forEach(siteVisit => {
    let domainEndIndex = findThirdSlashIndex(siteVisit.siteUrl);
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


function findThirdSlashIndex(str: string) {
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
//#endregion
/**
 * Creates a points display card (large number display)
 */
//#region CHART CREATION
function createPointsDisplay(elementId: string, data: PointsData) {
  const trace: Partial<Plotly.PlotData> = {
    type: 'indicator',
    mode: 'number',
    value: data.value,
    number: {
      font: { size: 48, color: '#FFFFFF' },
      valueformat: ",.0f"
    },
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' },
      standoff: 30
    }
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    margin: { l: 20, r: 20, t: 20, b: 20 },
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false,
    staticPlot: true
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates an EXP progress circle (gauge chart)
 */
function createExpGauge(elementId: string, data: ExpGaugeData) {
  const trace: Partial<Plotly.PlotData> = {
    type: 'indicator',
    mode: 'gauge+number',
    value: data.percentage,
    number: {
      suffix: '%',
      font: { size: 24, color: '#FFFFFF' }
    },
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' }
    },
    gauge: {
      axis: { range: [null, 100], visible: false },
      bar: { color: '#6395F2', thickness: 1 },
      bgcolor: '#2D3748',
      borderwidth: 0,
      steps: [{ range: [0, 100], color: '#2D3748' }]
    }
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    margin: { l: 20, r: 20, t: 20, b: 20 },
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false,
    staticPlot: true
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a level badge display
 */
function createLevelDisplay(elementId: string, data: LevelData) {
  const trace: Partial<Plotly.PlotData> = {
    type: 'indicator',
    mode: 'number',
    value: data.level,
    number: {
      font: { size: 36, color: '#FFFFFF' }
    },
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' }
    }
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    margin: { l: 20, r: 20, t: 20, b: 20 },
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false,
    staticPlot: true
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a streak display with trend line
 */
function createStreakDisplay(elementId: string, data: StreakData) {
  const numberTrace: Partial<Plotly.PlotData> = {
    type: 'indicator',
    mode: 'number',
    value: data.value,
    number: {
      font: { size: 36, color: '#FFFFFF' }
    },
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' }
    },
  };

  const trendTrace: Partial<Plotly.PlotData> = {
    type: 'scatter',
    mode: 'lines+markers',
    x: Array.from({ length: data.trendData.length }, (_, i) => i),
    y: data.trendData,
    line: { color: '#A0AEC0', width: 2 },
    marker: { color: '#A0AEC0', size: 4 },
    xaxis: 'x2',
    yaxis: 'y2',
    showlegend: false
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    margin: { l: 20, r: 20, t: 20, b: 20 },
    xaxis2: {
      domain: [0.3, 0],
      anchor: 'y2',
      showgrid: false,
      zeroline: false,
      showticklabels: false,
      showline: false
    },
    yaxis2: {
      domain: [0, 0.3],
      anchor: 'x2',
      showgrid: false,
      zeroline: false,
      showticklabels: false,
      showline: false
    }
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false,
    staticPlot: true
  };

  Plotly.newPlot(elementId, [numberTrace, trendTrace], layout, config);
}

/**
 * Creates a categories donut chart
 */
function createCategoriesChart(elementId: string, data: CategoriesData) {
  const colors = [
    '#344e41', '#3a5a40', '#588157', '#a3b18a', '#415a77'
  ];

  let smallCategories = data.categories.sort((a,b) => a.value - b.value).slice(0, data.categories.length - 5);
  data.categories = data.categories.sort((a,b) => b.value - a.value).slice(0,5);
  if(data.categories[4] != null){
    data.categories[4].name = "Others";
    data.categories[4].value = 0;
    console.log(smallCategories);
    console.log(data.categories);
    smallCategories.forEach(element => {
      data.categories[4].value += element.value;
    });
  }
  
  
  const trace: Partial<Plotly.PlotData> = {
    type: 'pie',
    labels: data.categories.map(cat => cat.name),
    values: data.categories.map(cat => cat.value),
    hole: 0.6,
    marker: {
      colors: colors,
      line: { color: 'rgb(41,42,47)', width: 4 }
    },
    domain: {
      x: [0, 0.65]
    },
    textinfo: 'none',
    hovertemplate: '<b>%{label}</b><br>%{percent}<extra></extra>',
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      text: data.title,
      font: { size: 24, color: '#A0AEC0' },
      x: 0.05,
      y: 0.95,
      xanchor: 'left'
    },
    height: 400,
    width: 460,
    annotations: data.categories.map((cat, i) => ({
      x: 0.7, // Horizontal position of the labels
      y: 0.8 - (i * 0.15), // Vertical position, stacking them down
      xanchor: 'left', // Anchor text to the left
      yanchor: 'top',
      align: 'left',
      // The text includes a colored dot and the category name
      text: `<span style="color:${colors[i]}; font-size: 20px;">●</span> <span style="position: relative; top: -4px;">${cat.name}</span>`,
      showarrow: false,
      font: { size: 16, color: 'White', weight: 600},
    }))
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a top sites horizontal bar chart
 */
function createTopSitesChart(elementId: string, data: TopSitesData) {
  // Plotly draws horizontal bars from the bottom up, so we reverse the data
  // to show the #1 site at the top of the chart.
  const reversedSites = [...data.sites].reverse();
  const maxTimeSpent = Math.max(...reversedSites.map(site => site.timeSpent));

  const trace: Partial<Plotly.PlotData> = {
    type: 'bar',
    orientation: 'h',
    x: reversedSites.map(site => site.timeSpent),
    y: reversedSites.map(site => site.url), // The URLs remain as the y-axis labels
      marker: {
        color: '#415a77',
        cornerradius: 10 
    } as any // <-- Add 'as any' to bypass the type check
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      // Style the title to match the target image
      text: '<b>TOP SITES</b>',
      font: { size: 18, color: '#E2E8F0' },
      x: 0.03,
      y: 0.95,
      xanchor: 'left'
    },
    height: 400,
    width: 460,
    // Use a larger left margin as a starting point.
    // `automargin` below will expand it if needed.
    // Adjust the gap to make bars thinner and increase spacing
    bargap: 0.5,
    xaxis: {
      // Hide the x-axis completely
      visible: false,
      range: [-maxTimeSpent * 0.15, maxTimeSpent],
    },
    yaxis: {
      // This is the key fix: it tells Plotly to automatically adjust
      // the margin to prevent labels from ever being cut off.
      automargin: true,
      // Hide the axis line and tick marks for a cleaner look
      showline: false,
      ticks: '',
      // Style the labels to match the target
      tickfont: {
        size: 14,
        color: '#A0AEC0'
      }
    }
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a time spent line chart
 */
function createTimeSpentChart(elementId: string, data: TimeSpentData) {
  const trace: Partial<Plotly.PlotData> = {
    type: 'scatter',
    mode: 'lines+markers',
    x: data.dates,
    y: data.values,
    line: { color: '#6395F2', width: 3 },
    marker: { color: '#6395F2', size: 6 },
    fill: 'tonexty',
    fillcolor: 'rgba(99, 149, 242, 0.1)'
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' },
      x: 0.02,
      y: 0.95,
      xanchor: 'left'
    },
    xaxis: {
      showgrid: false,
      zeroline: false,
      showline: false,
      tickfont: { size: 10 }
    },
    yaxis: {
      showgrid: true,
      gridcolor: '#2D3748',
      zeroline: false,
      showline: false,
      tickfont: { size: 10 },
      ticksuffix: 'h'
    }
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a daily usage heatmap
 */
function createDailyUsageHeatmap(elementId: string, data: DailyUsageData) {
  const trace: Partial<Plotly.PlotData> = {
    type: 'heatmap',
    z: data.heatmapData,
    x: data.dayLabels,
    y: data.timeLabels,
    colorscale: [
      [0, '#2D3748'],
      [0.3, '#4A5568'],
      [0.6, '#6395F2'],
      [1, '#3182CE']
    ],
    showscale: false,
    hoverongaps: false,
    hovertemplate: 'Day: %{x}<br>Time: %{y}<br>Usage: %{z}<extra></extra>'
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      text: data.title,
      font: { size: 14, color: '#A0AEC0' },
      x: 0.02,
      y: 0.95,
      xanchor: 'left'
    },
    height: 200,
    xaxis: {
      showgrid: false,
      zeroline: false,
      showline: false,
      tickfont: { size: 10 },
      side: 'top'
    },
    yaxis: {
      showgrid: false,
      zeroline: false,
      showline: false,
      tickfont: { size: 10 },
      autorange: 'reversed'
    }
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

class ChartData{
  public Labels: string[] = [];
  public Datas: number[] = [];

  public Add(datas?: number[], labels?: string[]): void{
    if(labels) this.Labels.push(...labels);
    if(datas) this.Datas.push(...datas);
  }
}

function getData(): SiteVisit[]{
  return [
  {
    "siteUrl": "https://github.com/features",
    "baseProductiveScore": 95,
    "timeSpent": 45.78,
    "mainCategory": "Tech"
  },
  {
    "siteUrl": "https://stackoverflow.com/questions/12345/how-to-center-a-div",
    "baseProductiveScore": 98,
    "timeSpent": 15.21,
    "mainCategory": "Tech"
  },
  {
    "siteUrl": "https://developer.mozilla.org/en-US/docs/Web/JavaScript",
    "baseProductiveScore": 95,
    "timeSpent": 62.55,
    "mainCategory": "Tech"
  },
  {
    "siteUrl": "https://aws.amazon.com/console",
    "baseProductiveScore": 90,
    "timeSpent": 121.89,
    "mainCategory": "Tech"
  },
  {
    "siteUrl": "https://mail.google.com/mail/u/0/#inbox",
    "baseProductiveScore": 90,
    "timeSpent": 55.1,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://docs.google.com/spreadsheets/d/1a2b3c",
    "baseProductiveScore": 95,
    "timeSpent": 88.43,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://calendar.google.com/calendar/r",
    "baseProductiveScore": 95,
    "timeSpent": 12.3,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://app.slack.com/client/T012345/C67890",
    "baseProductiveScore": 90,
    "timeSpent": 150.67,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://www.linkedin.com/feed/",
    "baseProductiveScore": 65,
    "timeSpent": 25.98,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://trello.com/b/boardid/project-alpha",
    "baseProductiveScore": 98,
    "timeSpent": 76.11,
    "mainCategory": "Productivity"
  },
  {
    "siteUrl": "https://www.facebook.com/",
    "baseProductiveScore": 10,
    "timeSpent": 48.23,
    "mainCategory": "Social Media"
  },
  {
    "siteUrl": "https://twitter.com/home",
    "baseProductiveScore": 15,
    "timeSpent": 33.7,
    "mainCategory": "Social Media"
  },
  {
    "siteUrl": "https://www.instagram.com/",
    "baseProductiveScore": 5,
    "timeSpent": 68.14,
    "mainCategory": "Social Media"
  },
  {
    "siteUrl": "https://www.reddit.com/r/funny",
    "baseProductiveScore": 5,
    "timeSpent": 51.5,
    "mainCategory": "Social Media"
  },
  {
    "siteUrl": "https://www.reddit.com/r/programming",
    "baseProductiveScore": 70,
    "timeSpent": 42.88,
    "mainCategory": "Tech"
  },
  {
    "siteUrl": "https://www.youtube.com/",
    "baseProductiveScore": 20,
    "timeSpent": 95.32,
    "mainCategory": "Entertainment"
  },
  {
    "siteUrl": "https://www.youtube.com/watch?v=some_educational_video",
    "baseProductiveScore": 80,
    "timeSpent": 28.6,
    "mainCategory": "Education"
  },
  {
    "siteUrl": "https://www.netflix.com/browse",
    "baseProductiveScore": 5,
    "timeSpent": 125.0,
    "mainCategory": "Entertainment"
  },
  {
    "siteUrl": "https://open.spotify.com/",
    "baseProductiveScore": 50,
    "timeSpent": 240.15,
    "mainCategory": "Entertainment"
  },
  {
    "siteUrl": "https://www.nytimes.com/",
    "baseProductiveScore": 55,
    "timeSpent": 18.9,
    "mainCategory": "News"
  },
  {
    "siteUrl": "https://www.bbc.com/news",
    "baseProductiveScore": 55,
    "timeSpent": 22.45,
    "mainCategory": "News"
  },
  {
    "siteUrl": "https://en.wikipedia.org/wiki/Machine_learning",
    "baseProductiveScore": 85,
    "timeSpent": 31.23,
    "mainCategory": "Reference"
  },
  {
    "siteUrl": "https://www.coursera.org/learn/machine-learning",
    "baseProductiveScore": 98,
    "timeSpent": 112.8,
    "mainCategory": "Education"
  },
  {
    "siteUrl": "https://www.udemy.com/course/the-web-developer-bootcamp/",
    "baseProductiveScore": 95,
    "timeSpent": 78.34,
    "mainCategory": "Education"
  },
  {
    "siteUrl": "https://www.khanacademy.org/math/algebra",
    "baseProductiveScore": 100,
    "timeSpent": 49.99,
    "mainCategory": "Education"
  },
  {
    "siteUrl": "https://www.amazon.com/deals",
    "baseProductiveScore": 15,
    "timeSpent": 19.55,
    "mainCategory": "Shopping"
  },
  {
    "siteUrl": "https://www.etsy.com/",
    "baseProductiveScore": 15,
    "timeSpent": 24.05,
    "mainCategory": "Shopping"
  },
  {
    "siteUrl": "https://www.wsj.com/news/markets",
    "baseProductiveScore": 75,
    "timeSpent": 35.6,
    "mainCategory": "Finance"
  },
  {
    "siteUrl": "https://www.bloomberg.com/",
    "baseProductiveScore": 80,
    "timeSpent": 41.2,
    "mainCategory": "Finance"
  },
  {
    "siteUrl": "https://www.expedia.com/",
    "baseProductiveScore": 25,
    "timeSpent": 17.76,
    "mainCategory": "Travel"
  },
  {
    "siteUrl": "https://www.airbnb.com/",
    "baseProductiveScore": 25,
    "timeSpent": 30.12,
    "mainCategory": "Travel"
  },
  {
    "siteUrl": "https://www.webmd.com/",
    "baseProductiveScore": 40,
    "timeSpent": 8.95,
    "mainCategory": "Health"
  },
  {
    "siteUrl": "https://www.nih.gov/",
    "baseProductiveScore": 85,
    "timeSpent": 29.8,
    "mainCategory": "Health"
  },
  {
    "siteUrl": "https://www.figma.com/files/project/12345/design-system",
    "baseProductiveScore": 97,
    "timeSpent": 180.3,
    "mainCategory": "Design"
  }
]
}

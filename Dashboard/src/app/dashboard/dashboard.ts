import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Api, GameStat } from '../services/api';
import { Observable } from 'rxjs';
import * as Plotly from 'plotly.js-dist-min';
import { DashboardData, PointsData, ExpGaugeData, LevelData, StreakData, CategoriesData, TopSitesData, TimeSpentData, DailyUsageData, HeatmapData, ProductivityLog, DailyAnalyticsDTO } from '../services/dashboard-data';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})

export class Dashboard implements OnInit{
  private apiService = inject(Api);
  private dashboardService = inject(DashboardData);

  chartData: ChartData = new ChartData();

  public stat$?: Observable<GameStat>;
  public dailyAnalytics$? : Observable<DailyAnalyticsDTO[]>;
  public productivityLogs$? : Observable<ProductivityLog[]>;

  ngOnInit(): void{
    this.stat$ = this.apiService.getDashboardStat();
    this.dailyAnalytics$ = this.apiService.getUserSiteVisits();
    this.productivityLogs$ = this.apiService.getProductivityLogs();

    this.chartData.Labels =  ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'];
    this.chartData.Datas = [5,7,8,3,5,2,29];
    this.chartData.Add([],["Pink"]);

    this.stat$.subscribe(result => {
      createPointsDisplay('pointsCard', this.dashboardService.getPointsData(result));
      createExpGauge('expCard', this.dashboardService.getExpData(result));
      createLevelDisplay('levelCard', this.dashboardService.getLevelData(result));
    })

    this.dailyAnalytics$.subscribe(analytics => {
      if (!analytics || analytics.length === 0) {
        console.log("No site visit analytics data received.");
        return; // Exit if there's no data
      }

      // --- Logic for Daily Charts (Categories, Top Sites) ---
      // Sort to find the most recent day's data
      const mostRecentDay = analytics.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())[0];

      if (mostRecentDay && mostRecentDay.siteVisits) {
        // Pass ONLY the most recent day's visits to these charts
        createCategoriesChart('by-category', this.dashboardService.getCategoriesData(mostRecentDay.siteVisits));
        createTopSitesChart('by-top-sites', this.dashboardService.getTopSitesData(mostRecentDay.siteVisits, 5));
      }

      // --- Logic for Time-Series Chart (Time Spent) ---
      // Pass the ENTIRE array of daily analytics to these charts
      const totalTimeData = this.dashboardService.getTimeSpentData(analytics);
      const productiveTimeData = this.dashboardService.getProductiveTimeSpentData(analytics);
      createTimeSpentChart('by-time-spent', totalTimeData, productiveTimeData);
    });

    this.productivityLogs$.subscribe(result => {
      let processedData = this.dashboardService.processDataForRolling30Days(result);
      createPlotlyHeatmap('heatmap-container', processedData);
    });

    
    
  }
}
// --- Charting Functions ---
function formatUrlForDisplay(url: string): string {
  if (!url) {
    return '';
  }
  
  const parts = url.split('.');

  let domainName = '';

  // If there are 3 or more parts (e.g., 'app.slack.com'), we want the second-to-last one.
  // If there are 2 parts (e.g., 'github.com'), we also want the second-to-last one (which is the first part).
  // This logic correctly handles both cases.
  if (parts.length >= 2) {
    domainName = parts[parts.length - 2];
  } else {
    // Fallback for single-word domains like 'localhost' or an unexpected format.
    domainName = parts[0];
  }

  // Capitalize the first letter and return
  return domainName.charAt(0).toUpperCase() + domainName.slice(1);
}
// Shared dark theme configuration
const darkThemeLayout: Partial<Plotly.Layout> = {
  paper_bgcolor: '#292A2F',
  plot_bgcolor: '#292A2F',
  font: {
    color: '#A0AEC0',
    family: 'Arial, sans-serif',
    size: 12,
  },
  margin: { l: 70, r: 40, t: 70, b: 40 },
  showlegend: false,
};

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
    '#549BE4', '#48CFAF', '#F5B95A', '#E47C7C', '#9D8CF0'
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
      line: { color: 'rgb(41,42,47)', width: 2 }
    },
    domain: {
      x: [0, 0.7]
    },
    textinfo: 'none',
    hovertemplate: '<b>%{label}</b><br>%{percent}<extra></extra>',
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      text: '<b>CATEGORY</b>',
      font: { size: 18, color: '#ffffff' },
      x: 0.05,
      y: 0.95,
      xanchor: 'left'
    },
    annotations: data.categories.map((cat, i) => ({
      x: 0.7, // Horizontal position of the labels
      y: 0.9 - (i * 0.07), // Vertical position, stacking them down
      xanchor: 'left', // Anchor text to the left
      yanchor: 'top',
      align: 'left',
      // The text includes a colored dot and the category name
      text: `<span style="color:${colors[i]}; font-size: 16px;">●</span> <span style="position: relative; top: -4px;">${cat.name}</span>`,
      showarrow: false,
      font: { size: 12, color: 'White', weight: 600},
    })),
    margin:{
      l: 40,
      t: 40
    }
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
    y: reversedSites.map(site => formatUrlForDisplay(site.url)), // The URLs remain as the y-axis labels
      marker: {
        color: '#F6806F',
        cornerradius: 10 
    } as any, // <-- Add 'as any' to bypass the type check
    hoverlabel:{
      font: {color: '#ffffffff', weight: 'bold'},
      // bgcolor: '#ae4231ff'
    },
    hovertemplate: '%{x}min<extra></extra>'
  };

  const layout: Partial<Plotly.Layout> = {
    ...darkThemeLayout,
    title: {
      // Style the title to match the target image
      text: '<b>TOP SITES TODAY</b>',
      font: { size: 18, color: '#E2E8F0' },
      x: 0.03,
      y: 0.95,
      xanchor: 'left'
    },
    bargap: 0.5,
    xaxis: {
      title: {text: "Minute"},
      showgrid: true,
      // Hide the x-axis completely
      // visible: false,
      range: [-maxTimeSpent * 0.15, maxTimeSpent],
    },
    yaxis: {
      showgrid: true,
      automargin: true,
      // Hide the axis line and tick marks for a cleaner look
      showline: false,
      // Style the labels to match the target
      tickfont: {
        size: 14,
        color: '#ffffffff'
      },
    },
  };

  const config: Partial<Plotly.Config> = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace], layout, config);
}

/**
 * Creates a time spent line chart
 */
function createTimeSpentChart(elementId: string, overallData: TimeSpentData, productiveTimeData: TimeSpentData) {
    // Trace for Productive Time (the bottom line)
  const productiveTrace: Partial<Plotly.Data> = {
    type: 'scatter',
    mode: 'lines+markers',
    // x: productiveTimeData.dates.map(convertDate),
    x: productiveTimeData.dates,
    y: productiveTimeData.values.map(d => d / 3600),
    line: { color: '#43E1CB', width: 3, shape: 'spline' }, // A nice Google Blue
    marker: { color: '#43E1CB', size: 8 },
    name: "Productive time",
    hovertemplate: '<b>Productive time</b>: %{y:.1f} hr<extra></extra>'
    // No fill property here
  };

  // Trace for Total Time (the top line)
  const totalTrace: Partial<Plotly.Data> = {
    type: 'scatter',
    mode: 'lines+markers',
    // x: overallData.dates.map(convertDate),
    x: overallData.dates,
    y: overallData.values.map(d => d / 3600),
    line: { color: '#F9806E', width: 3, shape: 'spline' }, // A neutral Google Grey
    marker: { color: '#F9806E', size: 8 },
    // This fills the area BETWEEN this trace and the one before it (productiveTrace)
    fill: 'tonexty',
    fillcolor: 'rgba(90, 43, 43, 0.2)', // A reddish color for unproductive time
    name: "Total time",
    hovertemplate: '<b>Total time</b>: %{y:.1f} hr<extra></extra>'
  };

  const layout: Partial<Plotly.Layout> = {
    plot_bgcolor: darkThemeLayout.plot_bgcolor,
    paper_bgcolor: darkThemeLayout.paper_bgcolor,
    title: {
      text: overallData.title,
      font: { size: 18, color: '#E2E8F0', family: 'Arial, sans-serif' },
      x: 0.02,
      y: 0.95,
      xanchor: 'left',
    },
    xaxis: {
      showgrid: false,
      zeroline: false,
      showline: false,
      tickfont: { size: 12, color: '#A0AEC0' },
      type: 'date',
      range: (() => {
        const now = new Date();
        const start = new Date(now);
        start.setDate(now.getDate() - 7); // 5 days before today
        const end = new Date(now);
        end.setDate(now.getDate());   // 5 days after today
        return [start.toISOString(), end.toISOString()];
      })(),
      rangeslider: { visible: true },  // 👈 allows scrolling
      rangemode: 'normal',
    },
    yaxis: {
      showgrid: true,
      gridcolor: '#2D3748',
      zeroline: false,
      showline: false,
      tickfont: { size: 12, color: '#A0AEC0' },
      ticksuffix: ' min',
    },
    legend: {
      font: { size: 12, color: '#E2E8F0' },
      x: 1,
      xanchor: 'right',
      y: 1
    },
    hovermode: 'x unified',
    hoverlabel: {
      bgcolor: '#2D3748',
      bordercolor: '#4A5568',
      font: {
        size: 13,
        color: '#E2E8F0'
      },
      namelength: -1
    },
    margin: darkThemeLayout.margin
  };

  const config = {
    displayModeBar: false,
    responsive: true
  };

  Plotly.newPlot(elementId, [productiveTrace, totalTrace], layout, config);
}

function createPlotlyHeatmap(
    chartId: string,
    heatmapData: HeatmapData,
    showValuesOnCells: boolean = true // Defaulting to true as it's a common request
): void {

    const trace: Plotly.Data = {
        x: heatmapData.xValues,
        y: heatmapData.yValues,
        z: heatmapData.zValues,
        type: 'heatmap',
        hoverinfo: 'text', // Use the content of 'hovertext' for tooltips
        hovertext: heatmapData.hoverText as any, // This is correct, keep the type assertion
        colorscale: [
          [0.0, 'rgb(230, 255, 230)'],
          [0.0625, 'rgb(212, 245, 212)'],
          [0.125, 'rgb(194, 235, 194)'],
          [0.1875, 'rgb(176, 225, 176)'],
          [0.25, 'rgb(158, 215, 158)'],
          [0.3125, 'rgb(140, 205, 140)'],
          [0.375, 'rgb(122, 195, 122)'],
          [0.4375, 'rgb(104, 185, 104)'],
          [0.5, 'rgb(86, 175, 86)'],
          [0.5625, 'rgb(68, 165, 68)'],
          [0.625, 'rgb(50, 155, 50)'],
          [0.6875, 'rgb(32, 145, 32)'],
          [0.75, 'rgb(14, 135, 14)'],
          [0.8125, 'rgb(0, 125, 0)'],
          [0.875, 'rgb(0, 115, 0)'],
          [0.9375, 'rgb(0, 105, 0)'],
          [1.0, 'rgb(0, 95, 0)']
        ],
        showscale: false,
        hoverongaps: false,
    };

    // --- THIS IS THE KEY FIX ---
    // Conditionally set the texttemplate to format the z-value.
    if (showValuesOnCells) {
        // This template tells Plotly:
        // 1. Check the z-value.
        // 2. If it's greater than 0, display it with 1 decimal place followed by 'h'.
        // 3. If it's 0 or null, display nothing ('').
        // trace.texttemplate = '%{z: >.1f}h'; // e.g., "2.5h", "4.0h"
        hovertemplate: '%{z:.1f}h<extra></extra>';
        
        // This ensures that days with 0 hours don't have "0.0h" cluttering the view.
        // We'll also update the textfont color based on the cell's value for readability.
        trace.textfont = {
            family: 'Arial, sans-serif',
            size: 12,
            // Automatically make the font dark on light cells and light on dark cells
            color: 'auto'
        };
    }

    const data: Plotly.Data[] = [trace];

    const layout: Partial<Plotly.Layout> = {
        title: {text:'Monthly Productivity Heatmap', font: { size: 18, color: '#E2E8F0', family: 'Arial, sans-serif' },},
        xaxis: { side: 'top', ticks: '', showgrid: false, tickfont: { size: 12, color: '#ffffffff' }, },
        yaxis: { ticks: '', showgrid: false, autorange: 'reversed', tickfont: { size: 12, color: '#ffffffff' } }, // Keep autorange reversed
        plot_bgcolor: darkThemeLayout.plot_bgcolor,
        paper_bgcolor: darkThemeLayout.paper_bgcolor,
        margin:{
          l: 80,  // left margin
          r: 40,  // right margin
          b: 40,  // bottom margin
          t: 80,  // top margin
          pad: 4
        }
    };

    const config: Partial<Plotly.Config> = { responsive: true };

    Plotly.newPlot(chartId, data, layout, config);
}

class ChartData{
  public Labels: string[] = [];
  public Datas: number[] = [];

  public Add(datas?: number[], labels?: string[]): void{
    if(labels) this.Labels.push(...labels);
    if(datas) this.Datas.push(...datas);
  }
}

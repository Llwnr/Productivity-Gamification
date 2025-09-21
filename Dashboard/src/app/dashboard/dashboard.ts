import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Api, GameStat, SiteVisit } from '../services/api';
import { Observable } from 'rxjs';
import * as Plotly from 'plotly.js-dist-min';
import { DashboardData, PointsData, ExpGaugeData, LevelData, StreakData, CategoriesData, TopSitesData, TimeSpentData, DailyUsageData } from '../services/dashboard-data';

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
  public siteVisits$? : Observable<SiteVisit[]>;

  public dummyData: SiteVisit[] = this.dashboardService.getData();

  ngOnInit(): void{
    this.stat$ = this.apiService.getDashboardStat();
    this.siteVisits$ = this.apiService.getUserSiteVisits();

    this.chartData.Labels =  ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'];
    this.chartData.Datas = [5,7,8,3,5,2,29];
    this.chartData.Add([],["Pink"]);

    this.stat$.subscribe(result => {
      createPointsDisplay('pointsCard', this.dashboardService.getPointsData(result));
      createExpGauge('expCard', this.dashboardService.getExpData(result));
      createLevelDisplay('levelCard', this.dashboardService.getLevelData(result));
    })

    createStreakDisplay('streakCard', this.dashboardService.getStreakData());

    createCategoriesChart('by-category', this.dashboardService.getCategoriesData(this.dummyData));
    createTopSitesChart('by-top-sites', this.dashboardService.getTopSitesData(this.dummyData, 5));
    createTimeSpentChart('by-time-spent', this.dashboardService.getTimeSpentData(this.dummyData), this.dashboardService.getProductiveTimeSpentData(this.dummyData));

    this.siteVisits$.subscribe(result => {
      createCategoriesChart('by-category', this.dashboardService.getCategoriesData(result));
      createTopSitesChart('by-top-sites', this.dashboardService.getTopSitesData(result, 5));
      createTimeSpentChart('by-time-spent', this.dashboardService.getTimeSpentData(result), this.dashboardService.getProductiveTimeSpentData(result));
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
        color: '#5A82A8',
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
function convertDate(dateString: string) {
  const parts = dateString.split('/');
  const month = parseInt(parts[0], 10);
  const day = parseInt(parts[1], 10);

  const date = new Date(2000, month - 1, day);
  const monthName = date.toLocaleString('default', { month: 'short' });

  return `${monthName}-${day}`;
}

function createTimeSpentChart(elementId: string, overallData: TimeSpentData, productiveTimeData: TimeSpentData) {
  const trace: Partial<Plotly.Data> = {
    type: 'scatter',
    mode: 'lines+markers',
    x: overallData.dates.map(convertDate),
    y: overallData.values.map(d => d / 60),
    line: { color: '#8E9BFF', width: 3 },
    marker: { color: '#8E9BFF', size: 8, symbol: 'circle' },
    fill: 'tonexty',
    fillcolor: 'rgba(142, 155, 255, 0.2)',
    name: "Total time",
    hovertemplate: '<b>Total time</b>: %{y:.1f} min<extra></extra>'
  };

  const trace1: Partial<Plotly.Data> = {
    type: 'scatter',
    mode: 'lines+markers',
    x: productiveTimeData.dates.map(convertDate),
    y: productiveTimeData.values.map(d => d / 60),
    line: { color: '#FFC107', width: 3 },
    marker: { color: '#FFC107', size: 8, symbol: 'circle' },
    fill: 'tonexty',
    fillcolor: 'rgba(255, 193, 7, 0.2)',
    name: "Productive time",
    hovertemplate: '<b>Productive time</b>: %{y:.1f} min<extra></extra>'
  };

  const layout: Partial<Plotly.Layout> = {
    width: 970,
    plot_bgcolor: darkThemeLayout.plot_bgcolor,
    paper_bgcolor: darkThemeLayout.paper_bgcolor,
    title: {
      text: overallData.title,
      font: { size: 18, color: '#E2E8F0', family: 'Arial, sans-serif' },
      x: 0.02,
      y: 0.95,
      xanchor: 'left'
    },
    xaxis: {
      showgrid: false,
      zeroline: false,
      showline: false,
      tickfont: { size: 12, color: '#A0AEC0' },
      range: [-0.5, 10],
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
    }
  };

  const config = {
    displayModeBar: false
  };

  Plotly.newPlot(elementId, [trace1, trace], layout, config);
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

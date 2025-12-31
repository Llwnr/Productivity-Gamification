import { Component, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable,map } from 'rxjs';
import { DailyAnalyticsDTO, ProductivityLog } from './dashboard-data';

export interface GameStat{
  username: string,
  coin: number,
  experiencePoints: number,
  nextLvlPercentage: string,
  level: number,
  totalAchievements: number,
  dailyStreak: number,
  weeklyStreak: number
}

export interface Achievement {
  title: string;
  description: string;
  earnedAt: string;
}

@Injectable({
  providedIn: 'root'
})

export class Api {
  private http = inject(HttpClient);
  private apiUrl = "https://localhost:7131"

  getDashboardStat(): Observable<GameStat>{
    const options = {withCredentials: true};
    return this.http.get<GameStat>(`${this.apiUrl}/Dashboard/UserStat`, options);
  }

  getUserSiteVisits(): Observable<DailyAnalyticsDTO[]>{
    const options = {withCredentials: true};
    return this.http.get<DailyAnalyticsDTO[]>(`${this.apiUrl}/Dashboard/Analytics`, options);
  }

  getProductivityLogs(): Observable<ProductivityLog[]>{
    const options = {withCredentials: true};
    return this.http.get<ProductivityLog[]>(`${this.apiUrl}/Dashboard/ProductivityLogs`, options);
  }

  getUserAchievements(): Observable<Achievement[]> {
    const options = {withCredentials: true};
    return this.http.get<Achievement[]>(`${this.apiUrl}/Dashboard/Achievements`, options);
  }

  logout() {
    // The options object with withCredentials
    const options = {
      withCredentials: true
    };

    // Use .post() and subscribe to send the request
    this.http.post(`${this.apiUrl}/Authentication/Logout`, null, options) // Use null for the body if it's empty
      .subscribe({
        next: () => console.log('Logout successful'),
        error: (err) => console.error('Logout failed:', err)
      });
  }
}

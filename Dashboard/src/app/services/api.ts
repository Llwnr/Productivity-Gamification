import { Component, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable,map } from 'rxjs';
import { DailyAnalyticsDTO, ProductivityLog } from './dashboard-data';

export interface GameStat{
  coin: number,
  experiencePoints: number,
  level: number
}

@Injectable({
  providedIn: 'root'
})

export class Api {
  private http = inject(HttpClient);
  private apiUrl = "https://localhost:7131"

  getDashboardStat(): Observable<GameStat>{
    const options = {withCredentials: true};
    return this.http.get<GameStat[]>(`${this.apiUrl}/Dashboard/UserStat`, options).pipe(map(array => array[0]));
  }

  getUserSiteVisits(): Observable<DailyAnalyticsDTO[]>{
    const options = {withCredentials: true};
    return this.http.get<DailyAnalyticsDTO[]>(`${this.apiUrl}/Dashboard/Analytics`, options);
  }

  getProductivityLogs(): Observable<ProductivityLog[]>{
    const options = {withCredentials: true};
    return this.http.get<ProductivityLog[]>(`${this.apiUrl}/Dashboard/ProductivityLogs`, options);
  }
}

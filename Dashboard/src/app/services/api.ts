import { Component, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable,map } from 'rxjs';

export interface GameStat{
  coin: number,
  experiencePoints: number,
  level: number
}

export interface SiteVisit{
  siteUrl: string,
  baseProductiveScore: number
  timeSpent: number,
}

@Injectable({
  providedIn: 'root'
})

export class Api {
  private http = inject(HttpClient);
  private apiUrl = "https://localhost:7131"

  getDashboardStat(): Observable<GameStat>{
    return this.http.get<GameStat[]>(`${this.apiUrl}/Dashboard/UserStat`).pipe(map(array => array[0]));
  }

  getUserSiteVisits(): Observable<SiteVisit[]>{
    return this.http.get<SiteVisit[]>(`${this.apiUrl}/Dashboard/Analytics`);
  }
}

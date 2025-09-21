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
  mainCategory: string,
  visitDate: string
}

@Injectable({
  providedIn: 'root'
})

export class Api {
  private http = inject(HttpClient);
  private apiUrl = "https://localhost:7131"

  getDashboardStat(): Observable<GameStat>{
    const options = {
      withCredentials: true
    }
    return this.http.get<GameStat[]>(`${this.apiUrl}/Dashboard/UserStat`, options).pipe(map(array => array[0]));
  }

  getUserSiteVisits(): Observable<SiteVisit[]>{
    const options = {
      withCredentials: true
    }
    return this.http.get<SiteVisit[]>(`${this.apiUrl}/Dashboard/Analytics`, options);
  }
}

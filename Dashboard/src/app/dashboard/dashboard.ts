import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Api, GameStat, SiteVisit } from '../services/api';
import { Observable } from 'rxjs';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit{
  private apiService = inject(Api);

  public stat$?: Observable<GameStat>;
  public siteVisits$? : Observable<SiteVisit[]>;

  ngOnInit(): void{
    this.stat$ = this.apiService.getDashboardStat();
    this.siteVisits$ = this.apiService.getUserSiteVisits();
  }
}

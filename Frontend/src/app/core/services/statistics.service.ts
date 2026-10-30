import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_URL } from '../api-config';
import { ForecastResultDto, StatisticsSummaryDto } from '../models/statistics.models';

@Injectable({
  providedIn: 'root'
})
export class StatisticsService {
 constructor(private http: HttpClient) { }

  getSummary(from: string | null, to: string | null): Observable<StatisticsSummaryDto> {
    let params = new HttpParams();

    if (from) {
      params = params.set('from', from);
    }

    if (to) {
      params = params.set('to', to);
    }

    return this.http.get<StatisticsSummaryDto>(`${API_URL}/Statistics/summary`, { params });
  }

  getForecast(): Observable<ForecastResultDto> {
    return this.http.get<ForecastResultDto>(`${API_URL}/Statistics/forecast`);
  }

  generateForecast(days: number): Observable<ForecastResultDto> {
    const params = new HttpParams().set('days', days);
    return this.http.post<ForecastResultDto>(`${API_URL}/Statistics/forecast`, null, { params });
  }
}

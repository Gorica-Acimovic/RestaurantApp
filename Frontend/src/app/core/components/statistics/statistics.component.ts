import { Component, OnInit } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { StatisticsService } from '../../services/statistics.service';
import { StatisticsSummaryDto } from '../../models/statistics.models';
import { CURRENCY } from '../../currency';
import { ForecastComponent } from '../forecast/forecast.component';

@Component({
  selector: 'app-statistics',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DecimalPipe,
    MatFormFieldModule,
    MatInputModule,
    BaseChartDirective,
    ForecastComponent
  ],
  templateUrl: './statistics.component.html',
  styleUrl: './statistics.component.css'
})
export class StatisticsComponent implements OnInit {
  summary: StatisticsSummaryDto | null = null;
  errorMessage = '';
  readonly currency = CURRENCY;

  fromControl = new FormControl('', { nonNullable: true });
  toControl = new FormControl('', { nonNullable: true });

  dailyChart: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  weekdayChart: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };
  hourChart: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  lineOptions: ChartConfiguration<'line'>['options'];
  barOptions: ChartConfiguration<'bar'>['options'];
  horizontalBarOptions: ChartConfiguration<'bar'>['options'];

  private accent = '';

  constructor(private statisticsService: StatisticsService) {
    this.accent = this.cssColor('--accent');
    const muted = this.cssColor('--muted');
    const grid = this.cssColor('--border');

    const scales = {
      x: { ticks: { color: muted, maxTicksLimit: 12 }, grid: { color: grid } },
      y: { beginAtZero: true, ticks: { color: muted, precision: 0 }, grid: { color: grid } }
    };

    this.lineOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales
    };

    this.barOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales
    };

    this.horizontalBarOptions = {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: 'y',
      plugins: { legend: { display: false } },
      scales: {
        x: { beginAtZero: true, ticks: { color: muted }, grid: { color: grid } },
        y: { ticks: { color: muted }, grid: { color: grid } }
      }
    };
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.errorMessage = '';

    this.statisticsService
      .getSummary(this.fromControl.value || null, this.toControl.value || null)
      .subscribe({
        next: (data) => {
          this.summary = data;
          this.fromControl.setValue(data.from, { emitEvent: false });
          this.toControl.setValue(data.to, { emitEvent: false });
          this.buildCharts(data);
        },
        error: (err) => {
          this.summary = null;
          this.errorMessage = err.error?.message ?? 'Could not load statistics.';
        }
      });
  }

  setPreset(days: number): void {
    this.toControl.setValue(this.isoDate(-1));
    this.fromControl.setValue(this.isoDate(-days));
    this.load();
  }

  private buildCharts(data: StatisticsSummaryDto): void {
    this.dailyChart = {
      labels: data.daily.map(d => this.shortDate(d.date)),
      datasets: [{
        data: data.daily.map(d => d.guests),
        label: 'Guests',
        borderColor: this.accent,
        backgroundColor: this.accent + '33',
        fill: true,
        tension: 0.25,
        pointRadius: 0
      }]
    };

    this.weekdayChart = {
      labels: data.weekdays.map(w => w.day),
      datasets: [{
        data: data.weekdays.map(w => w.averageGuests),
        label: 'Average guests',
        backgroundColor: this.accent
      }]
    };

    this.hourChart = {
      labels: data.hours.map(h => h.hour + ':00'),
      datasets: [{
        data: data.hours.map(h => h.orderCount),
        label: 'Orders',
        backgroundColor: this.accent
      }]
    };
  }

  private cssColor(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }

  private isoDate(offsetDays: number): string {
    const date = new Date();
    date.setDate(date.getDate() + offsetDays);
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
  }

  private shortDate(iso: string): string {
    const parts = iso.split('-');
    return `${parts[2]}.${parts[1]}.`;
  }
}
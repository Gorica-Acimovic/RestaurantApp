import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { ToastrService } from 'ngx-toastr';
import { StatisticsService } from '../../services/statistics.service';
import { ForecastResultDto } from '../../models/statistics.models';

@Component({
  selector: 'app-forecast',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    MatFormFieldModule,
    MatSelectModule,
    BaseChartDirective
  ],
  templateUrl: './forecast.component.html',
  styleUrl: './forecast.component.css'
})
export class ForecastComponent implements OnInit {
  result: ForecastResultDto | null = null;
  loaded = false;
  noForecast = false;
  sending = false;
  errorMessage = '';

  readonly dayOptions = [7, 8, 9, 10, 11, 12, 13, 14];
  daysControl = new FormControl(14, { nonNullable: true });

  forecastChart: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  backtestChart: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  chartOptions: ChartConfiguration<'line'>['options'];

  private actualColor = '';
  private ssaColor = '';
  private averageColor = '#7db8d4';

  constructor(private statisticsService: StatisticsService, private toastr: ToastrService) {
    this.actualColor = this.cssColor('--text');
    this.ssaColor = this.cssColor('--accent');
    const muted = this.cssColor('--muted');
    const grid = this.cssColor('--border');

    this.chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { labels: { color: muted } } },
      scales: {
        x: { ticks: { color: muted, maxTicksLimit: 14 }, grid: { color: grid } },
        y: { beginAtZero: true, ticks: { color: muted }, grid: { color: grid } }
      }
    };
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.errorMessage = '';

    this.statisticsService.getForecast().subscribe({
      next: (data) => {
        this.loaded = true;
        this.noForecast = false;
        this.result = data;
        this.buildCharts(data);
      },
      error: (err) => {
        this.loaded = true;
        this.result = null;

        if (err.status === 404) {
          this.noForecast = true;
        } else {
          this.errorMessage = err.error?.message ?? 'Could not load the forecast.';
        }
      }
    });
  }

  generate(): void {
    this.sending = true;
    this.errorMessage = '';

    this.statisticsService.generateForecast(this.daysControl.value).subscribe({
      next: (data) => {
        this.sending = false;
        this.noForecast = false;
        this.result = data;
        this.buildCharts(data);
        this.toastr.success('Forecast generated.');
      },
      error: (err) => {
        this.sending = false;
        this.errorMessage = err.error?.message ?? 'Something went wrong. Please try again.';
      }
    });
  }

  modelLabel(model: string): string {
    return model === 'Ssa' ? 'SSA (ML.NET)' : 'Average of the same weekday';
  }

  bestModel(): string | null {
    if (!this.result || this.result.metrics.length === 0) {
      return null;
    }

    return this.result.metrics.reduce((best, m) => (m.mae < best.mae ? m : best)).model;
  }

  weekday(iso: string): string {
    const parts = iso.split('-');
    const date = new Date(+parts[0], +parts[1] - 1, +parts[2]);
    return date.toLocaleDateString('en-GB', { weekday: 'long' });
  }

  private buildCharts(data: ForecastResultDto): void {
    const past = data.backtest;
    const future = data.forecast;
    const lastActual = past.length > 0 ? past[past.length - 1].actual : null;
    const gap = new Array<number | null>(Math.max(past.length - 1, 0)).fill(null);

    const actual: (number | null)[] = [...past.map(p => p.actual), ...future.map(() => null)];
    const ssa: (number | null)[] = [...gap, lastActual, ...future.map(f => f.ssa)];
    const average: (number | null)[] = [...gap, lastActual, ...future.map(f => f.weekdayAverage)];

    this.forecastChart = {
      labels: [...past.map(p => this.shortDate(p.date)), ...future.map(f => this.shortDate(f.date))],
      datasets: [
        { data: actual, label: 'Actual', borderColor: this.actualColor, backgroundColor: this.actualColor, tension: 0.2, pointRadius: 3 },
        { data: ssa, label: 'SSA forecast', borderColor: this.ssaColor, backgroundColor: this.ssaColor, tension: 0.2, pointRadius: 3 },
        { data: average, label: 'Weekday average', borderColor: this.averageColor, backgroundColor: this.averageColor, borderDash: [6, 4], tension: 0.2, pointRadius: 3 }
      ]
    };

    this.backtestChart = {
      labels: past.map(p => this.shortDate(p.date)),
      datasets: [
        { data: past.map(p => p.actual), label: 'Actual', borderColor: this.actualColor, backgroundColor: this.actualColor, tension: 0.2, pointRadius: 3 },
        { data: past.map(p => p.ssa), label: 'SSA', borderColor: this.ssaColor, backgroundColor: this.ssaColor, tension: 0.2, pointRadius: 3 },
        { data: past.map(p => p.weekdayAverage), label: 'Weekday average', borderColor: this.averageColor, backgroundColor: this.averageColor, borderDash: [6, 4], tension: 0.2, pointRadius: 3 }
      ]
    };
  }

  private cssColor(name: string): string {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  }

  private shortDate(iso: string): string {
    const parts = iso.split('-');
    return `${parts[2]}.${parts[1]}.`;
  }
}
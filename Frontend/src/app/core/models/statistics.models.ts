export interface WeekdayStatDto {
  day: string;
  averageGuests: number;
}

export interface HourStatDto {
  hour: number;
  orderCount: number;
}

export interface TopItemDto {
  menuItemId: number;
  name: string;
  quantity: number;
  revenue: number;
}

export interface DailyPointDto {
  date: string;
  guests: number;
  reservations: number;
  revenue: number;
  isSynthetic: boolean;
}

export interface StatisticsSummaryDto {
  from: string;
  to: string;
  days: number;
  containsSynthetic: boolean;
  totalReservations: number;
  totalGuests: number;
  totalOrders: number;
  totalRevenue: number;
  noShowCount: number;
  noShowPercent: number;
  weekdays: WeekdayStatDto[];
  hours: HourStatDto[];
  topItems: TopItemDto[];
  daily: DailyPointDto[];
}

export interface ForecastMetricDto {
  model: string;
  mae: number;
  mape: number;
}

export interface ForecastPointDto {
  date: string;
  ssa: number;
  weekdayAverage: number;
}

export interface BacktestPointDto {
  date: string;
  actual: number;
  ssa: number;
  weekdayAverage: number;
}

export interface ForecastResultDto {
  generatedAt: string;
  trainingFrom: string;
  trainingTo: string;
  trainingDays: number;
  containsSynthetic: boolean;
  backtestFolds: number;
  backtestDays: number;
  metrics: ForecastMetricDto[];
  forecast: ForecastPointDto[];
  backtest: BacktestPointDto[];
}
import { Component, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReviewService } from '../../services/review.service';
import { AuthService } from '../../services/auth.service';
import { ReviewSummaryDto } from '../../models/review.models';
import { stars } from '../../stars';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    DecimalPipe,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  summary: ReviewSummaryDto | null = null;
  errorMessage = '';
  loggedIn: boolean;

  constructor(private reviewService: ReviewService, private authService: AuthService) {
    this.loggedIn = this.authService.isLoggedIn();
  }

  ngOnInit(): void {
    this.reviewService.getSummary(5).subscribe({
      next: (data) => (this.summary = data),
      error: () => (this.errorMessage = 'Could not load reviews.')
    });
  }

  stars(rating: number): string {
    return stars(rating);
  }
}
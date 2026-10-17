import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ReviewService } from '../../services/review.service';
import { ReviewDto } from '../../models/review.models';
import { stars } from '../../stars';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './reviews.component.html',
  styleUrl: './reviews.component.css'
})
export class ReviewsComponent implements OnInit {
  reviews: ReviewDto[] = [];
  loaded = false;
  errorMessage = '';

  ratingControl = new FormControl<number | null>(null);
  unansweredControl = new FormControl(false, { nonNullable: true });
  readonly ratingValues = [5, 4, 3, 2, 1];

  respondingId: number | null = null;
  responseControl = new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(1000)] });

  constructor(private reviewService: ReviewService, private toastr: ToastrService) { }

  ngOnInit(): void {
    this.loadReviews();

    this.ratingControl.valueChanges.subscribe(() => this.loadReviews());
    this.unansweredControl.valueChanges.subscribe(() => this.loadReviews());
  }

  loadReviews(): void {
    this.errorMessage = '';

    this.reviewService.getReviews(this.ratingControl.value, this.unansweredControl.value).subscribe({
      next: (data) => {
        this.reviews = data;
        this.loaded = true;
      },
      error: () => (this.errorMessage = 'Could not load reviews.')
    });
  }

  startResponse(review: ReviewDto): void {
    this.respondingId = review.id;
    this.responseControl.reset('');
  }

  cancelResponse(): void {
    this.respondingId = null;
  }

  sendResponse(review: ReviewDto): void {
    const response = this.responseControl.value.trim();

    if (!response) {
      this.responseControl.setErrors({ required: true });
      this.responseControl.markAsTouched();
      return;
    }

    if (this.responseControl.invalid) {
      this.responseControl.markAsTouched();
      return;
    }

    this.reviewService.respond(review.id, { response, rowVersion: review.rowVersion }).subscribe({
      next: () => {
        this.toastr.success('Response sent.');
        this.respondingId = null;
        this.loadReviews();
      },
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');

        if (err.status === 409 || err.status === 404) {
          this.respondingId = null;
          this.loadReviews();
        }
      }
    });
  }

  stars(rating: number): string {
    return stars(rating);
  }
}
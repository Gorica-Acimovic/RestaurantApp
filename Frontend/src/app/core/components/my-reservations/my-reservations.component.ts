import { Component, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { ReservationService } from '../../services/reservation.service';
import { ReviewService } from '../../services/review.service';
import { ReservationDto } from '../../models/reservation.models';

@Component({
  selector: 'app-my-reservations',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule
  ],
  templateUrl: './my-reservations.component.html',
  styleUrl: './my-reservations.component.css'
})
export class MyReservationsComponent implements OnInit {
  reservations: ReservationDto[] = [];
  displayedColumns = ['time', 'guests', 'table', 'status', 'actions'];
  loaded = false;
  errorMessage = '';

  reviewing: ReservationDto | null = null;
  rating = 0;
  ratingError = '';
  commentControl = new FormControl('', { nonNullable: true, validators: [Validators.maxLength(1000)] });
  readonly starValues = [1, 2, 3, 4, 5];

  constructor(
    private reservationService: ReservationService,
    private reviewService: ReviewService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadReservations();
  }

  loadReservations(): void {
    this.errorMessage = '';

    this.reservationService.getMyReservations().subscribe({
      next: (data) => {
        this.reservations = data;
        this.loaded = true;
      },
      error: () => (this.errorMessage = 'Could not load your reservations.')
    });
  }

  cancelReservation(reservation: ReservationDto): void {
    if (!confirm('Cancel this reservation?')) {
      return;
    }

    this.reservationService
      .changeStatus(reservation.id, { status: 'Cancelled', rowVersion: reservation.rowVersion })
      .subscribe({
        next: () => {
          this.toastr.success('Reservation cancelled.');
          this.loadReservations();
        },
        error: (err) => {
          this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
          this.loadReservations();
        }
      });
  }

  startReview(reservation: ReservationDto): void {
    this.reviewing = reservation;
    this.rating = 0;
    this.ratingError = '';
    this.commentControl.reset('');
  }

  cancelReview(): void {
    this.reviewing = null;
  }

  setRating(value: number): void {
    this.rating = value;
    this.ratingError = '';
  }

  sendReview(): void {
    if (!this.reviewing) {
      return;
    }

    if (this.rating < 1) {
      this.ratingError = 'Choose a rating from 1 to 5 stars.';
      return;
    }

    if (this.commentControl.invalid) {
      this.commentControl.markAsTouched();
      return;
    }

    const comment = this.commentControl.value.trim();

    this.reviewService
      .createReview({ reservationId: this.reviewing.id, rating: this.rating, comment: comment || null })
      .subscribe({
        next: () => {
          this.toastr.success('Thank you for your review!');
          this.reviewing = null;
          this.loadReservations();
        },
        error: (err) => {
          this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');

          if (err.status === 409) {
            this.reviewing = null;
            this.loadReservations();
          }
        }
      });
  }
}
import { Component, OnInit, OnDestroy} from '@angular/core';
import { ReservationDto, ReservationStatus } from '../../models/reservation.models';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ReservationService } from '../../services/reservation.service';
import { ToastrService } from 'ngx-toastr';
import { DatePipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { merge, Subscription } from 'rxjs';
import { RealtimeService } from '../../services/realtime.service';

@Component({
  selector: 'app-reservations',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule
  ],
  templateUrl: './reservations.component.html',
  styleUrl: './reservations.component.css'
})
export class ReservationsComponent implements OnInit,OnDestroy{

 reservations: ReservationDto[] = [];
  displayedColumns = ['time', 'customer', 'guests', 'table', 'status', 'actions'];
  loaded = false;
  errorMessage = '';

  dateControl = new FormControl(this.today(), { nonNullable: true });
  private refreshSubscription: Subscription | null = null;

  constructor(private reservationService: ReservationService, private toastr: ToastrService, private realtimeService: RealtimeService) { }

  

   ngOnInit(): void {
     this.loadReservations();
    this.dateControl.valueChanges.subscribe(() => this.loadReservations());

        this.refreshSubscription = merge(
      this.realtimeService.reservationsChanged$,
      this.realtimeService.reconnected$
    ).subscribe(() => this.loadReservations());
  }

  ngOnDestroy(): void {
  this.refreshSubscription?.unsubscribe();
}

   loadReservations(): void {
    this.errorMessage = '';

    this.reservationService.getReservations(this.dateControl.value).subscribe({
      next: (data) => {
        this.reservations = data;
        this.loaded = true;
      },
      error: () => (this.errorMessage = 'Could not load reservations.')
    });
  }

   changeStatus(reservation: ReservationDto, status: ReservationStatus): void {
    if (status === 'Cancelled' && !confirm(`Cancel the reservation for ${reservation.customerName}?`)) {
      return;
    }

    this.reservationService
      .changeStatus(reservation.id, { status, rowVersion: reservation.rowVersion })
      .subscribe({
        next: () => {
          this.toastr.success('Reservation updated.');
          this.loadReservations();
        },
        error: (err) => {
          this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
          this.loadReservations();
        }
      });
  }

  showToday():void{
    this.dateControl.setValue(this.today());
  }

  showAll():void {
    this.dateControl.setValue('');
  }

  private today(): string {
    const now = new Date();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    return `${now.getFullYear()}-${month}-${day}`;
  }

}

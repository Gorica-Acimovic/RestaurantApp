import { Component, OnInit, OnDestroy} from '@angular/core';
import { AvailableTableDto, ReservationDto, ReservationStatus } from '../../models/reservation.models';
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


  confirming: ReservationDto | null = null; 
  availableTables : AvailableTableDto[] = []; 
  chosenTableId: number | null = null;  
  confirmError =''; 

  readonly mapWidth = 160; 
  readonly mapHeight = 100;
  readonly xScale = 1.6;

  dateControl = new FormControl(this.today(), { nonNullable: true }); 
  private refreshSubscription: Subscription | null = null;  

  constructor(private reservationService: ReservationService, private toastr: ToastrService, private realtimeService: RealtimeService) { } 

  

   ngOnInit(): void {               
      this.loadReservations();          
      this.dateControl.valueChanges.subscribe(() => this.loadReservations());  
      this.refreshSubscription = merge(
      this.realtimeService.reservationsChanged$,
      this.realtimeService.reconnected$
    ).subscribe(() => {
      this.loadReservations();
      if (this.confirming) {
        this.loadAvailability(this.confirming);
      }
});
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

  startConfirm(reservation: ReservationDto): void {
  this.confirming = reservation; 
  this.chosenTableId = reservation.tableId; 
  this.confirmError = '';
  this.availableTables = [];

  this.loadAvailability(reservation);

  if (reservation.tableId === null) {  
    this.reservationService.getSuggestedTable(reservation.id).subscribe({
      next: (table) => (this.chosenTableId = table.id), 
      error: () => (this.chosenTableId = null)      
    });
  }
}

private loadAvailability(reservation: ReservationDto): void {
  this.reservationService.getAvailableTables(reservation.startsAt, reservation.numberOfGuests).subscribe({
    next: (tables) => (this.availableTables = tables),
    error: (err) => (this.confirmError = err.error?.message ?? 'Could not load tables.')
  }); 
}

isSelectable(table: AvailableTableDto): boolean {
  return table.isAvailable || table.id === this.confirming?.tableId;
}


chooseTable(table: AvailableTableDto): void {
  if (this.isSelectable(table)) {
    this.chosenTableId = table.id;
  }
}

closeConfirm(): void {
  this.confirming = null; 
}

confirmReservation(): void {
  if (!this.confirming || this.chosenTableId === null) {
    this.confirmError = 'Choose a table first.';
    return;
  }

  const reservation = this.confirming;

  this.reservationService
    .changeStatus(reservation.id, { status: 'Confirmed', rowVersion: reservation.rowVersion, tableId: this.chosenTableId })
    .subscribe({
      next: () => {
        this.toastr.success('Reservation confirmed.');
        this.confirming = null;
        this.loadReservations();
      },
      error: (err) => {
        this.confirmError = err.error?.message ?? 'Something went wrong. Please try again.';
        this.loadAvailability(reservation);
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


  canComplete(reservation: ReservationDto): boolean {
  return new Date(reservation.startsAt).getTime() - Date.now() <= 30 * 60 * 1000;
}

canMarkNoShow(reservation: ReservationDto): boolean {
  return new Date(reservation.startsAt).getTime() <= Date.now();
}

}

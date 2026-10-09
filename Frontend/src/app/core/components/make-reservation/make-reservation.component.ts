import { Component } from '@angular/core';
import { ReservationService } from '../../services/reservation.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { RouterLink } from '@angular/router';
import { ReservationDto, ReservationRequest } from '../../models/reservation.models';
import {  MatToolbarModule } from '@angular/material/toolbar';
import { DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-make-reservation',
  standalone: true,
  imports: [
     ReactiveFormsModule,
    RouterLink,
    DatePipe,
    MatToolbarModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './make-reservation.component.html',
  styleUrl: './make-reservation.component.css'
})
export class MakeReservationComponent {

  form:FormGroup;
  result:ReservationDto | null = null;
  errorMessage= '';
  sending = false;


public constructor(private service:ReservationService, private formBuilder:FormBuilder){
  this.form = this.formBuilder.group({
    customerName: ['', [Validators.required, Validators.maxLength(100)]],
    customerEmail:['',[Validators.email,Validators.maxLength(200)]],
    customerPhone:['',[Validators.maxLength(20)]],
    date:['', [Validators.required]],
    time:['',[Validators.required]],
    numberOfGuests: [2,[Validators.required,Validators.min(1),Validators.max(50)]]
  });
}

submit():void{
  if(this.form.invalid){
    this.form.markAllAsTouched();
    return;
  }

  this.errorMessage ='';
  this.sending = true;

  const value = this.form.getRawValue();
  const request : ReservationRequest = {
    customerName: value.customerName.trim(),
    customerEmail:value.customerEmail.trim() || null,
    customerPhone:value.customerPhone.trim() || null,
    startsAt: `${value.date}T${value.time}:00`,
    numberOfGuests: value.numberOfGuests
  };

  this.service.createReservation(request).subscribe({
    next:(data) => {
      this.result = data;
      this.sending = false;
    },
    error:(err) => {
      this.errorMessage = err.error?.message?? 'Something went wrong.Please try again.';
      this.sending = false;
      
    }

  });
}

newReservation() :void {
  this.result = null;
  this.errorMessage='';
  this.form.reset({
    customerName: '',
    customerEmail: '',
    customerPhone:'',
    date:'',
    time:'',
    numberOfGuests:2

    });
  }
}

import { Component, OnInit } from '@angular/core';
import { ReservationService } from '../../services/reservation.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { ReservationDto, ReservationRequest } from '../../models/reservation.models';
import { DatePipe } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';

@Component({
  selector: 'app-make-reservation',
  standalone: true,
  imports: [
     ReactiveFormsModule,
    DatePipe,
   MatFormFieldModule,
    MatInputModule,
   
  ],
  templateUrl: './make-reservation.component.html',
  styleUrl: './make-reservation.component.css'
})
export class MakeReservationComponent implements OnInit {

  form:FormGroup;
  result:ReservationDto | null = null;
  errorMessage= '';
  sending = false;


public constructor(private service:ReservationService, private formBuilder:FormBuilder, private authService:AuthService, private profileService:ProfileService){
  this.form = this.formBuilder.group({
    customerName: ['', [Validators.required, Validators.maxLength(100)]],
    customerEmail:['',[Validators.email,Validators.maxLength(200)]],
    customerPhone:['',[Validators.maxLength(20)]],
    date:['', [Validators.required]],
    time:['',[Validators.required]],
    numberOfGuests: [2,[Validators.required,Validators.min(1),Validators.max(50)]]
  });
}
  ngOnInit(): void {
    if(this.authService.getRole() !== 'Guest') {
      return;
    }
    this.fillFromProfile();
  }

private fillFromProfile() : void {
  this.profileService.getProfile().subscribe ({
    next: (user) => {
      this.form.patchValue({
        customerName: `${user.firstName} ${user.lastName}`,
        customerEmail: user.email
      });
    },
    error: () => {}
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
    
    if(this.authService.getRole() === 'Guest'){
      this.fillFromProfile();
    }
  }
}

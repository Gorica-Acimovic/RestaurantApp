import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ToastrService } from 'ngx-toastr';
import { WaiterService } from '../../services/waiter.service';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { UserDto } from '../../models/user.models';

@Component({
  selector: 'app-waiters',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule
  ],
  templateUrl: './waiters.component.html',
  styleUrl: './waiters.component.css'
})
export class WaitersComponent implements OnInit {
   @ViewChild(FormGroupDirective) formDirective!: FormGroupDirective;

  waiters: UserDto[] = [];
  displayedColumns = ['name', 'email', 'status', 'actions'];
  errorMessage = '';
  registerError = '';
  form: FormGroup;

  constructor(private waiterService: WaiterService, private formBuilder: FormBuilder, private toastr: ToastrService) { 
    this.form = this.formBuilder.group({
       firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[A-Za-z0-9_.-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$/)
        ]
      ],
      pin: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]]
    });
  }



   ngOnInit(): void {
    this.loadWaiters();
  }

  loadWaiters(): void {
    this.waiterService.getAllWaiters().subscribe({
      next: (data) => (this.waiters = data),
      error: () => (this.errorMessage = 'Could not load waiters.')
    });
  }

  register(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.registerError = '';

    this.waiterService.registerWaiter(this.form.getRawValue()).subscribe({
      next: () => {
        this.toastr.success('Waiter registered.');
        this.formDirective.resetForm();
        this.loadWaiters();
      },
      error: (err) => {
        this.registerError = err.error?.message ?? 'Something went wrong. Please try again.';
      }
    });
  }

  toggleActive(waiter: UserDto): void {
    this.waiterService.setActive(waiter.id, !waiter.isActive).subscribe({
      next: () => this.loadWaiters(),
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }





}

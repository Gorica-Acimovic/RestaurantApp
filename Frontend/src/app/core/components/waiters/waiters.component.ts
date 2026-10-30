import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ToastrService } from 'ngx-toastr';
import { WaiterService } from '../../services/waiter.service';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { UserDto } from '../../models/user.models';

@Component({
  selector: 'app-waiters',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
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
  editingWaiter: UserDto | null = null;

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
startEdit(waiter: UserDto): void {
  this.editingWaiter = waiter;
  this.registerError = '';

  this.form.setValue({
    firstName: waiter.firstName,
    lastName: waiter.lastName,
    email: waiter.email,
    pin: ''
  });
  this.form.markAsUntouched();
  this.setPinRequired(false);

  window.scrollTo({ top: 0, behavior: 'smooth' });
}

cancelEdit(): void {
  this.editingWaiter = null;
  this.registerError = '';
  this.formDirective.resetForm();
  this.setPinRequired(true);
}

submit(): void {
  if (this.editingWaiter) {
    this.saveChanges(this.editingWaiter);
  } else {
    this.register();
  }
}

private saveChanges(waiter: UserDto): void {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }

  this.registerError = '';
  const { firstName, lastName, email, pin } = this.form.getRawValue();

  this.waiterService.updateWaiter(waiter.id, { firstName, lastName, email, pin: pin || null }).subscribe({
    next: () => {
      this.toastr.success('Waiter updated.');
      this.cancelEdit();
      this.loadWaiters();
    },
    error: (err) => {
      this.registerError = err.error?.message ?? 'Something went wrong. Please try again.';
    }
  });
}
  private setPinRequired(required: boolean): void {
  const control = this.form.controls['pin'];
  const pattern = Validators.pattern(/^\d{4}$/);

  control.setValidators(required ? [Validators.required, pattern] : [pattern]);
  control.updateValueAndValidity();
}




}

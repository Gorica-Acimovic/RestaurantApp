import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { Role } from '../../models/auth.models';

@Component({
  selector: 'app-pin-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './pin-login.component.html',
  styleUrl: './pin-login.component.css'
})
export class PinLoginComponent {
form: FormGroup;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private formBuilder: FormBuilder
  ) {
    this.form = this.formBuilder.group({
      pin: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage = '';

    this.authService.loginWithPin(this.form.getRawValue()).subscribe({
      next: (response) => this.router.navigate([this.homeFor(response.role)]),      error: (err) => {
      this.errorMessage = err.error?.message ??
       (err.status === 429 ? 'Too many requests. Please wait a minute and try again.' : 'Login failed. Please try again.');
      }
    });

  }

    private homeFor(role: Role): string {
    switch (role) {
      case 'Kitchen':
        return '/dashboard/kitchen';
      case 'Courier':
        return '/dashboard/courier';
      default:
        return '/dashboard/tables';
    }
  }
}

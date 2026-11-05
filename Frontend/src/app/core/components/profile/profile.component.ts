import { Component, OnInit } from '@angular/core';
import { ProfileService } from '../../services/profile.service';
import { UserDto } from '../../models/user.models';
import { AbstractControl, FormBuilder, FormControl, FormGroup, FormGroupDirective, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Role } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

const PASSWORD_PATTERN = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$/;

function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const newPassword = group.get('newPassword')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return newPassword === confirmPassword ? null : { mismatch: true };
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  profile : UserDto | null = null;
  role: Role | null;
  errorMessage = '';
  passwordError ='';
  deactivateError ='';

  passwordForm: FormGroup;
  credentialControl = new FormControl('',{nonNullable:true,validators:[Validators.required]})

 constructor(
    private profileService: ProfileService,
    private authService: AuthService,
    private router: Router,
    private formBuilder: FormBuilder,
    private toastr: ToastrService
  ) {
    this.role = this.authService.getRole();

    this.passwordForm = this.formBuilder.group(
      {
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
        confirmPassword: ['', [Validators.required]]
      },
      { validators: passwordsMatch }
    );
  }

  get usesPin():boolean {
    return this.role === 'Waiter' || this.role === 'Kitchen' || this.role === 'Courier';
  }

 get canChangePassword():boolean{
    return !this.usesPin;
  }
  get canDeactivate():boolean {
    return this.role !== 'Admin';
  }

  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: (data) => (this.profile = data),
      error:() => (this.errorMessage = 'Could not load your profile.')
    });
  }

  changePassword(directive: FormGroupDirective) : void {
    if(this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.passwordError ='';

    const {currentPassword, newPassword} = this.passwordForm.getRawValue();

    this.profileService.changePassword({currentPassword, newPassword}).subscribe({
    next: () => {
      this.toastr.success('Password changed.');
      directive.resetForm();
    },
    error: (err) => {
      this.passwordError = err.error?.message ?? 'Something went wrong, please try again.';
      
    }
 });
}

  deactivate(): void {
    if(this.credentialControl.invalid) {
      this.credentialControl.markAsTouched();
      return;
    }
    
    if (!confirm('Deactivate your account? You will be signed out and will not be able to sign in again.')) {
      return;
    }
    this.deactivateError = '';
    const value = this.credentialControl.value;
this.profileService
      .deactivate({ password: this.usesPin ? null : value, pin: this.usesPin ? value : null })
      .subscribe({
        next: () => {
          this.toastr.success('Your account has been deactivated.');
          this.authService.logout();
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.deactivateError = err.error?.message ?? 'Something went wrong. Please try again.';
        }
      });
  }

  }


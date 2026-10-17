import { Component, OnInit, ViewChild } from '@angular/core';
import { ManagerService } from '../../services/manager.service';
import { UserDto } from '../../models/user.models';
import { MatTableModule } from '@angular/material/table';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-managers',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule],
  templateUrl: './managers.component.html',
  styleUrl: './managers.component.css'
})
export class ManagersComponent implements OnInit {
   @ViewChild(FormGroupDirective) formDirective!: FormGroupDirective;

  managers: UserDto[] = [];
  displayedColumns: string[] = ['name', 'email', 'status', 'actions'];
  errorMessage: string = '';
  registerErrorMessage: string = '';
  form:FormGroup;

  constructor(private managerService: ManagerService, private formBuilder:FormBuilder, private toastr:ToastrService) {
    this.form = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9_.-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$/)]],
      password: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$/)]]
    });
  }

    ngOnInit(): void {
    this.loadManagers();
  }

  loadManagers(): void {
    this.managerService.getAllManagers().subscribe({
      next: (data) => (this.managers = data),
      error: () => (this.errorMessage = 'Could not load managers.')
    });
  }

  register(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.registerErrorMessage = '';

    this.managerService.registerManager(this.form.getRawValue()).subscribe({
      next: () => {
        this.toastr.success('Manager registered successfully.');
        this.formDirective.resetForm();
        this.loadManagers();
      },
      error: (err) => {
        this.registerErrorMessage = err.error?.message ?? 'Something went wrong. Please try again.';
      }
    });
  }

  toggleActive(manager: UserDto): void {
    this.managerService.setActive(manager.id, !manager.isActive).subscribe({
      next: () => this.loadManagers(),
     error: (err) => {
      const message = err.error?.message ?? 'Something went wrong. Please try again.';
      this.toastr.error(message);
}
    });
  }
}

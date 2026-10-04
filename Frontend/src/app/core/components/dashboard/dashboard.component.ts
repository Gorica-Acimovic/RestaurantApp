import { Component } from '@angular/core';
import { LoginResponse } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatToolbarModule, MatButtonModule, RouterOutlet, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {

  session: LoginResponse | null;

  constructor(private authService: AuthService, private router: Router) {
    this.session = this.authService.getSession();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

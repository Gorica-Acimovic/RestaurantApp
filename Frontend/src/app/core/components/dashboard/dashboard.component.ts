import { LoginResponse } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RealtimeService } from '../../services/realtime.service';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatToolbarModule, MatButtonModule, RouterOutlet, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy {

  session: LoginResponse | null;

  constructor(private authService: AuthService, private router: Router, private realtimeService: RealtimeService) {
    this.session = this.authService.getSession();
  }

   ngOnInit(): void {
   if (this.session?.role !== 'Guest') {
      this.realtimeService.connect();
    }
  }
  ngOnDestroy(): void {
    this.realtimeService.disconnect();
  }
 

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

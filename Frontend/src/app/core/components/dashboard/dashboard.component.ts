import { LoginResponse } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RealtimeService } from '../../services/realtime.service';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [ RouterOutlet, RouterLink, RouterLinkActive],
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

import { LoginResponse } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RealtimeService } from '../../services/realtime.service';
import { CartService } from '../../services/cart.service';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [ RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy {

  session: LoginResponse | null;
  private readySubscription?: Subscription;

  constructor(
    private authService: AuthService, 
    private router: Router, 
    private realtimeService: RealtimeService, 
    public cartService: CartService, 
    private toastr:ToastrService ) {

    this.session = this.authService.getSession();
  }

   ngOnInit(): void {
      this.realtimeService.connect();
      this.readySubscription = this.realtimeService.orderReady$.subscribe(tableNumber =>
      this.toastr.success(`Table ${tableNumber}: order is ready.`, 'Kitchen', { timeOut: 10000 }));
    
  }
  ngOnDestroy(): void {
    this.readySubscription?.unsubscribe();
    this.realtimeService.disconnect();
  }
 

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

import { Routes } from '@angular/router';
import { LoginComponent } from './core/components/login/login.component';
import { DashboardComponent } from './core/components/dashboard/dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { ManagersComponent } from './core/components/managers/managers.component';
import { TablesComponent } from './core/components/tables/tables.component';
import { WaitersComponent } from './core/components/waiters/waiters.component';
import { PinLoginComponent } from './core/components/pin-login/pin-login.component';
import { roleGuard } from './core/guards/role.guard';
import { MenuComponent } from './core/components/menu/menu.component';
import { MenuManagementComponent } from './core/components/menu-management/menu-management.component';
import { ReservationsComponent } from './core/components/reservations/reservations.component';
import { MakeReservationComponent } from './core/components/make-reservation/make-reservation.component';
import { RegisterComponent } from './core/components/register/register.component';
import { OrderComponent } from './core/components/order/order.component';
import { HomeComponent } from './core/components/home/home.component';
import { MyReservationsComponent } from './core/components/my-reservations/my-reservations.component';
import { ReviewsComponent } from './core/components/reviews/reviews.component';
import { StatisticsComponent } from './core/components/statistics/statistics.component';
import { ProfileComponent } from './core/components/profile/profile.component';
import { CheckoutComponent } from './core/components/checkout/checkout.component';
import { MyOrdersComponent } from './core/components/my-orders/my-orders.component';
import { OnlineOrdersComponent } from './core/components/online-orders/online-orders.component';
import { KitchenComponent } from './core/components/kitchen/kitchen.component';
import { CourierComponent } from './core/components/courier/courier.component';

export const routes: Routes = [

   { path: '', component: HomeComponent, pathMatch: 'full' },
    {path: 'login', component: LoginComponent},
    { path: 'pin-login', component: PinLoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'menu', component: MenuComponent },
    {path:'make-reservation', component:MakeReservationComponent},
    {
     path: 'dashboard', 
     component: DashboardComponent,
     canActivate: [authGuard],
     children: [
        {path: 'managers', component:ManagersComponent, canActivate: [roleGuard('Admin')] },
        {path: 'waiters', component:WaitersComponent, canActivate: [roleGuard('Manager')] },
        { path: 'tables', component: TablesComponent, canActivate: [roleGuard('Admin', 'Manager', 'Waiter')] },
        { path: 'menu', component: MenuManagementComponent, canActivate: [roleGuard('Manager')] },
        { path:'reservations', component: ReservationsComponent, canActivate: [roleGuard('Admin', 'Manager', 'Waiter')]},
        { path: 'orders/:tableId', component: OrderComponent, canActivate: [roleGuard('Admin', 'Manager', 'Waiter')] },
        { path: 'my-reservations', component: MyReservationsComponent, canActivate: [roleGuard('Guest')] },
        { path: 'checkout', component: CheckoutComponent, canActivate: [roleGuard('Guest')] },
        { path: 'my-orders', component: MyOrdersComponent, canActivate: [roleGuard('Guest')] },
        { path: 'online-orders', component: OnlineOrdersComponent, canActivate: [roleGuard('Manager')] },
        { path: 'kitchen', component: KitchenComponent, canActivate: [roleGuard('Kitchen', 'Manager')] },
        { path: 'reviews', component: ReviewsComponent, canActivate: [roleGuard('Manager')] },
        { path: 'statistics', component: StatisticsComponent, canActivate: [roleGuard('Admin', 'Manager')] },
        { path: 'courier', component: CourierComponent, canActivate: [roleGuard('Courier')] },
        { path: 'profile', component: ProfileComponent },
     ]
    }
];
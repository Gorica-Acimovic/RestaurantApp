import { Routes } from '@angular/router';
import { LoginComponent } from './core/components/login/login.component';
import { DashboardComponent } from './core/components/dashboard/dashboard.component';
import { authGuard } from './core/guards/auth.guard';
import { ManagersComponent } from './core/components/managers/managers.component';
import { roleGuard } from './core/guards/role.guard';
import { WaitersComponent } from './core/components/waiters/waiters.component';
import { PinLoginComponent } from './core/components/pin-login/pin-login.component';

export const routes: Routes = [

    {path: '', redirectTo: 'login', pathMatch: 'full'}, 
    {path: 'login', component: LoginComponent},
    { path: 'pin-login', component: PinLoginComponent },
    {
     path: 'dashboard', 
     component: DashboardComponent,
     canActivate: [authGuard],
     children: [
        {path: 'managers', component:ManagersComponent, canActivate: [roleGuard('Admin')] },
        {path: 'waiters', component:WaitersComponent, canActivate: [roleGuard('Manager')] }
     ]
    }
];
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

export const routes: Routes = [

    {path: '', redirectTo: 'login', pathMatch: 'full'}, 
    {path: 'login', component: LoginComponent},
    { path: 'pin-login', component: PinLoginComponent },
    { path: 'menu', component: MenuComponent },
    {
     path: 'dashboard', 
     component: DashboardComponent,
     canActivate: [authGuard],
     children: [
        {path: 'managers', component:ManagersComponent, canActivate: [roleGuard('Admin')] },
        {path: 'waiters', component:WaitersComponent, canActivate: [roleGuard('Manager')] },
        { path: 'tables', component: TablesComponent, canActivate: [roleGuard('Admin', 'Manager', 'Waiter')] },
        { path: 'menu', component: MenuManagementComponent, canActivate: [roleGuard('Manager')] }
     ]
    }
];
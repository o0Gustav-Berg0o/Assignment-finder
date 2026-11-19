import { Routes } from '@angular/router';
import { adminGuard } from './core/admin.guard';
import { authGuard } from './core/auth.guard';
import { AdminComponent } from './pages/admin/admin.component';
import { AssignmentsComponent } from './pages/assignments/assignments.component';
import { BrokersComponent } from './pages/brokers/brokers.component';
import { LoginComponent } from './pages/login/login.component';
import { MatchesComponent } from './pages/matches/matches.component';
import { RegisterComponent } from './pages/register/register.component';
import { SavedSearchesComponent } from './pages/saved-searches/saved-searches.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'matches' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'matches', component: MatchesComponent, canActivate: [authGuard] },
  { path: 'assignments', component: AssignmentsComponent, canActivate: [authGuard] },
  { path: 'saved-searches', component: SavedSearchesComponent, canActivate: [authGuard] },
  { path: 'brokers', component: BrokersComponent, canActivate: [authGuard, adminGuard] },
  { path: 'admin', component: AdminComponent, canActivate: [authGuard, adminGuard] },
];

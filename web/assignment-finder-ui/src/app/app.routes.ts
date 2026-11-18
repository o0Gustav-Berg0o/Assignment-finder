import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { BrokersComponent } from './pages/brokers/brokers.component';
import { CriteriaComponent } from './pages/criteria/criteria.component';
import { LoginComponent } from './pages/login/login.component';
import { MatchesComponent } from './pages/matches/matches.component';
import { RegisterComponent } from './pages/register/register.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'matches' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'matches', component: MatchesComponent, canActivate: [authGuard] },
  { path: 'criteria', component: CriteriaComponent, canActivate: [authGuard] },
  { path: 'brokers', component: BrokersComponent, canActivate: [authGuard] },
];

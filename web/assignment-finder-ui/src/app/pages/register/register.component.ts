import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  email = '';
  password = '';
  readonly error = signal<string | null>(null);

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  submit(): void {
    this.error.set(null);
    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        this.auth.login(this.email, this.password).subscribe({
          next: () => this.router.navigateByUrl('/matches'),
          error: () => this.router.navigateByUrl('/login'),
        });
      },
      error: () => this.error.set('Could not register. Check your email/password.'),
    });
  }
}

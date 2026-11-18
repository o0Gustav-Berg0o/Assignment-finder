import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
  readonly submitted = signal(false);

  constructor(private readonly auth: AuthService) {}

  submit(): void {
    this.error.set(null);
    this.auth.register(this.email, this.password).subscribe({
      // New accounts need an admin to approve them before they can log in, so there's no
      // token to use yet - just confirm the request went through.
      next: () => this.submitted.set(true),
      error: () => this.error.set('Could not register. Check your email/password.'),
    });
  }
}

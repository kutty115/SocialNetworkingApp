import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class LoginComponent {
  model = { identifier: '', password: '' };
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    this.loading = true;

    this.auth.login(this.model).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/feed']);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || 'Login failed. Check credentials and backend auth response.';
      }
    });
  }
}

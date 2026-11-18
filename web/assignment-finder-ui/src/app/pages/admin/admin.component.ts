import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { AdminService } from '../../core/admin.service';
import { AdminUser } from '../../core/models';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
})
export class AdminComponent implements OnInit {
  readonly users = signal<AdminUser[]>([]);

  constructor(private readonly adminService: AdminService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.adminService.listUsers().subscribe((users) => this.users.set(users));
  }

  approve(user: AdminUser): void {
    this.adminService.approve(user.id).subscribe(() => this.refresh());
  }

  reject(user: AdminUser): void {
    this.adminService.reject(user.id).subscribe(() => this.refresh());
  }
}

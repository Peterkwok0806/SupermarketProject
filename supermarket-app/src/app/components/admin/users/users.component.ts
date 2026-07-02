import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { AdminApiService } from '../../../services/admin-api.service';
import { NotificationService } from '../../../services/notification.service';
import { LoggerService } from '../../../services/logger.service';
import { AdminUser } from '../../../models/admin-user';

@Component({
  selector: 'app-users',
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class AdminUsersComponent implements OnInit {
  private adminApi = inject(AdminApiService);
  private notificationService = inject(NotificationService);
  private logger = inject(LoggerService);

  users = signal<AdminUser[]>([]);
  isLoading = signal(true);
  totalCount = signal(0);
  pageNumber = signal(1);
  pageSize = signal(10);
  totalPages = signal(0);
  searchQuery = signal('');
  error = signal<string | null>(null);

  // Edit modal
  showEditModal = signal(false);
  editingUser = signal<AdminUser | null>(null);
  editForm = signal<{ isActive: boolean; role: string }>({ isActive: true, role: 'Customer' });

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.adminApi.getUsers(this.pageNumber(), this.pageSize(), this.searchQuery() || undefined).subscribe({
      next: (res) => {
        if (res.success) {
          this.users.set(res.items);
          this.totalCount.set(res.totalCount);
          this.totalPages.set(res.totalPages);
        } else {
          this.error.set(res.message);
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.logger.error('Failed to load users:', err);
        this.error.set('Failed to load users');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
    this.pageNumber.set(1);
    this.loadUsers();
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.pageNumber.set(page);
    this.loadUsers();
  }

  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.pageNumber();
    const pages: number[] = [];
    const maxVisible = 5;

    let start = Math.max(1, current - Math.floor(maxVisible / 2));
    let end = Math.min(total, start + maxVisible - 1);

    if (end - start + 1 < maxVisible) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  openEditModal(user: AdminUser) {
    this.editingUser.set(user);
    this.editForm.set({ isActive: user.isActive, role: user.role });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingUser.set(null);
  }

  toggleEditStatus() {
    const form = this.editForm();
    this.editForm.set({ ...form, isActive: !form.isActive });
  }

  setEditRole(role: string) {
    const form = this.editForm();
    this.editForm.set({ ...form, role });
  }

  saveUser() {
    const user = this.editingUser();
    const form = this.editForm();
    if (!user) return;

    // Update status if changed
    if (user.isActive !== form.isActive) {
      this.adminApi.updateUserStatus(user.id, form.isActive).subscribe({
        next: (res) => {
          if (!res.success) {
            this.notificationService.error(res.message);
          }
        },
        error: (err) => {
          this.logger.error('Failed to update status:', err);
          this.notificationService.error('更新狀態失敗');
        }
      });
    }

    // Update role if changed
    if (user.role !== form.role) {
      this.adminApi.updateUserRole(user.id, form.role).subscribe({
        next: (res) => {
          if (!res.success) {
            this.notificationService.error(res.message);
          }
        },
        error: (err) => {
          this.logger.error('Failed to update role:', err);
          this.notificationService.error('更新角色失敗');
        }
      });
    }

    this.closeEditModal();
    this.loadUsers();
  }

  getRoleBadgeClass(role: string): string {
    return role === 'Admin' ? 'bg-red-100 text-red-700' : 'bg-blue-100 text-blue-700';
  }

  getStatusClass(isActive: boolean): string {
    return isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600';
  }
}
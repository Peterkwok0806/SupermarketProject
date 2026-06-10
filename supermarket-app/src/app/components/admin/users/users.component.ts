import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-users',
  imports: [CommonModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class AdminUsersComponent implements OnInit{
  users: any[] = [
    { id: 1, username: "admin", email: "admin@supermart.com", role: "Admin", createdAt: new Date() },
    { id: 2, username: "user1", email: "user1@example.com", role: "Customer", createdAt: new Date(Date.now() - 86400000) },
    { id: 3, username: "user2", email: "user2@example.com", role: "Customer", createdAt: new Date(Date.now() - 172800000) },
  ];

  ngOnInit() {
    // TODO: 之後從後端 API 取得真實用戶列表
  }

  editUser(user: any) {
    alert(`編輯用戶: ${user.username} (功能開發中)`);
  }

  deleteUser(user: any) {
    if (confirm(`確定要刪除用戶 ${user.username} 嗎？`)) {
      alert(`已刪除用戶 ${user.username} (功能開發中)`);
    }
  }
}

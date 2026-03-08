import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';

import { ChatService, ChatMessage } from '../../services/chat.service';
import { UsersService, AppUser } from '../../services/users.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class ChatComponent implements OnInit, OnDestroy {
  users: AppUser[] = [];
  selectedUser?: AppUser;

  text = '';
  messages: ChatMessage[] = [];

  private sub?: Subscription;

  constructor(private chat: ChatService, private usersApi: UsersService) {}

  async ngOnInit() {
    // 1) Load users list (fixes your 404 screen)
    this.usersApi.getAll().subscribe({
      next: (list) => (this.users = list),
      error: (err) => {
        console.error(err);
        alert('Failed to load users list');
      }
    });

    // 2) Start SignalR + subscribe messages
    await this.chat.start();
    this.sub = this.chat.messages$.subscribe(m => (this.messages = m));
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
    this.chat.stop();
  }

  select(u: AppUser) {
    this.selectedUser = u;
    this.chat.clearMessages(); // optional: clear when switching user
  }

  async send() {
    if (!this.selectedUser) {
      alert('Select a user first');
      return;
    }
    const msg = this.text.trim();
    if (!msg) return;

    await this.chat.send(this.selectedUser.id, msg);
    this.text = '';
  }
}
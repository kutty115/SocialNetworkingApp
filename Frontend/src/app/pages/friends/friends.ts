import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FriendsService } from '../../services/friends.service';

@Component({
  selector: 'app-friends',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './friends.html',
  styleUrl: './friends.css',
})
export class FriendsComponent {
  email = '';

  incomingReqs: any[] = [];
  outgoingReqs: any[] = [];
  friends: any[] = [];
  blocked: any[] = [];

  constructor(private friendsApi: FriendsService) {}

  ngOnInit() {
    this.refreshAll();
  }

  refreshAll() {
    this.friendsApi.incoming().subscribe(x => (this.incomingReqs = x));
    this.friendsApi.outgoing().subscribe(x => (this.outgoingReqs = x));
    this.friendsApi.list().subscribe(x => (this.friends = x));
    this.friendsApi.blocked().subscribe(x => (this.blocked = x));
  }

  send() {
    if (!this.email.trim()) return;
    this.friendsApi.sendRequest(this.email.trim()).subscribe({
      next: () => {
        alert('Request sent!');
        this.email = '';
        this.refreshAll();
      },
      error: (e) => alert(e?.error ?? 'Failed to send request')
    });
  }

  accept(id: number) {
    this.friendsApi.accept(id).subscribe({
      next: () => this.refreshAll(),
      error: (e) => alert(e?.error ?? 'Accept failed')
    });
  }

  reject(id: number) {
    this.friendsApi.reject(id).subscribe({
      next: () => this.refreshAll(),
      error: (e) => alert(e?.error ?? 'Reject failed')
    });
  }

  unfriend(userId: string) {
    this.friendsApi.unfriend(userId).subscribe({
      next: () => this.refreshAll(),
      error: (e) => alert(e?.error ?? 'Unfriend failed')
    });
  }

  block(userId: string) {
    this.friendsApi.block(userId).subscribe({
      next: () => this.refreshAll(),
      error: (e) => alert(e?.error ?? 'Block failed')
    });
  }

  unblock(userId: string) {
    this.friendsApi.unblock(userId).subscribe({
      next: () => this.refreshAll(),
      error: (e) => alert(e?.error ?? 'Unblock failed')
    });
  }
}

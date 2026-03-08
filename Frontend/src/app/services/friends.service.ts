import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class FriendsService {
  private api = 'https://localhost:7061/api/friends';

  constructor(private http: HttpClient) {}

  sendRequest(email: string) {
    return this.http.post(`${this.api}/request`, { email });
  }

  incoming() {
    return this.http.get<any[]>(`${this.api}/requests/incoming`);
  }

  outgoing() {
    return this.http.get<any[]>(`${this.api}/requests/outgoing`);
  }

  accept(requestId: number) {
    return this.http.post(`${this.api}/accept`, { requestId });
  }

  reject(requestId: number) {
    return this.http.post(`${this.api}/reject`, { requestId });
  }

  list() {
    return this.http.get<any[]>(`${this.api}/list`);
  }

  unfriend(friendId: string) {
    return this.http.delete(`${this.api}/unfriend/${friendId}`);
  }

  block(userId: string) {
    return this.http.post(`${this.api}/block/${userId}`, {});
  }

  unblock(userId: string) {
    return this.http.post(`${this.api}/unblock/${userId}`, {});
  }

  blocked() {
    return this.http.get<any[]>(`${this.api}/blocked`);
  }
}
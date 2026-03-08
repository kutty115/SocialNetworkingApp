import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private api = '/api/users';

  constructor(private http: HttpClient) {}

  getMe() {
    return this.http.get(`${this.api}/me`);
  }

  updateMe(data: any) {
    return this.http.put(`${this.api}/me`, data);
  }
}
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

export interface AppUser {
  id: string;
  fullName: string;
  email: string;
  profileImageUrl?: string;
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = '/api/users';

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<AppUser[]>(this.api); // GET /api/users
  }
}
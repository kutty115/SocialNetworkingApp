import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class PostsService {
  private api = 'https://localhost:7061/api/posts';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private authOptions() {
    const token = this.auth.token();
    return token
      ? { headers: new HttpHeaders({ Authorization: `Bearer ${token}` }) }
      : {};
  }

  feed() {
    return this.http.get<any[]>(`${this.api}/feed`, this.authOptions());
  }

  create(body: { content: string; imageUrl?: string | null }) {
    return this.http.post(`${this.api}`, body, this.authOptions());
  }

  like(postId: number) {
    return this.http.post(`${this.api}/${postId}/like`, {}, this.authOptions());
  }

  comment(postId: number, text: string) {
    return this.http.post(`${this.api}/comment`, { postId, text }, this.authOptions());
  }
}

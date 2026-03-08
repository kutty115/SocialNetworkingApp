import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PostsService } from '../../services/posts.service';

@Component({
  selector: 'app-feed',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './feed.html',
  styleUrls: ['./feed.css']
})
export class FeedComponent implements OnInit {
  posts: any[] = [];
  loading = false;

  newPost = { content: '', imageUrl: '' };
  commentText: Record<number, string> = {};

  constructor(private postsService: PostsService, private router: Router) {}

  ngOnInit(): void {
    this.loadFeed();
  }

  loadFeed() {
    this.loading = true;
    this.postsService.feed().subscribe({
      next: (res) => {
        this.posts = res;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        if (err?.status === 401) {
          alert('Session expired or invalid token. Please login again.');
          this.router.navigate(['/login']);
          return;
        }
        alert(`Feed API failed (${err?.status ?? 'no-status'}). ${err?.error?.message ?? ''}`.trim());
        this.loading = false;
      }
    });
  }

  createPost() {
    if (!this.newPost.content.trim()) return;

    this.postsService.create({
      content: this.newPost.content,
      imageUrl: this.newPost.imageUrl || null
    }).subscribe({
      next: () => {
        this.newPost = { content: '', imageUrl: '' };
        this.loadFeed();
      },
      error: (err) => {
        console.error(err);
        if (err?.status === 401) {
          alert('Session expired or invalid token. Please login again.');
          this.router.navigate(['/login']);
          return;
        }
        alert(`Create post failed (${err?.status ?? 'no-status'}). ${err?.error?.message ?? ''}`.trim());
      }
    });
  }

  like(postId: number) {
    this.postsService.like(postId).subscribe({
      next: () => this.loadFeed(),
      error: (err) => {
        console.error(err);
        if (err?.status === 401) {
          alert('Session expired or invalid token. Please login again.');
          this.router.navigate(['/login']);
          return;
        }
        alert(`Like failed (${err?.status ?? 'no-status'}). ${err?.error?.message ?? ''}`.trim());
      }
    });
  }

  addComment(postId: number) {
    const text = (this.commentText[postId] || '').trim();
    if (!text) return;

    this.postsService.comment(postId, text).subscribe({
      next: () => {
        this.commentText[postId] = '';
        this.loadFeed();
      },
      error: (err) => {
        console.error(err);
        if (err?.status === 401) {
          alert('Session expired or invalid token. Please login again.');
          this.router.navigate(['/login']);
          return;
        }
        alert(`Comment failed (${err?.status ?? 'no-status'}). ${err?.error?.message ?? ''}`.trim());
      }
    });
  }
}

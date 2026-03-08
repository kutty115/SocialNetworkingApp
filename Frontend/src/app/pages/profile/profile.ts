import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { ProfileService } from '../../services/profile.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {

  loading = true;
  saving = false;

  model: any = {
    fullName: '',
    bio: '',
    profileImageUrl: ''
  };

  constructor(private profileApi: ProfileService) {}

  ngOnInit(): void {
    console.log("PROFILE INIT CALLED");   // 🔥 debug

    this.profileApi.getMe()
      .pipe(finalize(() => {
        this.loading = false;
        console.log("LOADING FALSE");
      }))
      .subscribe({
        next: (res: any) => {
          console.log("PROFILE DATA", res);

          this.model.fullName = res?.fullName ?? '';
          this.model.bio = res?.bio ?? '';
          this.model.profileImageUrl = res?.profileImageUrl ?? '';
        },
        error: (err) => {
          console.error(err);
          alert('Failed to load profile');
        }
      });
  }

save(): void {
  this.saving = true;

  this.profileApi.updateMe(this.model).subscribe({
    next: () => {
      this.saving = false;
      alert('Profile updated ✅');
    },
    error: (err) => {
      this.saving = false;
      console.error(err);
      alert('Update failed ❌');
    }
  });
}}
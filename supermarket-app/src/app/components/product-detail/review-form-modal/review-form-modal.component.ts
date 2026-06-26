import { Component, OnInit, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewApiService } from '../../../services/review-api.service';
import { Review, CreateReview, UpdateReview } from '../../../models/review';

@Component({
  selector: 'app-review-form-modal',
  imports: [CommonModule, FormsModule],
  templateUrl: './review-form-modal.component.html',
  styleUrl: './review-form-modal.component.css'
})
export class ReviewFormModalComponent implements OnInit {
  productId = input.required<number>();
  editingReview = input<Review | null>(null);

  submitted = output<void>();
  closed = output<void>();

  private reviewApi = inject(ReviewApiService);

  rating = 0;
  hoverRating = 0;
  title = '';
  content = '';
  selectedFiles: File[] = [];
  previewUrls: string[] = [];
  isSubmitting = false;
  errorMessage = '';

  get isEditing(): boolean {
    return this.editingReview() !== null;
  }

  get modalTitle(): string {
    return this.isEditing ? 'Edit Review' : 'Write a Review';
  }

  get submitLabel(): string {
    return this.isEditing ? 'Update Review' : 'Submit Review';
  }

  get isValid(): boolean {
    return this.rating > 0 && this.content.trim().length >= 10;
  }

  ngOnInit(): void {
    const edit = this.editingReview();
    if (edit) {
      this.rating = edit.rating;
      this.title = edit.title || '';
      this.content = edit.content;
    }
  }

  selectRating(star: number): void {
    this.rating = star;
  }

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const files = Array.from(input.files);
    const maxFiles = this.isEditing ? 5 - (this.editingReview()?.images.length ?? 0) : 5;

    const remaining = maxFiles - this.selectedFiles.length;
    if (remaining <= 0) {
      this.errorMessage = 'Maximum 5 images allowed';
      return;
    }

    for (const file of files.slice(0, remaining)) {
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage = `File "${file.name}" exceeds 5MB limit`;
        continue;
      }
      if (!['image/jpeg', 'image/png', 'image/gif', 'image/webp'].includes(file.type)) {
        this.errorMessage = `File "${file.name}" is not a supported image type`;
        continue;
      }
      this.selectedFiles.push(file);
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrls.push(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }

    input.value = '';
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.previewUrls.splice(index, 1);
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    if (!this.isValid || this.isSubmitting) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    if (this.selectedFiles.length > 0) {
      // Use multipart/form-data for file uploads
      const formData = new FormData();
      if (this.isEditing) {
        formData.append('rating', this.rating.toString());
        formData.append('content', this.content);
        if (this.title) formData.append('title', this.title);
        this.selectedFiles.forEach(file => formData.append('images', file));

        this.reviewApi.updateReviewMultipart(this.editingReview()!.id, formData).subscribe({
          next: (res) => {
            this.isSubmitting = false;
            if (res.success) {
              this.submitted.emit();
            } else {
              this.errorMessage = res.message || 'Failed to update review';
            }
          },
          error: (err) => {
            this.isSubmitting = false;
            this.errorMessage = err.error?.message || 'Failed to update review';
          }
        });
      } else {
        formData.append('productId', this.productId().toString());
        formData.append('rating', this.rating.toString());
        formData.append('content', this.content);
        if (this.title) formData.append('title', this.title);
        this.selectedFiles.forEach(file => formData.append('images', file));

        this.reviewApi.createReviewMultipart(formData).subscribe({
          next: (res) => {
            this.isSubmitting = false;
            if (res.success) {
              this.submitted.emit();
            } else {
              this.errorMessage = res.message || 'Failed to submit review';
            }
          },
          error: (err) => {
            this.isSubmitting = false;
            this.errorMessage = err.error?.message || 'Failed to submit review';
          }
        });
      }
    } else {
      // Use JSON for no-file submissions
      if (this.isEditing) {
        const dto: UpdateReview = {
          rating: this.rating,
          title: this.title || undefined,
          content: this.content
        };

        this.reviewApi.updateReview(this.editingReview()!.id, dto).subscribe({
          next: (res) => {
            this.isSubmitting = false;
            if (res.success) {
              this.submitted.emit();
            } else {
              this.errorMessage = res.message || 'Failed to update review';
            }
          },
          error: (err) => {
            this.isSubmitting = false;
            this.errorMessage = err.error?.message || 'Failed to update review';
          }
        });
      } else {
        const dto: CreateReview = {
          productId: this.productId(),
          rating: this.rating,
          title: this.title || undefined,
          content: this.content
        };

        this.reviewApi.createReview(dto).subscribe({
          next: (res) => {
            this.isSubmitting = false;
            if (res.success) {
              this.submitted.emit();
            } else {
              this.errorMessage = res.message || 'Failed to submit review';
            }
          },
          error: (err) => {
            this.isSubmitting = false;
            this.errorMessage = err.error?.message || 'Failed to submit review';
          }
        });
      }
    }
  }

  getRatingLabel(rating: number): string {
    switch (rating) {
      case 1: return 'Poor';
      case 2: return 'Fair';
      case 3: return 'Good';
      case 4: return 'Very Good';
      case 5: return 'Excellent';
      default: return 'Select a rating';
    }
  }
}
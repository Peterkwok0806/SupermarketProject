import { Component, Input } from '@angular/core';

/**
 * Reusable Skeleton Loading Component
 * Displays animated placeholder shapes while content is loading.
 *
 * @example
 * <app-skeleton variant="card"></app-skeleton>      <!-- Product card -->
 * <app-skeleton variant="row"></app-skeleton>        <!-- Table row -->
 * <app-skeleton variant="text" width="100px" height="16px"></app-skeleton>
 * <app-skeleton variant="circle" width="48px" height="48px"></app-skeleton>
 * <app-skeleton variant="rect" width="200px" height="120px"></app-skeleton>
 */
@Component({
  selector: 'app-skeleton',
  standalone: true,
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.css',
})
export class SkeletonComponent {
  /** Shape variant: 'card' | 'row' | 'text' | 'circle' | 'rect' */
  @Input() variant: 'card' | 'row' | 'text' | 'circle' | 'rect' = 'text';

  /** Width for text/circle/rect variants (e.g. '100px', '50%') */
  @Input() width: string = '100%';

  /** Height for text/circle/rect variants (e.g. '16px') */
  @Input() height: string = '16px';
}

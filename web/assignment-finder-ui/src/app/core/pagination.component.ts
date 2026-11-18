import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pagination" *ngIf="totalPages() > 1">
      <button type="button" [disabled]="page() <= 1" (click)="pageChange.emit(page() - 1)">
        ← Previous
      </button>
      <span>Page {{ page() }} of {{ totalPages() }}</span>
      <button type="button" [disabled]="page() >= totalPages()" (click)="pageChange.emit(page() + 1)">
        Next →
      </button>
    </div>
  `,
})
export class PaginationComponent {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly pageChange = output<number>();

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
}

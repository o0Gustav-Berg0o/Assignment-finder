import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../core/pagination.component';
import { AssignmentsService } from '../../core/assignments.service';
import { AssignmentItem } from '../../core/models';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './assignments.component.html',
})
export class AssignmentsComponent implements OnInit {
  readonly assignments = signal<AssignmentItem[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = PAGE_SIZE;

  query = '';
  area = '';
  onlyInteresting = false;

  constructor(private readonly assignmentsService: AssignmentsService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.assignmentsService
      .list({
        query: this.query,
        area: this.area,
        onlyInteresting: this.onlyInteresting,
        page: this.page(),
        pageSize: this.pageSize,
      })
      .subscribe((page) => {
        this.assignments.set(page.items);
        this.totalCount.set(page.totalCount);
      });
  }

  onFilterChange(): void {
    this.page.set(1);
    this.refresh();
  }

  goToPage(page: number): void {
    this.page.set(page);
    this.refresh();
  }

  toggleInteresting(assignment: AssignmentItem): void {
    const action = assignment.isInteresting
      ? this.assignmentsService.unmarkInteresting(assignment.id)
      : this.assignmentsService.markInteresting(assignment.id);

    action.subscribe(() => this.refresh());
  }
}

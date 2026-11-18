import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AssignmentsService } from '../../core/assignments.service';
import { AssignmentItem } from '../../core/models';

@Component({
  selector: 'app-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assignments.component.html',
})
export class AssignmentsComponent implements OnInit {
  readonly assignments = signal<AssignmentItem[]>([]);
  readonly totalCount = signal(0);

  query = '';
  area = '';
  onlyInteresting = false;

  constructor(private readonly assignmentsService: AssignmentsService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.assignmentsService
      .list({ query: this.query, area: this.area, onlyInteresting: this.onlyInteresting, pageSize: 50 })
      .subscribe((page) => {
        this.assignments.set(page.items);
        this.totalCount.set(page.totalCount);
      });
  }

  toggleInteresting(assignment: AssignmentItem): void {
    const action = assignment.isInteresting
      ? this.assignmentsService.unmarkInteresting(assignment.id)
      : this.assignmentsService.markInteresting(assignment.id);

    action.subscribe(() => this.refresh());
  }
}

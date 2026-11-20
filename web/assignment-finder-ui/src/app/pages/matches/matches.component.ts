import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../core/pagination.component';
import { MatchesService } from '../../core/matches.service';
import { MatchItem } from '../../core/models';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-matches',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './matches.component.html',
})
export class MatchesComponent implements OnInit {
  readonly matches = signal<MatchItem[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = PAGE_SIZE;

  query = '';
  area = '';
  unseenOnly = false;

  constructor(private readonly matchesService: MatchesService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.matchesService
      .list({
        query: this.query,
        area: this.area,
        unseenOnly: this.unseenOnly,
        page: this.page(),
        pageSize: this.pageSize,
      })
      .subscribe((page) => {
        if (page.items.length === 0 && page.totalCount > 0 && this.page() > 1) {
          this.page.set(Math.max(1, Math.ceil(page.totalCount / this.pageSize)));
          this.refresh();
          return;
        }
        this.matches.set(page.items);
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

  markSeen(match: MatchItem): void {
    this.matchesService.markSeen(match.id).subscribe(() => this.refresh());
  }
}

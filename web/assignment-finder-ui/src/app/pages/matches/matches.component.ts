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
  unseenOnly = false;

  constructor(private readonly matchesService: MatchesService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.matchesService.list(this.unseenOnly, this.page(), this.pageSize).subscribe((page) => {
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

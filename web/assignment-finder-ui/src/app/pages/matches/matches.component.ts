import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatchesService } from '../../core/matches.service';
import { MatchItem } from '../../core/models';

@Component({
  selector: 'app-matches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './matches.component.html',
})
export class MatchesComponent implements OnInit {
  readonly matches = signal<MatchItem[]>([]);
  readonly totalCount = signal(0);
  unseenOnly = false;

  constructor(private readonly matchesService: MatchesService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.matchesService.list(this.unseenOnly).subscribe((page) => {
      this.matches.set(page.items);
      this.totalCount.set(page.totalCount);
    });
  }

  markSeen(match: MatchItem): void {
    this.matchesService.markSeen(match.id).subscribe(() => this.refresh());
  }
}

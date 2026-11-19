import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CriteriaService } from '../../core/criteria.service';
import { Criterion } from '../../core/models';

@Component({
  selector: 'app-saved-searches',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './saved-searches.component.html',
})
export class SavedSearchesComponent implements OnInit {
  readonly savedSearches = signal<Criterion[]>([]);

  newName = '';
  newArea = '';
  newRequirements = '';
  newMinMatches = 1;

  constructor(private readonly criteriaService: CriteriaService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.criteriaService.list().subscribe((savedSearches) => this.savedSearches.set(savedSearches));
  }

  add(): void {
    if (!this.newName.trim()) {
      return;
    }

    this.criteriaService
      .create({
        name: this.newName,
        areaKeywords: this.newArea,
        requirementKeywords: this.newRequirements,
        minRequirementMatches: this.newMinMatches,
        isActive: true,
      })
      .subscribe(() => {
        this.newName = '';
        this.newArea = '';
        this.newRequirements = '';
        this.newMinMatches = 1;
        this.refresh();
      });
  }

  toggleActive(savedSearch: Criterion): void {
    this.criteriaService
      .update(savedSearch.id, { ...savedSearch, isActive: !savedSearch.isActive })
      .subscribe(() => this.refresh());
  }

  remove(savedSearch: Criterion): void {
    this.criteriaService.delete(savedSearch.id).subscribe(() => this.refresh());
  }
}

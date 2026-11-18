import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CriteriaService } from '../../core/criteria.service';
import { Criterion } from '../../core/models';

@Component({
  selector: 'app-criteria',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './criteria.component.html',
})
export class CriteriaComponent implements OnInit {
  readonly criteria = signal<Criterion[]>([]);

  newName = '';
  newArea = '';
  newRequirements = '';
  newMinMatches = 1;

  constructor(private readonly criteriaService: CriteriaService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.criteriaService.list().subscribe((criteria) => this.criteria.set(criteria));
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

  toggleActive(criterion: Criterion): void {
    this.criteriaService
      .update(criterion.id, { ...criterion, isActive: !criterion.isActive })
      .subscribe(() => this.refresh());
  }

  remove(criterion: Criterion): void {
    this.criteriaService.delete(criterion.id).subscribe(() => this.refresh());
  }
}

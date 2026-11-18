import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { BrokersService } from '../../core/brokers.service';
import { Broker } from '../../core/models';

@Component({
  selector: 'app-brokers',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './brokers.component.html',
})
export class BrokersComponent implements OnInit {
  readonly brokers = signal<Broker[]>([]);
  readonly scanningId = signal<number | null>(null);

  constructor(private readonly brokersService: BrokersService) {}

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.brokersService.list().subscribe((brokers) => this.brokers.set(brokers));
  }

  toggleEnabled(broker: Broker): void {
    this.brokersService.setEnabled(broker.id, !broker.isEnabled).subscribe(() => this.refresh());
  }

  scanNow(broker: Broker): void {
    this.scanningId.set(broker.id);
    this.brokersService.scanNow(broker.id).subscribe({
      next: () => {
        this.scanningId.set(null);
        this.refresh();
      },
      error: () => this.scanningId.set(null),
    });
  }
}

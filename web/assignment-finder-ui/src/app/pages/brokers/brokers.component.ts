import { CommonModule } from '@angular/common';
import { Component, computed, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrokersService } from '../../core/brokers.service';
import { Broker, UpsertBroker } from '../../core/models';

const EMPTY_FORM: UpsertBroker = {
  name: '',
  websiteUrl: '',
  assignmentPortalUrl: null,
  loginUrl: null,
  connectorKey: 'generic-selector',
  selectorConfigJson: null,
  isEnabled: true,
  loginUsername: null,
  loginPassword: null,
};

@Component({
  selector: 'app-brokers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './brokers.component.html',
})
export class BrokersComponent implements OnInit {
  readonly brokers = signal<Broker[]>([]);
  readonly scanningId = signal<number | null>(null);
  readonly scanningAll = signal(false);
  readonly totalAssignments = computed(() =>
    this.brokers().reduce((sum, b) => sum + b.assignmentCount, 0),
  );

  readonly editingId = signal<number | null>(null);
  readonly showForm = signal(false);
  form: UpsertBroker = { ...EMPTY_FORM };

  /** Broker id -> revealed plaintext password, fetched on demand and kept only in memory. */
  readonly revealedPasswords = signal<Record<number, string>>({});
  readonly copiedId = signal<number | null>(null);

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

  scanAll(): void {
    this.scanningAll.set(true);
    this.brokersService.scanAll().subscribe({
      next: () => {
        this.scanningAll.set(false);
        this.refresh();
      },
      error: () => this.scanningAll.set(false),
    });
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form = { ...EMPTY_FORM };
    this.showForm.set(true);
  }

  startEdit(broker: Broker): void {
    this.editingId.set(broker.id);
    this.form = {
      name: broker.name,
      websiteUrl: broker.websiteUrl,
      assignmentPortalUrl: broker.assignmentPortalUrl,
      loginUrl: broker.loginUrl,
      connectorKey: broker.connectorKey,
      selectorConfigJson: broker.selectorConfigJson,
      isEnabled: broker.isEnabled,
      loginUsername: broker.loginUsername,
      loginPassword: null,
    };
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
  }

  submitForm(): void {
    const id = this.editingId();
    const request = id === null
      ? this.brokersService.create(this.form)
      : this.brokersService.update(id, this.form);

    request.subscribe(() => {
      this.showForm.set(false);
      this.refresh();
    });
  }

  deleteBroker(broker: Broker): void {
    if (!confirm(`Delete broker "${broker.name}" and all its ${broker.assignmentCount} assignments?`)) {
      return;
    }
    this.brokersService.delete(broker.id).subscribe(() => this.refresh());
  }

  isPasswordRevealed(broker: Broker): boolean {
    return broker.id in this.revealedPasswords();
  }

  revealedPassword(broker: Broker): string {
    return this.revealedPasswords()[broker.id] ?? '';
  }

  toggleRevealPassword(broker: Broker): void {
    if (this.isPasswordRevealed(broker)) {
      this.revealedPasswords.update(({ [broker.id]: _removed, ...rest }) => rest);
      return;
    }
    this.brokersService.revealPassword(broker.id).subscribe(({ password }) => {
      this.revealedPasswords.update((current) => ({ ...current, [broker.id]: password }));
    });
  }

  copyPassword(broker: Broker): void {
    const password = this.revealedPassword(broker);
    if (!password) {
      return;
    }
    navigator.clipboard.writeText(password).then(() => {
      this.copiedId.set(broker.id);
      setTimeout(() => this.copiedId.set(null), 1500);
    });
  }

  loginLink(broker: Broker): string {
    return broker.loginUrl || broker.websiteUrl;
  }
}

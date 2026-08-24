import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, takeUntil } from 'rxjs';

import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { NotaFiscal } from '../../models/nota-fiscal.model';

@Component({
    selector: 'app-nota-fiscal-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterLink,
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatChipsModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './nota-fiscal-list.component.html'
})
export class NotaFiscalListComponent implements OnInit, OnDestroy {
    notas: NotaFiscal[] = [];
    carregando = false;
    colunas = ['numero', 'status', 'criadaEm', 'itens', 'acoes'];

    private readonly destroy$ = new Subject<void>();

    constructor(private notaFiscalService: NotaFiscalService) { }

    ngOnInit(): void {
        this.carregar();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    carregar(): void {
        this.carregando = true;
        this.notaFiscalService
            .listar()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (notas) => {
                    this.notas = notas;
                    this.carregando = false;
                },
                error: () => (this.carregando = false)
            });
    }
}

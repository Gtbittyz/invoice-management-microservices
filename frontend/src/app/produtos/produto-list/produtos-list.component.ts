import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, takeUntil } from 'rxjs';

import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';
import { ProdutoFormComponent } from '../produto-form/produto-form.component';
import { NotificationService } from '../../core/notification.service';

@Component({
    selector: 'app-produto-list',
    standalone: true,
    imports: [
        CommonModule,
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatDialogModule,
        MatProgressSpinnerModule,
    ],
    templateUrl: './produto-list.component.html'
})
export class ProdutoListComponent implements OnInit, OnDestroy {
    produtos: Produto[] = [];
    carregando = false;
    colunas = ['codigo', 'descricao', 'saldo', 'acoes'];

    private readonly destroy$ = new Subject<void>();

    constructor(
        private produtoService: ProdutoService,
        private dialog: MatDialog,
        private notification: NotificationService
    ) { }

    ngOnInit(): void {
        this.produtoService.produtos$
            .pipe(takeUntil(this.destroy$))
            .subscribe((produtos) => {
                this.produtos = produtos;
            });

        this.carregar();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    carregar(): void {
        this.carregando = true;
        this.produtoService
            .listar()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => (this.carregando = false),
                error: () => (this.carregando = false)
            });
    }

    novoProduto(): void {
        const dialogRef = this.dialog.open(ProdutoFormComponent, { width: '420px' });
        dialogRef.afterClosed().subscribe((salvou) => {
            if (salvou) {
                this.notification.sucesso('Produto cadastrado com sucesso.');
                this.carregar();
            }
        });
    }

    editarProduto(produto: Produto): void {
        const dialogRef = this.dialog.open(ProdutoFormComponent, {
            width: '420px',
            data: { produto }
        });
        dialogRef.afterClosed().subscribe((salvou) => {
            if (salvou) {
                this.notification.sucesso('Produto atualizado com sucesso.');
                this.carregar();
            }
        });
    }
}**
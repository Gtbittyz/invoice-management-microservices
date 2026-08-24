import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { Subject, catchError, finalize, of, switchMap, takeUntil } from 'rxjs';

import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { NotaFiscal } from '../../models/nota-fiscal.model';
import { NotificationService } from '../../core/notification.service';


@Component({
    selector: 'app-nota-fiscal-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterLink,
        MatCardModule,
        MatButtonModule,
        MatChipsModule,
        MatIconModule,
        MatProgressSpinnerModule,
        MatTableModule
    ],
    templateUrl: './nota-fiscal-detail.component.html'
})
export class NotaFiscalDetailComponent implements OnInit, OnDestroy {
    nota: NotaFiscal | null = null;
    carregando = false;
    imprimindo = false;
    erroImpressao: string | null = null;
    colunasItens = ['produtoCodigo', 'produtoDescricao', 'quantidade'];

    private readonly destroy$ = new Subject<void>();

    constructor(
        private route: ActivatedRoute,
        private notaFiscalService: NotaFiscalService,
        private notification: NotificationService
    ) { }

    ngOnInit(): void {
        this.carregando = true;

        this.route.paramMap
            .pipe(
                switchMap((params) => this.notaFiscalService.obterPorId(Number(params.get('id')))),
                takeUntil(this.destroy$)
            )
            .subscribe({
                next: (nota) => {
                    this.nota = nota;
                    this.carregando = false;
                },
                error: () => (this.carregando = false)
            });
        
    }
    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    imprimir(): void {
        if (!this.nota || this.nota.status !== 'Aberta') {
            return;
        }

        this.imprimindo = true;
        this.erroImpressao = null;

        this.notaFiscalService
            .imprimir(this.nota.id)
            .pipe(
                catchError((err) => {
                    this.erroImpressao =
                        err.error?.error || 'Nao foi possivel imprimir a nota fiscal. Tente novamente';
                    return of(null);
                }),
                finalize(() => this.imprimindo = false)
            )
            .subscribe((notaAtualizada) => {
                if (notaAtualizada) {
                    this.nota = notaAtualizada;
                    this.notification.sucesso(`Nota Fiscal nº ${notaAtualizada.numero} impressa e fechada com sucesso.`);
                }
            });
    }

}
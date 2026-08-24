import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Subject, finalize, takeUntil } from 'rxjs';

import { ProdutoService } from '../../services/produto.service';
import { NotaFiscalService } from '../../services/nota-fiscal.service';
import { Produto } from '../../models/produto.model';
import { NotificationService } from '../../core/notification.service';


@Component({
    selector: 'app-nota-fiscal-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatSelectModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule
    ],
    templateUrl: './nota-fiscal-form.component.html'
})
export class NotaFiscalFormComponent implements OnInit, OnDestroy {
    produtos: Produto[] = [];
    salvando = false;

    form = this.fb.group({
        itens: this.fb.array([this.criarLinhaItem()])
    });

    private readonly destroy$ = new Subject<void>();

    constructor(
        private fb: FormBuilder,
        private produtoService: ProdutoService,
        private notaFiscalService: NotaFiscalService,
        private notification: NotificationService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.produtoService
            .listar()
            .pipe(takeUntil(this.destroy$))
            .subscribe((produtos) => (this.produtos = produtos));
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    get itens(): FormArray {
        return this.form.get('itens') as FormArray;
    }

    private criarLinhaItem() {
        return this.fb.group({
            produtoId: [null as number | null, Validators.required],
            quantidade: [1, [Validators.required, Validators.min(1)]]
        });
    }

    adicionarItem(): void {
        this.itens.push(this.criarLinhaItem());
    }

    removerItem(index: number): void {
        if (this.itens.length > 1) {
            this.itens.removeAt(index);
        }
    }

    salvar(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        const itensPayload = this.itens.value.map((item: { produtoId: number; quantidade: number }) => {
            const produto = this.produtos.find((p) => p.id === item.produtoId)!;
            return {
                produtoId: produto.id,
                produtoCodigo: produto.codigo,
                produtoDescricao: produto.descricao,
                quantidade: item.quantidade
            };
        });

        this.salvando = true;
        this.notaFiscalService
            .criar({ itens: itensPayload })
            .pipe(finalize(() => (this.salvando = false)))
            .subscribe({
                next: (nota) => {
                    this.notification.sucesso(`Nota fiscal nº ${nota.numero} criada com status Aberta.`);
                    this.router.navigate(['/notas-fiscais', nota.id]);
                }
            });
    }
}

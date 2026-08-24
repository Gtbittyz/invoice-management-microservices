import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validator, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { finalize } from "rxjs";

import { ProdutoService } from '../../services/produto.service';
import { Produto } from '../../models/produto.model';

export interface ProdutoFormDialogData {
    produto?: Produto;
}

@Component({
    selector: 'app-produto-form',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule
    ],
    templateUrl: './produto-form.component.html'
})
export class ProdutoFormComponent {
    salvando = false;
    readonly modoEdicao: boolean;

    form = this.fb.group({
        codigo: ['', [Validators.required, Validators.maxLength(50)]],
        descricao: ['', [Validators.required, Validators.maxLength(200)]],
        saldo: [0, [Validators.required, Validators.min(0)]]

    });

    constructor(
        private fb: FormBuilder,
        private produtoService: ProdutoService,
        private dialogRef: MatDialogRef<ProdutoFormComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ProdutoFormDialogData
    ) {
        this.modoEdicao = !!data?.produto;

        if (data?.produto) {
            this.form.patchValue(data.produto);
            this.form.controls.codigo.disable();
        }
    }

    salvar(): void {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.salvando = true;
        const valor = this.form.getRawValue();

        if (this.modoEdicao) {
            this.produtoService
                .atualizar(this.data.produto!.id, {
                    descricao: valor.descricao!,
                    saldo: valor.saldo!
                })
                .pipe(finalize(() => (this.salvando = false)))
                .subscribe({
                    next: () => this.dialogRef.close(true)
                });
        } else {
            this.produtoService
                .criar({
                    codigo: valor.codigo!,
                    descricao: valor.descricao!,
                    saldo: valor.saldo!
                })
                .pipe(finalize(() => (this.salvando = false)))
                .subscribe({
                    next: () => this.dialogRef.close(true)
                });
        }
    }

    cancelar(): void {
        this.dialogRef.close(false);
    }
}

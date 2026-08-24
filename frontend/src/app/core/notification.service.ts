import { Injectable } from '@angular/core';
import { MatSnackBar } from "@angular/material/snack-bar";

@Injectable({ providedIn: 'root' })
export class NotificationService {
    constructor(private snackBar: MatSnackBar) { }

    sucesso(mensagem: string): void {
        this.snackBar.open(mensagem, "Ok", { duration: 4000, panelClass: 'snack-sucesso' });
    }
    erro(mensagem: string): void {
        this.snackBar.open(mensagem, 'Fechar', { duration: 6000, panelClass: 'snack-erro' });
    }
}
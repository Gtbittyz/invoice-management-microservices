import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from 'rxjs';
import { NotificationService } from "./notification.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const notification = inject(NotificationService);

    return next(req).pipe(
        catchError((err: HttpErrorResponse) => {
            const mensagem = err.error?.error || montarMensagemPadrao(err);
            notification.erro(mensagem);
            return throwError(() => err);
        })
    );
};

function montarMensagemPadrao(err: HttpErrorResponse): string {
    if (err.status === 0) {
        return `Nao foi possivel conectar ao servidor. Verifique se os servicos estao em execucao.`;
    }
    if (err.status === 502) {
        return `Um dos servicos esta indisponivel no momento. Tente novamente em instantes.`;
    }
    return `Ocorreu um erro inesperado (código ${err.status}).`;
}
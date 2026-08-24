import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { NotaFiscal, NotaFiscalCreate } from '../models/nota-fiscal.model';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
    private readonly baseUrl = `${environment.faturamentoApiUrl}/notas-fiscais`;

    constructor(private http: HttpClient) { }

    listar(): Observable<NotaFiscal[]> {
        return this.http.get<NotaFiscal[]>(this.baseUrl);
    }

    obterPorId(id: number): Observable<NotaFiscal> {
        return this.http.get<NotaFiscal>(`${this.baseUrl}/${id}`);
    }

    criar(nota: NotaFiscalCreate): Observable<NotaFiscal> {
        return this.http.post<NotaFiscal>(this.baseUrl, nota);
    }

    imprimir(id: number): Observable<NotaFiscal> {
        return this.http.post<NotaFiscal>(`${this.baseUrl}/${id}/imprimir`, {});
    }
}


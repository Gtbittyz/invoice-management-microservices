import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Produto, ProdutoCreate } from '../models/produto.model';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
    private readonly baseUrl = `${environment.estoqueApiUrl}/produtos`;

    private readonly produtosSubject = new BehaviorSubject<Produto[]>([]);
    readonly produtos$: Observable<Produto[]> = this.produtosSubject.asObservable();

    constructor(private http: HttpClient) { }

    listar(): Observable<Produto[]> {
        return this.http.get<Produto[]>(this.baseUrl).pipe(
            tap((produtos) => this.produtosSubject.next(produtos))
        );
    }

    obterPorId(id: number): Observable<Produto> {
        return this.http.get<Produto>(`${this.baseUrl}/${id}`);
    }

    criar(produto: ProdutoCreate): Observable<Produto> {
        return this.http.post<Produto>(this.baseUrl, produto).pipe(
            tap(() => this.listar().subscribe())
        );
    }

    atualizar(id: number, produto: { descricao: string, saldo: number }): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/${id}`, produto).pipe(
            tap(() => this.listar().subscribe())
        );
    }
}
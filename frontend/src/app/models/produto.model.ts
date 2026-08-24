export interface Produto {
    id: number;
    codigo: string;
    descricao: string;
    saldo: number;
}

export interface ProdutoCreate {
    codigo: string;
    descricao: string;
    saldo: number;
}
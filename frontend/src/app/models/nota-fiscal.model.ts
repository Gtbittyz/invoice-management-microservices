export type StatusNotaFiscal = 'Aberta' | 'Fechada';

export interface ItemNotaFiscal {
	produtoId: number;
	produtoCodigo: string;
	produtoDescricao: string;
	quantidade: number;
}

export interface NotaFiscal {
	id: number;
	numero: number;
	status: StatusNotaFiscal;
	criadaEm: string;
	impressaEm: string | null;
	itens: ItemNotaFiscal[];
}

export interface NotaFiscalCreate {
	itens: ItemNotaFiscal[];
}
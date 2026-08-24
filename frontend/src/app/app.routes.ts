import { M } from '@angular/cdk/keycodes';
import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'produtos', pathMatch: 'full' },
    {
        path: 'produtos',
        loadComponent: () =>
            import('./produtos/produto-list/produtos-list.component').then((m) => m.ProdutoListComponent)
    },
    {
        path: 'notas-fiscais',
        loadComponent: () =>
            import('./notas-fiscais/nota-fiscal-list/nota-fiscal-list.component').then((m) => m.NotaFiscalListComponent)
    },
    {
        path: 'notas-fiscais/nova',
        loadComponent: () =>
            import('./notas-fiscais/nota-fiscal-form/nota-fiscal-form.component').then((m) => m.NotaFiscalFormComponent)
    },
    {
        path: 'notas-fiscais/:id',
        loadComponent: () =>
            import('./notas-fiscais/nota-fiscal-detail/nota-fiscal.detail.component').then((m) => m.NotaFiscalDetailComponent)
    },
    {path: '**', redirectTo: 'produtos'}
]
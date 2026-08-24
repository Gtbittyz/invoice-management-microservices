# Detalhamento Técnico — Sistema de Notas Fiscais

Este documento responde diretamente aos pontos exigidos pelo teste técnico da Korp.

## 1. Arquitetura geral

- **Frontend**: Angular 17 (standalone components) + Angular Material.
- **Backend**: dois microsserviços em C# / .NET 8:
  - `EstoqueService` — cadastro de produtos e controle de saldo.
  - `FaturamentoService` — cadastro e impressão de notas fiscais; consome o `EstoqueService` via HTTP.
- **Banco de dados**: PostgreSQL, um banco por serviço (`estoque_db` e `faturamento_db`) — padrão *database per service*.
- **Orquestração local**: `docker-compose.yml` na raiz sobe os dois bancos, os dois serviços e o frontend.

## 2. Ciclos de vida do Angular utilizados

| Hook | Onde | Por quê |
|---|---|---|
| `ngOnInit` | `ProdutoListComponent`, `NotaFiscalListComponent`, `NotaFiscalFormComponent`, `NotaFiscalDetailComponent` | Disparar o carregamento inicial de dados (produtos e notas fiscais) assim que o componente é montado. |
| `ngOnDestroy` | Todos os componentes acima | Cancelar inscrições ativas em Observables (via `Subject` + `takeUntil`), evitando vazamento de memória e chamadas HTTP "fantasmas" ao trocar de tela. |

Não há necessidade de `ngOnChanges`, `ngAfterViewInit` ou outros hooks mais avançados neste escopo, já que os componentes não recebem `@Input()` complexos nem precisam manipular a DOM diretamente após a renderização — o Angular Material e o binding de template resolvem tudo declarativamente.

## 3. Uso do RxJS

RxJS é usado em várias camadas, não só para chamadas HTTP isoladas:

- **Estado compartilhado reativo**: `ProdutoService` mantém um `BehaviorSubject<Produto[]>` (`produtos$`), permitindo que a listagem de produtos e o formulário de nota fiscal compartilhem a mesma lista sem duplicar chamadas HTTP.
- **Cancelamento de inscrições**: padrão `Subject<void>` + `takeUntil(destroy$)` em todos os componentes com dados assíncronos.
- **Encadeamento de streams**: `switchMap` em `NotaFiscalDetailComponent` para converter o parâmetro de rota (`ActivatedRoute.paramMap`) diretamente na chamada HTTP de detalhe da nota.
- **Tratamento de erro no fluxo**: `catchError` (para capturar falha específica da impressão sem quebrar o Observable) e `finalize` (para sempre desligar o spinner de carregamento, com sucesso ou erro).
- **Interceptor HTTP funcional**: `errorInterceptor` usa `catchError` + `throwError` para centralizar o tratamento de erros de todas as chamadas HTTP em um único lugar.

## 4. Outras bibliotecas utilizadas (frontend)

| Biblioteca | Finalidade |
|---|---|
| `@angular/material` + `@angular/cdk` | Componentes visuais (toolbar, tabelas, formulários, cards, chips, spinners, dialog, snackbar). |
| `@angular/forms` (Reactive Forms) | Formulários de produto e de nota fiscal, incluindo `FormArray` para a lista dinâmica de itens da nota. |
| `rxjs` | Programação reativa (ver seção acima). |

## 5. Bibliotecas de componentes visuais

**Angular Material** foi a biblioteca escolhida (toolbar, table, card, chips, dialog, form-field, select, snack-bar, progress-spinner, icon, button). Motivo: integração nativa com Angular, componentes acessíveis por padrão e cobre 100% das necessidades de UI do escopo sem precisar de CSS customizado extenso.

## 6. Gerenciamento de dependências (Golang)

Não aplicável nesta entrega — o backend foi implementado em **C# / .NET**, conforme alinhado. O gerenciamento de dependências do backend é feito via **NuGet**, através dos arquivos `.csproj` de cada microsserviço (`EstoqueService.csproj` e `FaturamentoService.csproj`), que declaram os pacotes (`Microsoft.EntityFrameworkCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Polly`, `Microsoft.Extensions.Http.Polly`, `Swashbuckle.AspNetCore`) e suas versões.

## 7. Frameworks utilizados (C#)

- **ASP.NET Core 8 Web API** — framework HTTP dos dois microsserviços (controllers, roteamento, middleware pipeline).
- **Entity Framework Core 8** com o provider **Npgsql** — ORM para acesso ao PostgreSQL.
- **Polly** (via `Microsoft.Extensions.Http.Polly`) — políticas de resiliência (retry + circuit breaker) nas chamadas HTTP entre os microsserviços.
- **Swashbuckle (Swagger)** — documentação/exploração interativa das APIs.

## 8. Tratamento de erros e exceções no backend

Cada serviço tem um **middleware global de tratamento de exceções** (`ExceptionHandlingMiddleware`), que:

- Captura `KeyNotFoundException` → `404 Not Found` (ex.: produto ou nota inexistente).
- Captura `InvalidOperationException` → `422 Unprocessable Entity` (regras de negócio: saldo insuficiente, código duplicado, nota já fechada).
- No `FaturamentoService`, captura uma exceção de domínio própria, `EstoqueIndisponivelException` → `502 Bad Gateway`, lançada pelo `EstoqueClient` quando o `EstoqueService` não responde (timeout, conexão recusada, erro 5xx) mesmo após as tentativas de retry do Polly.
- Qualquer outra exceção não mapeada → `500 Internal Server Error`, sempre logada via `ILogger`, sem vazar stack trace para o cliente.

Todas as respostas de erro seguem um formato JSON padronizado (`status`, `error`, `timestamp`), o que permite ao frontend (via interceptor HTTP) mostrar uma mensagem amigável e consistente independentemente de qual dos dois serviços gerou o erro.

### Cenário de falha entre microsserviços (requisito obrigatório)

O `FaturamentoServic  e` expõe o endpoint `POST /api/diagnostico/simular-falha?ativar=true` no `EstoqueService`, que força esse serviço a responder `503` para qualquer chamada de negócio — usado para **demonstrar em vídeo** a queda do microsserviço.

Fluxo de recuperação:

1. Ao imprimir uma nota, o `FaturamentoService` chama `POST /produtos/{id}/baixa` no `EstoqueService` através do `EstoqueClient`.
2. O `HttpClient` desse cliente tem duas políticas do **Polly** configuradas: retry com backoff exponencial (3 tentativas: 200ms, 400ms, 800ms) para erros transitórios, e um circuit breaker (abre após 5 falhas seguidas, por 15s) para não insistir indefinidamente em um serviço fora do ar.
3. Se, mesmo assim, a chamada falhar, o `EstoqueClient` lança `EstoqueIndisponivelException`, capturada pelo middleware e traduzida em `502` com mensagem clara.
4. Se a nota tiver múltiplos itens e algum item falhar **depois** que outros já tiveram baixa de saldo, o controller de notas fiscais estorna (compensa) o saldo já baixado antes de propagar o erro — a nota permanece `Aberta` e o estoque não fica inconsistente.
5. O frontend recebe o erro via interceptor HTTP global, exibe um snackbar e uma mensagem inline na tela de detalhe da nota, sem travar a aplicação.

## 9. Uso de LINQ (C#)

LINQ é usado extensivamente nas consultas via Entity Framework Core, por exemplo:

- `EstoqueService`: filtro (`Where(p => p.Saldo > 0)`), ordenação (`OrderBy(p => p.Codigo)`), projeção para DTO (`Select(...)`) e verificação de existência (`AnyAsync(p => p.Codigo == dto.Codigo)`).
- `FaturamentoService`: `Include(n => n.Itens)` combinado com `OrderByDescending(n => n.Numero)` e `Select(...)` para montar a lista de notas fiscais; `MaxAsync(n => n.Numero)` para calcular o próximo número sequencial.

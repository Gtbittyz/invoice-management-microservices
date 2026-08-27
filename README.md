# Korp_Test_GustavoTavares — Sistema de Emissão de Notas Fiscais

Projeto desenvolvido para o teste técnico da Korp: cadastro de produtos,
emissão de notas fiscais e impressão com baixa automática de estoque,
implementado como dois microsserviços em **C# (.NET 8)** + frontend em
**Angular 17**, com **PostgreSQL** como banco de dados.

## Estrutura do repositório

```
# Korp_Test_GustavoTavare/
├── backend/
│   ├── EstoqueService/        # Microsserviço de Estoque (produtos e saldo)
│   └── FaturamentoService/     # Microsserviço de Faturamento (notas fiscais)
├── frontend/                   # Aplicação Angular
├── docker-compose.yml          # Sobe tudo (bancos + serviços + frontend) de uma vez
└── DETALHAMENTO_TECNICO.md     # Documento técnico exigido no teste
```

## Como rodar

### Opção 1 — Docker Compose

Requer apenas Docker e Docker Compose instalados.

```bash
docker compose up --build
```

- Frontend: http://localhost:4200
- EstoqueService (Swagger): http://localhost:5080/swagger
- FaturamentoService (Swagger): http://localhost:5081/swagger

### Opção 2 — Rodando manualmente

Requer .NET 8 SDK, Node.js 20+ e um PostgreSQL local.

```bash
# 1. Suba um Postgres local (ou ajuste as connection strings nos appsettings.json)
#    para apontar para o seu banco.

# 2. EstoqueService
cd backend/EstoqueService
dotnet restore
dotnet run
# sobe em http://localhost:5080

# 3. FaturamentoService (em outro terminal)
cd backend/FaturamentoService
dotnet restore
dotnet run
# sobe em http://localhost:5081

# 4. Frontend (em outro terminal)
cd frontend
npm install
npm start
# sobe em http://localhost:4200
```

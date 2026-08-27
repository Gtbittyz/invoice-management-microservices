# Invoice Management Microservice — Sistema de Emissão de Notas Fiscais

Sistema completo de gestão de estoque e faturamento construído com arquitetura de microsserviços em .NET 8
e frontend dinâmico em Angular 17. A aplicação orquestra a baixa automática de saldo de estoque integrada 
à emissão e impressão de notas fiscais, utilizando PostgreSQL para persistência de dados.

## Estrutura do repositório

```
# invoice-management-microservices/
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

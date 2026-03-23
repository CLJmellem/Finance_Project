# Transactions API

Microserviço responsável pelo CRUD de transações de cartão de crédito vinculadas ao usuário autenticado.

---

## Estrutura

```
Finance_Project.Transactions.api/
└── src/
    ├── TransactionsService.API/            → Controllers, middlewares, configuração
    ├── TransactionsService.Application/    → Commands, Queries, DTOs, validações (CQRS)
    ├── TransactionsService.Domain/         → Entidades, Enums, interfaces de repositório
    ├── TransactionsService.Infrastructure/ → MongoDB, repositórios, background service
    └── TransactionsService.Tests/          → Testes unitários (xUnit + Moq)
```

---

## Endpoints

Todos os endpoints requerem `Authorization: Bearer <token>`.

| Método | Rota | Descrição | Role |
|--------|------|-----------|------|
| `GET` | `/api/transactions` | Lista transações (filtros + paginação) | Todos |
| `GET` | `/api/transactions/{cardId}/{transactionId}` | Retorna uma transação pelo ID | Todos |
| `POST` | `/api/transactions/{cardId}` | Cria uma nova transação | Todos |
| `PUT` | `/api/transactions/{cardId}/{transactionId}` | Atualiza uma transação (partial update) | Todos |
| `DELETE` | `/api/transactions/{cardId}/{transactionId}` | Remove a transação permanentemente | Todos |

> **Admin** pode informar `userId` opcionalmente para acessar dados de outro usuário. Se não informar, usa o próprio `userId` do token.
> **FreeUser** acessa apenas os próprios dados — `userId` é sempre extraído do JWT.

### Regras de negócio

- O cartão (`cardId`) deve existir e estar **ativo** para qualquer operação. Retorna `404 Not Found` se não encontrado; `422 Unprocessable Entity` se encontrado mas inativo.
- **POST** valida se o `TotalAmount` não excede o `CreditLimit` do cartão individualmente e também em somatória com as transações existentes.
- **PUT** recalcula o crédito disponível excluindo o valor anterior da própria transação antes de validar.
- **DELETE** realiza hard delete (remove o documento do MongoDB).
- **GET /api/transactions** — se `page` e `pageSize` não forem informados, retorna todas as transações sem paginação.

---

## Request / Response

**POST /api/transactions/{cardId}**
```json
{
  "totalAmount": 500.00,
  "monthAmount": 125.00,
  "installments": 4,
  "type": 0,
  "description": "Compra parcelada no shopping",
  "isRecurring": false,
  "userId": null
}
```

**PUT /api/transactions/{cardId}/{transactionId}**
```json
{
  "totalAmount": 600.00,
  "monthAmount": null,
  "installments": null,
  "type": 1,
  "description": "Descrição atualizada",
  "userId": null
}
```

**Resposta padrão (TransactionResponse)**
```json
{
  "id": "...",
  "userId": "...",
  "cardId": "...",
  "totalAmount": 500.00,
  "monthAmount": 125.00,
  "installments": 4,
  "actualInstallments": 4,
  "type": "OnlineShopping",
  "description": "Compra parcelada no shopping",
  "isRecurring": false,
  "cardDueDate": "2026-04-10T00:00:00Z",
  "createdAt": "2026-03-23T00:00:00Z",
  "lastUpdated": null
}
```

**Resposta paginada (GET /api/transactions)**
```json
{
  "items": [ /* TransactionResponse[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 35,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Tipos de transação (`type`)**

| Valor | Tipo |
|-------|------|
| `0` | Subscription |
| `1` | Groceries |
| `2` | Delivery |
| `3` | OnlineShopping |
| `4` | Education |
| `5` | Pharmacy |
| `6` | Healthcare |
| `7` | RideApps |
| `8` | InStorePurchase |
| `9` | Entertainment |
| `10` | VehicleExpenses |
| `11` | Rent |
| `12` | Utilities |
| `13` | Internet |
| `14` | Mobile |
| `15` | Restaurants |

---

## Filtros disponíveis — GET /api/transactions

| Query param | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `userId` | string (ObjectId) | Não | Admin pode filtrar por outro usuário |
| `cardId` | string (ObjectId) | Não | Filtra por cartão específico |
| `transactionId` | string (ObjectId) | Não | Filtra por transação específica |
| `cardName` | string | Não | Busca cartão pelo nome e retorna suas transações |
| `lastFourDigits` | string (4 dígitos) | Não | Busca cartão pelos últimos 4 dígitos |
| `page` | int | Não | Número da página (0 = sem paginação) |
| `pageSize` | int | Não | Tamanho da página (0 = sem paginação) |

> Prioridade dos filtros de cartão: `cardId` > `cardName` > `lastFourDigits`.

---

## Parcelamento e recorrência

### Compras parceladas

- Forneça `installments` (mín. 2) e `monthAmount` juntos — ambos são obrigatórios entre si.
- `actualInstallments` é preenchido com o valor de `installments` na criação.
- O **background service** decrementa `actualInstallments` mensalmente. Ao chegar a 0, o documento é deletado automaticamente.
- O cálculo de crédito disponível usa `actualInstallments × monthAmount` como saldo comprometido da parcela.

### Compras recorrentes

- Defina `isRecurring: true` para assinaturas e gastos fixos mensais.
- O **background service** renova automaticamente a transação a cada mês com novo `cardDueDate`.
- Não é necessário registrar a mesma transação todo mês.

### CardDueDate

Calculado automaticamente na criação: próximo mês a partir do `DueDay` do cartão.

> Exemplo: cartão com `DueDay = 10`, transação criada em março → `cardDueDate = 10 de abril`.

---

## Health Checks

| Rota | Tipo |
|------|------|
| `GET /health` | Liveness |
| `GET /health/ready` | Readiness (inclui ping no MongoDB) |

---

## Configuração

`appsettings.json` — sobrescreva via variáveis de ambiente em produção:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "Your-MongoDB-ConnectionString",
    "DatabaseName": "Your-DatabaseName"
  },
  "Jwt": {
    "Key": "Your-Jwt-Secret-Key",
    "Issuer": "Your-Issuer",
    "Audience": "Your-Audience"
  }
}
```

> A chave JWT deve ter no mínimo 64 caracteres para o algoritmo HMAC-SHA512. O token é validado localmente — nenhuma chamada à Auth API por request.

---

## Documentação da API (Scalar)

A documentação interativa é servida pelo **Scalar** (substitui Swagger/Swashbuckle).

Disponível apenas no ambiente de desenvolvimento:

```
http://localhost:<porta>/scalar/v1
```

O schema OpenAPI bruto fica em `/openapi/v1.json`.

---

## Executando localmente

```bash
cd src/TransactionsService.API
dotnet run
```

## Executando testes

```bash
dotnet test src/TransactionsService.Tests/TransactionsService.Tests/TransactionsService.Tests.csproj
```

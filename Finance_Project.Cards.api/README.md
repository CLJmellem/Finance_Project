# Cards API

Microserviço responsável pelo CRUD de cartões de crédito vinculados ao usuário autenticado.

---

## Estrutura

```
Finance_Project.Cards.api/
└── src/
    ├── CardsService.API/            → Controllers, middlewares, configuração
    ├── CardsService.Application/    → Commands, Queries, DTOs, validações (CQRS)
    ├── CardsService.Domain/         → Entidades, Enums, interfaces de repositório
    ├── CardsService.Infrastructure/ → MongoDB, repositórios
    └── CardsService.Tests/          → Testes unitários (xUnit + Moq)
```

---

## Endpoints

Todos os endpoints requerem `Authorization: Bearer <token>`.

| Método | Rota | Descrição | Role |
|--------|------|-----------|------|
| `GET` | `/api/cards` | Lista cartões (paginado) | Todos |
| `GET` | `/api/cards/{id}` | Retorna um cartão pelo ID | Todos |
| `POST` | `/api/cards` | Cria um novo cartão | Todos |
| `PUT` | `/api/cards/{id}` | Atualiza nome, limite, vencimento e status | Todos |
| `DELETE` | `/api/cards/{id}` | Remove o cartão permanentemente | Todos |

> **Admin** enxerga cartões de qualquer usuário — deve informar `userId` via query param em `GET /api/cards`.
> **FreeUser** acessa apenas os próprios dados — `userId` é extraído do JWT.

### Regras de negócio

- **GET /api/cards** aceita os query params `page`, `pageSize`, `userId` (Admin obrigatório), `inactiveCards`.
  - Se `page` e `pageSize` não forem informados, retorna todos os registros sem paginação.
  - Por padrão retorna apenas cartões ativos; passe `inactiveCards=true` para incluir inativos.
- **DELETE /api/cards/{id}** realiza hard delete (remove o documento do MongoDB). O cartão deve estar inativo antes de ser deletado.

---

## Request / Response

**POST /api/cards**
```json
{
  "name": "Nubank",
  "brand": 0,
  "lastFourDigits": "1234",
  "creditLimit": 5000.00,
  "dueDay": 10
}
```

**PUT /api/cards/{id}**
```json
{
  "name": "Nubank Black",
  "creditLimit": 10000.00,
  "dueDay": 15,
  "isActive": false
}
```

**Resposta padrão (CardResponse)**
```json
{
  "id": "...",
  "userId": "...",
  "name": "Nubank",
  "brand": "Visa",
  "lastFourDigits": "1234",
  "creditLimit": 5000.00,
  "dueDay": 10,
  "isActive": true,
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Resposta paginada (GET /api/cards)**
```json
{
  "items": [ /* CardResponse[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 42,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Bandeiras disponíveis (`brand`)**

| Valor | Bandeira |
|-------|----------|
| `0` | Visa |
| `1` | Mastercard |
| `2` | Elo |
| `3` | AmericanExpress |
| `4` | Hipercard |
| `99` | Other |

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
    "Audience": "Your-Audience",
    "Expiration": "Your-Expiration"
  }
}
```

> A chave JWT deve ter no mínimo 64 caracteres para o algoritmo HMAC-SHA512. `Expiration` é em minutos.

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
cd src/CardsService.API
dotnet run
```

## Executando testes

```bash
dotnet test src/CardsService.Tests/CardsService.Tests/CardsService.Tests.csproj
```

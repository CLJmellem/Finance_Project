# Finance Project

Sistema de finanças pessoais com foco na organização de gastos em cartões de crédito.

Desenvolvido como TCC: **"Resiliência em Microserviços: Aplicação do Circuit Breaker no .NET"**.

---

## Ideia do projeto

O sistema permite que usuários cadastrem seus cartões e registrem transações, organizando gastos por cartão e período. A arquitetura é baseada em microserviços, com um API Gateway centralizando o Circuit Breaker (Polly v8) para demonstrar resiliência em caso de falha de um dos serviços.

---

## Serviços

| Serviço | Status | Repositório / Pasta |
|---------|--------|---------------------|
| Auth API | ✅ Funcional | `Finance_Project.Authentication.api/` |
| Cards API | ✅ Funcional | `Finance_Project.Cards.api/` |
| Transactions API | ✅ Funcional | `Finance_Project.Transactions.api/` |
| API Gateway + Circuit Breaker | ✅ Funcional | `Finance_Project.ApiGateway/` |

---

## Stack

- .NET 10 / ASP.NET Core 10
- MongoDB (instância única, uma collection por entidade)
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- Polly v8 (Circuit Breaker no Gateway)
- JWT — HMAC-SHA512, validação local em cada serviço
- Scalar (documentação interativa da API)

---

## Autenticação

Todos os endpoints (exceto Auth) exigem `Authorization: Bearer <token>`.

O token é emitido pela Auth API. Roles disponíveis:

| Role | Valor no JWT | Acesso |
|------|-------------|--------|
| Admin | `0` | Todos os dados de todos os usuários |
| FreeUser | `1` | Apenas os próprios dados |

---

## Auth API

> `Finance_Project.Authentication.api/` — funcional.

Responsável por registro, login, renovação e revogação de tokens JWT.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/api/v1/Authentication/register` | Não | Cria conta de usuário |
| `POST` | `/api/v1/Authentication/login` | Não | Autentica e retorna access + refresh token |
| `POST` | `/api/v1/Authentication/refresh` | Não | Renova o access token via refresh token |
| `DELETE` | `/api/v1/Authentication/logout` | Bearer | Revoga o refresh token |

Mais detalhes: [`Finance_Project.Authentication.api/README.md`](Finance_Project.Authentication.api/README.md)

---

## Cards API

> `Finance_Project.Cards.api/` — funcional.

Gerencia os cartões de crédito do usuário.

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/cards` | Lista cartões paginados (Admin requer `userId` via query param) |
| `GET` | `/api/cards/{id}` | Retorna um cartão pelo ID |
| `POST` | `/api/cards` | Cadastra novo cartão |
| `PUT` | `/api/cards/{id}` | Atualiza nome, limite, vencimento e status |
| `DELETE` | `/api/cards/{id}` | Remove o cartão permanentemente (hard delete; exige cartão inativo) |

Mais detalhes: [`Finance_Project.Cards.api/README.md`](Finance_Project.Cards.api/README.md)

---

## Transactions API

> `Finance_Project.Transactions.api/` — funcional.

Gerencia as transações de cartão de crédito, com suporte a parcelamento e cobranças recorrentes.

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/transactions` | Lista transações com filtros (paginado) |
| `GET` | `/api/transactions/{cardId}/{transactionId}` | Retorna uma transação pelo ID |
| `POST` | `/api/transactions/{cardId}` | Registra nova transação |
| `PUT` | `/api/transactions/{cardId}/{transactionId}` | Atualiza uma transação (partial update) |
| `DELETE` | `/api/transactions/{cardId}/{transactionId}` | Remove a transação permanentemente |

Mais detalhes: [`Finance_Project.Transactions.api/README.md`](Finance_Project.Transactions.api/README.md)

---

## API Gateway + Circuit Breaker

> `Finance_Project.ApiGateway/` — funcional.

Ponto de entrada único do sistema. Reverse proxy via YARP que roteia requisições para Cards API, Transactions API e Auth API, aplicando Circuit Breaker via Polly v8 para isolar falhas entre serviços.

| Rota | Serviço destino | Auth |
|------|----------------|------|
| `/api/cards/{**catch-all}` | Cards API | Bearer |
| `/api/transactions/{**catch-all}` | Transactions API | Bearer |
| `/api/v1/Authentication/{**catch-all}` | Auth API | Anônimo |

Mais detalhes: [`Finance_Project.ApiGateway/README.md`](Finance_Project.ApiGateway/README.md)

---

## Health Checks

Cada serviço expõe:

| Rota | Tipo |
|------|------|
| `GET /health` | Liveness |
| `GET /health/ready` | Readiness (verifica dependências, ex: MongoDB) |

---

## Portas

| Serviço | HTTPS | HTTP |
|---------|-------|------|
| API Gateway | `65000` | `65001` |
| Auth API | `65002` | `65003` |
| Cards API | `65006` | `65007` |
| Transactions API | `65010` | `65011` |

---

## Executando localmente

Cada serviço é independente e possui seu próprio `.slnx`.

```bash
# Auth API
cd Finance_Project.Authentication.api/src/Auth.Api
dotnet run

# Cards API
cd Finance_Project.Cards.api/src/CardsService.API
dotnet run
```

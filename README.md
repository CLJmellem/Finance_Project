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
| Auth API | ✅ Funcional | Repositório separado |
| Cards API | ✅ Funcional | `Finance_Project.Cards.api/` |
| Transactions API | 🚧 WIP | — |
| API Gateway + Circuit Breaker | 🚧 WIP | — |

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

> Repositório separado — já funcional.

Responsável por registro, login, renovação e revogação de tokens JWT.

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `POST` | `/api/v1/Authentication/register` | ✅ | Cria conta de usuário |
| `POST` | `/api/v1/Authentication/login` | ✅ | Autentica e retorna access + refresh token |
| `POST` | `/api/v1/Authentication/refresh` | ✅ | Renova o access token via refresh token |
| `DELETE` | `/api/v1/Authentication/logout` | ✅ Bearer | Revoga o refresh token |

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

## Transactions API — 🚧 WIP

> Não implementada ainda.

Registrará as transações vinculadas a um cartão.

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/transactions` | Lista transações do usuário |
| `GET` | `/api/transactions/{id}` | Retorna uma transação pelo ID |
| `POST` | `/api/transactions` | Registra nova transação |
| `PUT` | `/api/transactions/{id}` | Atualiza uma transação |
| `DELETE` | `/api/transactions/{id}` | Remove uma transação |

---

## API Gateway + Circuit Breaker — 🚧 WIP

> Não implementado ainda.

Ponto de entrada único. Roteará as requisições para Cards API e Transactions API, aplicando Circuit Breaker via Polly v8 para isolar falhas entre serviços.

---

## Health Checks

Cada serviço expõe:

| Rota | Tipo |
|------|------|
| `GET /health` | Liveness |
| `GET /health/ready` | Readiness (verifica dependências, ex: MongoDB) |

---

## Executando localmente

Cada serviço é independente e possui seu próprio `.sln`.

```bash
# Cards API
cd Finance_Project.Cards.api/src/CardsService.API
dotnet run
```

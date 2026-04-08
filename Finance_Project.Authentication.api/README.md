# Authentication API

Microservico responsavel pelo registro, login, renovacao e revogacao de tokens JWT.

---

## Estrutura

```
Finance_Project.Authentication.api/
└── src/
    ├── Auth.Api/              → Controllers, middlewares, configuracao
    ├── Auth.Application/      → Commands, DTOs, validacoes (CQRS)
    ├── Auth.Domain/           → Entidades, Enums, interfaces de repositorio
    ├── Auth.Infrastructure/   → MongoDB, repositorios, servicos (JWT, BCrypt)
    └── Auth.Test/             → Testes unitarios
```

---

## Endpoints

| Metodo | Rota | Auth | Descricao |
|--------|------|------|-----------|
| `POST` | `/api/v1/Authentication/register` | Nao | Cria conta de usuario |
| `POST` | `/api/v1/Authentication/login` | Nao | Autentica e retorna access + refresh token |
| `POST` | `/api/v1/Authentication/refresh` | Nao | Renova o access token via refresh token |
| `DELETE` | `/api/v1/Authentication/logout` | Bearer | Revoga o refresh token |

### Regras de negocio

- **Register:** Valida username, email (unico), senha (minimo de requisitos). Senha armazenada com BCrypt.
- **Login:** Retorna `accessToken` (30 min) + `refreshToken` (7 dias). Se ja existir um refresh token valido, ele e reutilizado.
- **Refresh:** Recebe o refresh token e retorna um novo access token. O refresh token nao e rotacionado.
- **Logout:** Requer `Authorization: Bearer <accessToken>`. Extrai `userId` do token e deleta o registro de refresh token.

---

## Request / Response

**POST /api/v1/Authentication/register**
```json
{
  "username": "usuario",
  "email": "usuario@email.com",
  "password": "SenhaForte123!",
  "confirmPassword": "SenhaForte123!"
}
```

**POST /api/v1/Authentication/login**
```json
{
  "email": "usuario@email.com",
  "password": "SenhaForte123!"
}
```

**Resposta do login:**
```json
{
  "accessToken": "eyJhbGciOiJIUzUxMiIs...",
  "refreshToken": "base64-encoded-random-bytes"
}
```

**POST /api/v1/Authentication/refresh**
```json
{
  "refreshToken": "base64-encoded-random-bytes"
}
```

---

## Roles

| Role | Valor no JWT | Acesso |
|------|-------------|--------|
| Admin | `0` | Todos os dados de todos os usuarios |
| FreeUser | `1` | Apenas os proprios dados |

O token JWT inclui `ClaimTypes.NameIdentifier` (userId), `ClaimTypes.Name` (username) e `ClaimTypes.Role`.

---

## Health Checks

| Rota | Tipo |
|------|------|
| `GET /health` | Liveness |
| `GET /health/ready` | Readiness (inclui ping no MongoDB) |

---

## Configuracao

`appsettings.json` — sobrescreva via variaveis de ambiente em producao:

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
    "Expiration": "30",
    "RefreshExpiration": "10080"
  },
  "Encryption": {
    "Key": "Your-Encryption-Key"
  }
}
```

> A chave JWT deve ter no minimo 64 caracteres para o algoritmo HMAC-SHA512. `Expiration` e em minutos (30 = 30 min). `RefreshExpiration` tambem em minutos (10080 = 7 dias).

---

## Documentacao da API (Scalar)

A documentacao interativa e servida pelo **Scalar** (substitui Swagger/Swashbuckle).

Disponivel apenas no ambiente de desenvolvimento:

```
https://localhost:65002/scalar/v1
```

O schema OpenAPI bruto fica em `/openapi/v1.json`.

---

## Executando localmente

```bash
cd src/Auth.Api
dotnet run
```

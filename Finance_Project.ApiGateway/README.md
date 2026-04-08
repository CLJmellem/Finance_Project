# API Gateway

Ponto de entrada único do sistema. Atua como reverse proxy com **Circuit Breaker** centralizado (Polly v8) para isolar falhas entre microserviços.

Componente central do TCC: **"Resiliencia em Microservicos: Aplicacao do Circuit Breaker no .NET"**.

---

## Estrutura

```
Finance_Project.ApiGateway/
├── Configuration/
│   └── Bootstrapper.cs              → Registro de DI (YARP, Circuit Breaker, health checks, OpenAPI)
├── Factory/
│   └── ResilientForwarderHttpClientFactory.cs → Injeta Polly Circuit Breaker no pipeline HTTP do YARP
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs → Mapeia exceções para respostas HTTP adequadas
├── Models/
│   └── CircuitBreakerSettings.cs     → Configuração do Circuit Breaker (bind do appsettings)
├── Services/
│   └── DownstreamHealthCheck.cs      → Health check de conectividade com serviços downstream
├── Program.cs                        → Entry point
├── appsettings.json                  → Rotas YARP + configuração do Circuit Breaker
└── Dockerfile                        → Multi-stage build com non-root user
```

---

## Roteamento (YARP)

Todas as requisicoes passam pelo Gateway, que encaminha para o servico correto via YARP.

| Rota | Servico destino | Porta (HTTPS) | Auth |
|------|----------------|---------------|------|
| `/api/cards/{**catch-all}` | Cards API | `localhost:65006` | Bearer (obrigatorio) |
| `/api/transactions/{**catch-all}` | Transactions API | `localhost:65010` | Bearer (obrigatorio) |
| `/api/v1/Authentication/{**catch-all}` | Auth API | `localhost:65002` | Anonimo |

> O token JWT e encaminhado automaticamente pelo YARP para os servicos downstream.

---

## Circuit Breaker (Polly v8)

O Gateway aplica um Circuit Breaker **por cluster** (um para cada servico downstream). Isso garante que a falha de um servico nao derrube os demais.

### Estados

```
Closed (normal) → Open (falhas detectadas) → Half-Open (teste de recuperacao)
                                    ↑                          ↓
                                    └──── Open (ainda falha) ←─┘
                                                               ↓
                                                         Closed (recuperou)
```

### Configuracao padrao

| Parametro | Valor | Descricao |
|-----------|-------|-----------|
| `FailureRatio` | `0.5` | 50% de falhas dentro da janela dispara abertura |
| `SamplingDurationSeconds` | `30` | Janela de observacao (30 segundos) |
| `MinimumThroughput` | `5` | Minimo de requisicoes para avaliar |
| `BreakDurationSeconds` | `30` | Tempo que o circuito permanece aberto |

### Tratamento de excecoes

| Excecao | HTTP Status | Significado |
|---------|-------------|-------------|
| `BrokenCircuitException` | `503 Service Unavailable` | Circuito aberto — servico downstream indisponivel |
| `HttpRequestException` | `502 Bad Gateway` | Falha na comunicacao com o servico downstream |
| `TimeoutException` | `504 Gateway Timeout` | Servico downstream nao respondeu a tempo |

### Implementacao

- `ResilientForwarderHttpClientFactory` — factory customizada do YARP que envolve o `HttpMessageHandler` com o pipeline de resiliencia do Polly.
- Um `ResiliencePipeline<HttpResponseMessage>` independente por cluster (cards, transactions, auth).
- Callbacks (`OnOpened`, `OnClosed`, `OnHalfOpened`) registram transicoes de estado via `ILogger`.

---

## Health Checks

| Rota | Tipo | Descricao |
|------|------|-----------|
| `GET /health` | Liveness | Verifica se o Gateway esta rodando |
| `GET /health/ready` | Readiness | Verifica conectividade com os servicos downstream |

---

## Configuracao

`appsettings.json` — sobrescreva via variaveis de ambiente em producao:

```json
{
  "Jwt": {
    "Key": "Your-Jwt-Secret-Key",
    "Issuer": "Your-Issuer",
    "Audience": "Your-Audience"
  },
  "CircuitBreaker": {
    "FailureRatio": 0.5,
    "SamplingDurationSeconds": 30,
    "MinimumThroughput": 5,
    "BreakDurationSeconds": 30
  },
  "ReverseProxy": {
    "Routes": { "..." },
    "Clusters": { "..." }
  }
}
```

> A chave JWT deve ser a mesma compartilhada por todos os servicos. O Gateway valida o token localmente — nenhuma chamada a Auth API por request.

---

## Documentacao da API (Scalar)

A documentacao interativa e servida pelo **Scalar** (substitui Swagger/Swashbuckle).

Disponivel apenas no ambiente de desenvolvimento:

```
http://localhost:<porta>/scalar/v1
```

O schema OpenAPI bruto fica em `/openapi/v1.json`.

---

## Executando localmente

```bash
cd Finance_Project.ApiGateway
dotnet run
```

> **Importante:** Os servicos downstream (Cards API, Transactions API, Auth API) devem estar rodando para que o Gateway funcione corretamente.

### Portas

| Servico | HTTPS | HTTP |
|---------|-------|------|
| API Gateway | `65000` | `65001` |
| Cards API | `65006` | `65007` |
| Transactions API | `65010` | `65011` |
| Auth API | `65002` | `65003` |

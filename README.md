# BankMore - Sistema Bancário Digital

Sistema bancário digital baseado em microsserviços desenvolvido com .NET 8, seguindo os princípios de **Domain-Driven Design (DDD)** e **CQRS**.

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar](#como-executar)
- [APIs Disponíveis](#apis-disponíveis)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)

## 🏦 Sobre o Projeto

BankMore é uma plataforma bancária digital que oferece:

- ✅ Cadastro e autenticação de usuários
- ✅ Movimentações na conta corrente (depósitos e saques)
- ✅ Transferências entre contas da mesma instituição
- ✅ Consulta de saldo e extrato
- ✅ Autenticação JWT
- ✅ Idempotência de requisições
- ✅ Validações de negócio

## 🏗️ Arquitetura

O sistema é composto por dois microsserviços:

### API Conta Corrente

Responsável por:

- Cadastro de contas
- Autenticação (JWT)
- Movimentações (crédito e débito)
- Consulta de saldo
- Consulta de extrato
- Inativação de contas

### API Transferência

Responsável por:

- Transferências entre contas
- Validação de contas e saldos
- Estorno automático em caso de falha
- Registro de histórico de transferências

**Padrões Utilizados:**

- Domain-Driven Design (DDD)
- CQRS (Command Query Responsibility Segregation)
- Repository Pattern
- MediatR para mediação de comandos e queries

## 🚀 Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **SQLite** - Banco de dados
- **Dapper** - Micro ORM
- **MediatR** - Mediação de comandos/queries
- **FluentValidation** - Validação de dados
- **JWT** - Autenticação
- **xUnit** - Framework de testes
- **FluentAssertions** - Assertions para testes
- **Moq** - Mocks para testes
- **Swagger/OpenAPI** - Documentação das APIs
- **Docker** - Containerização

## 📦 Pré-requisitos

### Opção 1: Docker (Recomendado)

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução

### Opção 2: Execução Local

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado
- Editor de código (Visual Studio, VS Code ou Rider)

## 🎯 Como Executar

### Usando Docker (Recomendado)

1. **Clone o repositório**

```bash
git clone <url-do-repositorio>
```

2. **Inicie o Docker Desktop**

3. **Execute os containers**

```bash
docker-compose up -d
```

4. **Verifique se os containers estão rodando**

```bash
docker ps
```

5. **Acesse as APIs**

- API Conta Corrente: http://localhost:5001/swagger
- API Transferência: http://localhost:5002/swagger

### Executando Localmente (sem Docker)

1. **Crie o diretório de bancos de dados**

```powershell
New-Item -ItemType Directory -Path "databases" -Force
```

2. **Execute a API de Conta Corrente**

```bash
cd src/BankMore.ContaCorrente.API
dotnet run
```

3. **Em outro terminal, execute a API de Transferência**

```bash
cd src/BankMore.Transferencia.API
dotnet run
```

> **Nota:** Ao executar localmente, a API de Conta Corrente estará em `http://localhost:5001` e a API de Transferência em `http://localhost:5002`.

## 📡 APIs Disponíveis

### API Conta Corrente (Porta 5001)

#### Endpoints Públicos

- `POST /api/contacorrente` - Criar conta
- `POST /api/auth/login` - Login

#### Endpoints Autenticados (requer JWT)

- `GET /api/contacorrente/{id}/saldo` - Consultar saldo
- `GET /api/contacorrente/{id}/extrato` - Consultar extrato
- `POST /api/contacorrente/{id}/movimentacao` - Realizar movimentação
- `DELETE /api/contacorrente/{id}` - Inativar conta

### API Transferência (Porta 5002)

#### Endpoints Autenticados (requer JWT)

- `POST /api/transferencia` - Realizar transferência
- `GET /api/transferencia/{idRequisicao}` - Consultar transferência

## 🧪 Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar Testes de um Projeto Específico

```bash
# Testes da API Conta Corrente
cd tests/BankMore.ContaCorrente.Tests
dotnet test

# Testes da API Transferência
cd tests/BankMore.Transferencia.Tests
dotnet test
```

### Cobertura de Testes

O projeto conta com:

- Testes unitários de entidades e value objects
- Testes de command handlers
- Testes de validadores
- Testes de controllers
- Testes de autorização

## 📂 Estrutura do Projeto

```
BankMore/
├── src/
│   ├── BankMore.ContaCorrente.API/       # API de Conta Corrente
│   │   ├── Application/                  # Comandos, Queries e Validators
│   │   ├── Controllers/                  # Controllers da API
│   │   ├── Domain/                       # Entidades, Value Objects, Repositories
│   │   ├── Infrastructure/               # Implementações, Repositórios, Serviços
│   │   └── DTOs/                         # Data Transfer Objects
│   │
│   └── BankMore.Transferencia.API/       # API de Transferência
│       ├── Application/                  # Comandos e Handlers
│       ├── Controllers/                  # Controllers da API
│       ├── Domain/                       # Entidades, Enums, Repositories
│       ├── Infrastructure/               # Implementações, Repositórios, Serviços
│       └── DTOs/                         # Data Transfer Objects
│
├── tests/
│   ├── BankMore.ContaCorrente.Tests/     # Testes da API Conta Corrente
│   └── BankMore.Transferencia.Tests/     # Testes da API Transferência
│
├── databases/                             # Bancos de dados SQLite
├── docker-compose.yml                     # Configuração Docker
└── README.md                              # Este arquivo
```

## 🔐 Autenticação

O sistema utiliza JWT (JSON Web Token) para autenticação. Para acessar endpoints protegidos:

1. Crie uma conta: `POST /api/contacorrente`
2. Faça login: `POST /api/auth/login`
3. Copie o token retornado
4. No Swagger, clique em "Authorize" e insira: `Bearer {seu-token}`

## 🔄 Idempotência

Todas as operações de movimentação e transferência suportam idempotência através do campo `idRequisicao`. Se a mesma requisição for enviada novamente, o sistema retornará o resultado da operação anterior sem reprocessar.

## 📝 Exemplo de Uso

### 1. Criar uma conta

```json
POST /api/contacorrente
{
  "nome": "João Silva",
  "cpf": "12345678909",
  "senha": "senha123"
}
```

### 2. Fazer login

```json
POST /api/auth/login
{
  "cpfOuNumeroConta": "12345678909",
  "senha": "senha123"
}
```

### 3. Consultar saldo

```
GET /api/contacorrente/{id}/saldo
Authorization: Bearer {token}
```

### 4. Realizar transferência

```json
POST /api/transferencia
Authorization: Bearer {token}
{
  "idRequisicao": "unique-id-123",
  "idContaDestino": 2,
  "valor": 100.00
}
```

## 🛠️ Comandos Úteis Docker

```bash
# Parar containers
docker-compose down

# Reconstruir e iniciar
docker-compose up --build -d

# Ver logs
docker-compose logs -f

# Remover volumes (limpar dados)
docker-compose down -v

# Verificar status
docker ps
```

## 🐛 Troubleshooting

### Porta em uso

Se as portas 5001 ou 5002 estiverem em uso, edite o `docker-compose.yml` e altere as portas:

```yaml
ports:
  - "5003:8080" # Altere para uma porta disponível
```

### Banco de dados não inicializa

Os bancos de dados são criados automaticamente na primeira execução. Se houver problemas:

```bash
docker-compose down -v
docker-compose up --build
```

### Erro de conexão entre APIs

Certifique-se de que o Docker Desktop está rodando e que ambos os containers estão na mesma rede (`bankmore-network`).

## 📄 Licença

Este projeto foi desenvolvido como parte de um teste técnico.

## 👨‍💻 Desenvolvimento

- Arquitetura: DDD + CQRS
- Banco de dados: SQLite
- Autenticação: JWT
- Validações: FluentValidation
- Testes: xUnit + FluentAssertions + Moq
- Containerização: Docker

# BankMore - Dockerização

## Como executar com Docker

### Pré-requisitos

- Docker instalado
- Docker Compose instalado

### Executar a aplicação

```bash
# Construir e iniciar os containers
docker-compose up --build

# Ou em modo background
docker-compose up -d --build
```

### Acessar a API

- **API Conta Corrente**: http://localhost:5001
- **Swagger**: http://localhost:5001/swagger

### Parar os containers

```bash
docker-compose down
```

### Visualizar logs

```bash
# Todos os serviços
docker-compose logs -f

# Apenas API Conta Corrente
docker-compose logs -f contacorrente-api
```

### Remover volumes (limpar dados)

```bash
docker-compose down -v
```

## Estrutura de Volumes

- `contacorrente-data`: Armazena o banco de dados SQLite da API Conta Corrente

## Variáveis de Ambiente

Configuradas no `docker-compose.yml`:

- `JWT__Secret`: Chave secreta para geração de tokens JWT
- `JWT__Issuer`: Emissor do token
- `JWT__Audience`: Público-alvo do token
- `JWT__ExpirationMinutes`: Tempo de expiração do token em minutos
- `ConnectionStrings__DefaultConnection`: String de conexão com o banco de dados

## Health Check

A API possui um health check configurado que verifica se o Swagger está acessível a cada 30 segundos.

```bash
# Verificar status do container
docker ps
```

## Troubleshooting

### Erro de porta em uso

Se a porta 5001 já estiver em uso, edite o `docker-compose.yml` e altere a porta:

```yaml
ports:
  - "5002:8080" # Altere 5001 para outra porta
```

### Limpar tudo e recomeçar

```bash
docker-compose down -v
docker system prune -a
docker-compose up --build
```

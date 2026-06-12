# TicketsBooking API 🚍

API para gerenciamento de viagens rodoviárias e reservas de passagens, desenvolvida em ASP.NET Core 9 seguindo princípios de arquitetura em camadas, regras de negócio, testes automatizados e execução via Docker.

## Tecnologias Utilizadas

* .NET 9
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Redis
* RabbitMQ
* Docker & Docker Compose
* xUnit
* Moq
* Swagger / OpenAPI

---

## Arquitetura

```text
src/
├── TicketsBooking.API
├── TicketsBooking.Application
├── TicketsBooking.Core
└── TicketsBooking.Infrastructure

tests/
└── TicketsBooking.Application.Tests
```

### Responsabilidades

**TicketsBooking.API**

* Controllers
* Configuração da aplicação
* Swagger

**TicketsBooking.Application**

* Casos de uso
* Serviços de aplicação
* DTOs

**TicketsBooking.Core**

* Entidades
* Eventos
* Interfaces
* Regras de domínio

**TicketsBooking.Infrastructure**

* Entity Framework Core
* Banco de dados
* Cache Redis
* Mensageria RabbitMQ

---

## Funcionalidades Implementadas

### Rotas

```http
GET /rotas
```

Lista todas as rotas disponíveis.

### Viagens

```http
GET /viagens
GET /viagens/{id}
```

Permite consultar viagens e visualizar assentos disponíveis.

### Reservas

```http
POST /reservas
GET /reservas/{codigo}
DELETE /reservas/{codigo}
```

Permite criar, consultar e cancelar reservas.

---

## Regras de Negócio

### Assento Único

Não é permitido reservar um assento já ocupado.

### Validação de CPF

Todos os CPFs são validados utilizando cálculo dos dígitos verificadores.

### Viagem Expirada

Não é permitido realizar reservas para viagens já iniciadas.

### Código de Reserva

Cada reserva recebe um código único no formato:

```text
ABC-12345
```

### Cancelamento

O cancelamento somente é permitido até 2 horas antes da partida.

---

## Testes Automatizados

O projeto possui testes automatizados utilizando xUnit.

### Cenários Cobertos

* Validação de CPF válido
* Validação de CPF inválido
* Geração de código de reserva
* Garantia de unicidade do código de reserva
* Reserva de assento já ocupado
* Reserva para viagem já realizada
* Cancelamento fora do prazo permitido
* Cancelamento realizado com sucesso

### Executar Testes

```bash
dotnet test
```

Resultado atual:

```text
Total: 16
Sucesso: 16
Falhas: 0
```

---

## Executando com Docker

### Subir todo o ambiente

```bash
docker compose up --build
```

Serviços iniciados:

* SQL Server
* Redis
* RabbitMQ
* API

### Swagger

Após a inicialização:

```text
http://localhost:5000/swagger
```

---

## Seed de Dados

Para carregar dados de exemplo:

```http
POST /api/Admin/seed
```

---

## Exemplo de Reserva

### Criar Reserva

```json
{
  "tripId": "9b85f3b2-2222-5555-9999-111111111111",
  "seatNumber": "15A",
  "passengerName": "Gerson Aparecido Lima",
  "passengerDocument": "52998224725",
  "passengerEmail": "gersonaparecido@gmail.com",
  "price": 450.00
}
```

### Resposta

```json
{
  "success": true,
  "message": "Reserva criada com sucesso.",
  "reservationCode": "ABC-12345"
}
```

---

## Diferenciais Técnicos

* Arquitetura em camadas
* Separação clara de responsabilidades
* Integração com Redis para controle de concorrência
* Integração com RabbitMQ para processamento assíncrono
* Testes automatizados
* Docker Compose para execução completa do ambiente
* Swagger/OpenAPI para documentação da API

---

## Autor

**Gerson Aparecido Lima**

Desenvolvedor Backend .NET

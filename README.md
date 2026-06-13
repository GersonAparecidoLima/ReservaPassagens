# TicketsBooking API 🚍

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Tests](https://img.shields.io/badge/Tests-16%20Passing-success)
![License](https://img.shields.io/badge/License-MIT-green)

## Links Rápidos

- Swagger: <http://localhost:5000/swagger>
- RabbitMQ Management: <http://localhost:15672>

Credenciais RabbitMQ:

- Usuário: guest
- Senha: guest

Sistema de gerenciamento de viagens rodoviárias e reservas de passagens desenvolvido com **ASP.NET Core 9**, aplicando boas práticas de arquitetura de software, regras de negócio, mensageria assíncrona, cache distribuído e testes automatizados.

O projeto foi construído com foco em **escalabilidade, manutenibilidade e qualidade de código**, seguindo uma arquitetura em camadas e utilizando tecnologias amplamente adotadas no mercado.

## Visão Geral

O TicketsBooking é uma API REST para gerenciamento de viagens rodoviárias e reservas de passagens, desenvolvida em ASP.NET Core 9.

A solução foi construída utilizando arquitetura em camadas, Entity Framework Core, SQL Server, Redis e RabbitMQ, aplicando princípios SOLID, controle de concorrência e testes automatizados para garantir consistência e escalabilidade.

O projeto simula um cenário real de reservas de passagens, contemplando regras de negócio, persistência de dados, mensageria assíncrona e execução completa via Docker Compose.


---

# 🚀 Tecnologias Utilizadas

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

# 🏗️ Arquitetura da Solução

```text
src/
├── TicketsBooking.API
├── TicketsBooking.Application
├── TicketsBooking.Core
└── TicketsBooking.Infrastructure

tests/
└── TicketsBooking.Application.Tests
```

```text
┌─────────────┐
│   Swagger   │
└──────┬──────┘
       │
       ▼
┌─────────────────────┐
│ TicketsBooking.API  │
└─────────┬───────────┘
          │
          ▼
┌───────────────────────────┐
│ TicketsBooking.Application│
└─────────┬─────────────────┘
          │
          ▼
┌─────────────────────┐
│ TicketsBooking.Core │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐
│ Infrastructure      │
├─────────────────────┤
│ SQL Server          │
│ Redis               │
│ RabbitMQ            │
└─────────────────────┘
```


## Camadas da Aplicação

### TicketsBooking.API

Responsável pela exposição da API REST.

* Controllers
* Configuração da aplicação
* Swagger/OpenAPI
* Injeção de Dependências

### TicketsBooking.Application

Responsável pelos casos de uso da aplicação.

* Serviços de aplicação
* DTOs
* Orquestração das regras de negócio

### TicketsBooking.Core

Camada de domínio.

* Entidades
* Interfaces
* Eventos
* Regras de negócio
* Objetos de valor

### TicketsBooking.Infrastructure

Camada de infraestrutura.

* Entity Framework Core
* Persistência de dados
* Redis
* RabbitMQ
* Implementações de serviços externos

---

# 📋 Funcionalidades Implementadas

## Rotas

```http
GET /rotas
```

Retorna todas as rotas cadastradas no sistema.

### Exemplo

```json
[
  {
    "id": "aaaaaaaa-1111-2222-3333-bbbbbbbbbbbb",
    "nome": "São Paulo x Rio de Janeiro",
    "cidadeOrigem": "São Paulo",
    "cidadeDestino": "Rio de Janeiro"
  }
]
```

---

## Viagens

```http
GET /viagens
GET /viagens/{id}
```

Permite consultar viagens disponíveis e visualizar detalhes da viagem, incluindo assentos livres e ocupados.

---

## Reservas

```http
POST /reservas
GET /reservas/{codigo}
DELETE /reservas/{codigo}
```

Permite criar, consultar e cancelar reservas.

---

# 📌 Regras de Negócio

### Assento Único

Não é permitido reservar um assento já ocupado.

### Validação de CPF

Todos os CPFs são validados através do cálculo dos dígitos verificadores.
O CPF do passageiro é informado através do campo `passengerDocument` durante a criação da reserva.

### Viagem Encerrada

Não é permitido criar reservas para viagens cuja partida já ocorreu.

### Código de Reserva

Cada reserva recebe um identificador único e legível.

Exemplo:

```text
ABC-12345
```

### Cancelamento

Reservas só podem ser canceladas até **2 horas antes da partida**.

---

# 🧪 Testes Automatizados

O projeto possui cobertura de testes automatizados utilizando **xUnit**.

## Cenários Cobertos

### Validação de CPF

* CPF válido
* CPF inválido
* CPF com dígitos verificadores incorretos

### Regras de Reserva

* Assento já ocupado
* Viagem já realizada
* Cancelamento fora do prazo permitido
* Cancelamento realizado com sucesso

### Código de Reserva

* Geração de código
* Garantia de unicidade

## Executar Testes

```bash
dotnet test
```

Resultado atual:

```text
✓ Total de testes: 16
✓ Testes aprovados: 16
✓ Falhas: 0
```

---

# 🐳 Executando com Docker

Subir toda a infraestrutura e aplicação:

```bash
docker compose up --build
```


## Swagger

Após a inicialização:

```text
http://localhost:5000/swagger
```

# 💻 Executando Localmente

Restaurar dependências:

```bash
dotnet restore
```

Executar a aplicação:

```bash
dotnet ef database update --project src/TicketsBooking.Infrastructure --startup-project src/TicketsBooking.API
dotnet run --project src/TicketsBooking.API
```

Acessar o Swagger:

```text
http://localhost:5000/swagger
```


---

# 🚀 Primeiros Passos


Após subir os containers pela primeira vez, o banco de dados estará vazio.

Para carregar os dados de demonstração e iniciar os testes da aplicação, execute o endpoint:

```http
POST /api/Admin/seed
```

Esse endpoint criará automaticamente:

* 1 rota de exemplo
* 1 viagem de exemplo
* Assentos disponíveis para reserva

Após a execução do seed, os seguintes endpoints estarão prontos para teste:

```http
GET /rotas
GET /viagens
GET /viagens/{id}
POST /reservas
GET /reservas/{codigo}
DELETE /reservas/{codigo}
```

Caso o endpoint seja executado novamente, os dados não serão duplicados. O sistema identificará que os registros de demonstração já foram carregados.

---

# 📦 Exemplo de Reserva
## Requisição

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

### Campos da Requisição

| Campo             | Descrição               |
| ----------------- | ----------------------- |
| tripId            | Identificador da viagem |
| seatNumber        | Número do assento       |
| passengerName     | Nome do passageiro      |
| passengerDocument | CPF do passageiro       |
| passengerEmail    | E-mail do passageiro    |
| price             | Valor da passagem       |


## Resposta

```json
{
  "success": true,
  "message": "Reserva criada com sucesso.",
  "expiresAt": "<data de expiração>",
  "reservationCode": "ABC-12345"
}
```

# 🔒 Controle de Concorrência

O projeto utiliza Redis para implementar um mecanismo de lock distribuído durante o processo de reserva.

Esse controle impede que dois usuários reservem simultaneamente o mesmo assento, garantindo consistência dos dados e evitando conflitos em cenários concorrentes.

---

# 📨 Mensageria Assíncrona

A aplicação utiliza RabbitMQ para publicação e processamento de eventos de negócio.

Quando uma reserva é criada, um evento `BookingCreatedEvent` é publicado e consumido pelo `BookingCreatedConsumer`, permitindo desacoplamento entre os componentes da aplicação e facilitando futuras integrações.


---

# ⭐ Diferenciais Técnicos

* Arquitetura em camadas
* Princípios SOLID
* Entity Framework Core
* Cache distribuído com Redis
* Mensageria assíncrona com RabbitMQ
* Docker Compose para ambiente completo
* Testes automatizados com xUnit
* Swagger/OpenAPI
* Controle de concorrência para reservas
* Validações de domínio

---

# ✅ Status do Projeto

Funcionalidades implementadas:

* Cadastro e consulta de rotas
* Consulta de viagens
* Reserva de assentos
* Cancelamento de reservas
* Validação de CPF
* Controle de concorrência com Redis
* Mensageria assíncrona com RabbitMQ
* Testes automatizados
* Docker Compose

Projeto pronto para execução e avaliação técnica.

---

Projeto desenvolvido para fins de estudo, demonstração técnica e avaliação de competências em desenvolvimento Backend com .NET.

# 👨‍💻 Autor

**Gerson Aparecido Lima**

Desenvolvedor Backend .NET | C# | ASP.NET Core | SQL Server | Docker

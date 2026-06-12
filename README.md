# TicketsBooking API 🚍

Sistema de gerenciamento de viagens rodoviárias e reservas de passagens desenvolvido com **ASP.NET Core 9**, aplicando boas práticas de arquitetura de software, regras de negócio, mensageria assíncrona, cache distribuído e testes automatizados.

O projeto foi construído com foco em **escalabilidade, manutenibilidade e qualidade de código**, seguindo uma arquitetura em camadas e utilizando tecnologias amplamente adotadas no mercado.

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
Total: 16
Sucesso: 16
Falhas: 0
```

---

# 🐳 Executando com Docker

Subir toda a infraestrutura e aplicação:

```bash
docker compose up --build
```

Serviços disponibilizados:

* SQL Server
* Redis
* RabbitMQ
* API ASP.NET Core

---

## Swagger

Após a inicialização:

```text
http://localhost:5000/swagger
```

---

# 🌱 Seed de Dados

Para popular o ambiente com dados de exemplo:

```http
POST /api/Admin/seed
```

O endpoint cria:

* Rotas
* Viagens
* Assentos

Permitindo testar rapidamente todas as funcionalidades da API.

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

## Resposta

```json
{
  "success": true,
  "message": "Reserva criada com sucesso.",
  "expiresAt": "2026-06-12T13:08:42.101304Z",
  "reservationCode": "ABC-12345"
}
```

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

# 👨‍💻 Autor

**Gerson Aparecido Lima**

Desenvolvedor Backend .NET | C# | ASP.NET Core | SQL Server | Docker

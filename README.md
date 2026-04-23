# 💈 BarberShop API & Web

Projeto desenvolvido com **ASP.NET Core + Blazor** com o objetivo de simular um sistema completo de barbearia, incluindo cadastro de clientes, cortes e agendamentos.

---

## 📌 Objetivo

Criar uma aplicação web para gerenciamento de uma barbearia, aplicando conceitos como:

* Endpoints (Minimal API)
* Entity Framework Core
* Relacionamentos entre entidades
* Injeção de Dependência
* Arquitetura em camadas (API, Core, Web)
* Consumo de API com Blazor
* Controle de regras de negócio (ex: apenas admin cria cortes)

---

## 🛠️ Tecnologias Utilizadas

* ASP.NET Core
* C#
* Entity Framework Core
* SQL Server
* Blazor (MudBlazor)
* Swagger
* Git e GitHub

---

## 📂 Estrutura do Projeto

### 🔹 BarberShop.Api

* Endpoints
* Handlers
* Data (DbContext)
* Configurações
* Regras de negócio

### 🔹 BarberShop.Core

* Models (Cliente, Corte, Agendamento)
* Enums (Status do Agendamento)
* Requests / Responses
* Handlers compartilhados

### 🔹 BarberShop.Web

* Interface com Blazor
* Layouts (MudBlazor)
* Páginas (Login, Registro, Home)
* Integração com API

---

## ⚙️ Funcionalidades

* ✅ Cadastro de clientes
* ✅ Cadastro de cortes (**somente admin**)
* ✅ Agendamento de horários
* ✅ Cálculo automático de preço e duração
* ✅ Tema claro/escuro (Dark Mode)
* ✅ Interface moderna com MudBlazor

---

## ▶️ Como executar o projeto

```bash
# Restaurar pacotes
dotnet restore

# Executar API
dotnet run --project BarberShop.Api

# Executar Web
dotnet run --project BarberShop.Web
```

---

## 🔐 Regras de Negócio

* Apenas usuários com perfil **Admin** podem criar cortes
* O cliente escolhe a data do agendamento
* O sistema define automaticamente:

  * Valor do corte
  * Tempo de duração
  * Status inicial (Pendente)

---

## 📸 Interface

Interface construída com foco em um design moderno, utilizando paleta **preto + amarelo (estilo barbearia premium)**.

---

## 🚀 Melhorias Futuras

* Autenticação com JWT
* Controle de usuários e permissões
* Dashboard administrativo
* Histórico de agendamentos
* Integração com pagamentos

---

## 👨‍💻 Autor

Desenvolvido por Maycon Douglas Sousa Leite
Projeto para estudo e evolução como desenvolvedor .NET

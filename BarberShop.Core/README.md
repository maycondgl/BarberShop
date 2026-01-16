💈 BarberShop System - Fullstack .NET
Projeto de um sistema de gestão para barbearia desenvolvido como parte do meu aprendizado no curso Fullstack .NET do Balta.io. O sistema permite o cadastro de serviços, clientes, agendamentos e avaliações via QR Code.

🚀 Tecnologias
C# / .NET 8

ASP.NET Core Web API

Entity Framework Core

SQLite (para desenvolvimento local)

Padrão de Arquitetura: Clean Architecture (simplificada)

📁 Estrutura do Projeto
O projeto está dividido em bibliotecas de classes (ClassLibs) para melhor organização:

BarberShop.Core: Contém as entidades de negócio (Models) como Corte, Cliente, Agendamento e Avaliacao.

BarberShop.Infra: Camada de persistência e acesso ao banco de dados.

BarberShop.Api: Porta de entrada da aplicação e endpoints.

🛠️ Funcionalidades em Desenvolvimento
[x] Criação da estrutura de pastas e projetos.

[x] Modelagem das entidades principais (Corte, Cliente, Agendamento, Avaliacao).

[ ] Configuração do DataContext e Migrations.

[ ] CRUD de serviços (Cortes).

[ ] Sistema de avaliação para clientes (via QR Code).

[ ] Dashboard de agendamentos para o barbeiro.

📝 Diário de Bordo (Logs de Evolução)
Dia 1 (16/01/2026): Iniciei a estrutura da solução seguindo o exemplo do projeto Dima do curso do Balta. Criei o projeto .Core e as classes dentro da pasta Models.

Próximo passo: Configurar o BarberShopContext na camada de Infra e rodar o primeiro Migration.
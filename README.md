🪒 BarberShop - Sistema de Agendamento
Este projeto é uma aplicação de gestão para barbearias desenvolvida com o ecossistema .NET, focando em boas práticas de arquitetura e design de software. O projeto é fruto dos meus estudos práticos no curso Fullstack .NET do Balta.io.

🎯 Objetivo
Construir uma solução robusta para o gerenciamento de agendamentos, serviços e clientes, aplicando conceitos de Domain-Driven Design (DDD) e Always Valid Domain Models.

🛠️ Tecnologias e Conceitos
C# / .NET 8/9: Linguagem e framework principal.

DDD (Domain-Driven Design): Organização em camadas para separação de responsabilidades.

OOP (Programação Orientada a Objetos): Modelagem de entidades ricas.

Git: Versionamento semântico de código.

🏗️ Estrutura Atual (Camada Core)
Atualmente, o projeto foca na definição das Entidades de Domínio dentro do projeto BarberShop.Core:

Agendamento: Entidade que orquestra o horário, o cliente e o serviço solicitado.

Cliente: Representação do usuário e seus dados de contato.

Corte: Catálogo de serviços com descrição e valores.

Avaliação: Lógica para feedback após o serviço concluído.

🚀 Próximos Passos
[ ] Implementar Validações (Contracts/Notifications).

[ ] Configurar Contexto de Dados com Entity Framework Core.

[ ] Criar Repositórios e Unidade de Trabalho (Unit of Work).

[ ] Desenvolver a API (Controllers e Endpoints).

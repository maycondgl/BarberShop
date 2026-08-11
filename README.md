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

## ▶️ Executar localmente

### Pré-requisitos

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* Git

### 1. Preparar o projeto e o SQL Server

```powershell
git clone https://github.com/maycondgl/BarberShop.git
cd BarberShop
dotnet restore
dotnet build BarberShop.slnx
$secureDbPassword = Read-Host "Senha local do SQL Server" -AsSecureString
$env:BARBERSHOP_DB_PASSWORD = [System.Net.NetworkCredential]::new("", $secureDbPassword).Password
docker compose up -d
```

O `compose.yaml` cria o SQL Server local na porta `1433` e mantém os dados em um volume do Docker. A variável `BARBERSHOP_DB_PASSWORD` é obrigatória e permanece apenas na sessão atual do PowerShell.

### 2. Configurar os segredos locais

Os valores abaixo ficam no armazenamento de user-secrets da sua máquina e não são enviados ao GitHub:

```powershell
dotnet user-secrets set "ConnectionStrings:Connection" "Server=localhost,1433;Database=BarberShop;User Id=sa;Password=$env:BARBERSHOP_DB_PASSWORD;TrustServerCertificate=True;Encrypt=False" --project BarberShop.Api
$secureAdminKey = Read-Host "Chave inicial do administrador" -AsSecureString
$adminSetupKey = [System.Net.NetworkCredential]::new("", $secureAdminKey).Password
dotnet user-secrets set "AdminSetupKey" $adminSetupKey --project BarberShop.Api
Remove-Item Env:BARBERSHOP_DB_PASSWORD
Remove-Variable secureDbPassword, secureAdminKey, adminSetupKey
```

As notificações Web Push são opcionais. Se quiser testá-las, configure também:

```bash
dotnet user-secrets set "Secrets:VapidSubject" "mailto:seu-email@exemplo.com" --project BarberShop.Api
dotnet user-secrets set "Secrets:VapidPublicKey" "SUA_CHAVE_PUBLICA" --project BarberShop.Api
dotnet user-secrets set "Secrets:VapidPrivateKey" "SUA_CHAVE_PRIVADA" --project BarberShop.Api
```

### 3. Criar o banco de dados

```bash
dotnet tool install --global dotnet-ef --version 8.0.17
dotnet ef database update --project BarberShop.Api --startup-project BarberShop.Api -- --environment Development
```

Se o `dotnet-ef` já estiver instalado, atualize-o com `dotnet tool update --global dotnet-ef --version 8.0.17`.

### 4. Confiar no certificado local

```bash
dotnet dev-certs https --trust
```

### 5. Iniciar a aplicação

Abra dois terminais na pasta do projeto.

Terminal da API:

```bash
dotnet run --project BarberShop.Api --launch-profile https
```

Terminal do frontend:

```bash
dotnet run --project BarberShop.Web --launch-profile https
```

Acesse:

* Frontend: `https://localhost:7109`
* Swagger: `https://localhost:7197/swagger`
* Health check: `https://localhost:7197/health`

O ambiente de desenvolvimento já usa `https://localhost:7197` como backend e libera as portas locais corretas no CORS. Use HTTPS para que o cookie de autenticação funcione.

No primeiro cadastro, informe no campo **Chave de administrador** o mesmo valor configurado em `AdminSetupKey`. Somente o primeiro usuário com a chave correta é promovido automaticamente a administrador.

Para parar apenas o banco, preservando os dados:

```bash
docker compose stop
```

## 🚀 Publicar em produção no Azure

Arquitetura sugerida:

* API: Azure App Service
* Frontend: Azure Static Web Apps
* Banco: Azure SQL Database

### 1. Configurar o endereço público do frontend

Em `BarberShop.Web/wwwroot/appsettings.json`, defina a URL pública da API:

```json
{
  "BackendUrl": "https://sua-api.azurewebsites.net"
}
```

### 2. Publicar os projetos

```bash
dotnet publish BarberShop.Api -c Release -o ./publish/api
dotnet publish BarberShop.Web -c Release -o ./publish/web
```

Envie `publish/api` ao App Service e o conteúdo de `publish/web/wwwroot` ao Static Web Apps.

### 3. Configurar a API no Azure

Cadastre estes valores em **App Service > Configuration > Application settings**:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Connection=<conexao-do-azure-sql>
BackendUrl=https://sua-api.azurewebsites.net
FrontendUrl=https://seu-frontend.azurestaticapps.net
AdminSetupKey=<chave-secreta-forte-e-unica>
Secrets__VapidSubject=mailto:seu-email@exemplo.com
Secrets__VapidPublicKey=<chave-publica-vapid>
Secrets__VapidPrivateKey=<chave-privada-vapid>
```

As três configurações VAPID são opcionais; sem elas, somente as notificações push ficam desativadas. O CORS usa automaticamente `FrontendUrl`, portanto não é necessário alterar código para cada domínio.

### 4. Atualizar o banco de produção

Faça backup antes de aplicar migrations em um banco com dados. Em um terminal PowerShell seguro, disponibilize temporariamente as configurações de produção e execute:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__Connection = "<CONEXAO_DO_AZURE_SQL>"
$env:BackendUrl = "https://sua-api.azurewebsites.net"
$env:FrontendUrl = "https://seu-frontend.azurestaticapps.net"

dotnet ef database update --project BarberShop.Api --startup-project BarberShop.Api

Remove-Item Env:ASPNETCORE_ENVIRONMENT, Env:ConnectionStrings__Connection, Env:BackendUrl, Env:FrontendUrl
```

### Segurança

Os arquivos versionados não contêm mais chaves administrativas ou chaves privadas. Como valores antigos continuam no histórico do Git, gere novas chaves antes de publicar e nunca reutilize os valores removidos. Segredos locais devem ficar em user-secrets e segredos de produção nas configurações protegidas do Azure.

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

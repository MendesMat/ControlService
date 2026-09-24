# Control Service: back-end

API em C# com .NET 10, organizada em Clean Architecture ([ADR-0005](../../docs/adr/0005-adopt-clean-architecture.md)) com Minimal APIs ([ADR-0003](../../docs/adr/0003-use-minimal-apis-grouped-by-feature.md)). As regras de negócio estão em [`docs/`](../../docs/README.md).

## Como rodar

Pré-requisitos: .NET SDK 10 e Docker em execução.

```bash
dotnet run --project src/ControlService.AppHost
```

O Aspire sobe o PostgreSQL, o Mailpit (caixa de e-mails de teste) e a API, e abre o painel com os endereços de cada um. A documentação interativa da API fica em `/scalar`.

```bash
dotnet test
```

## Projetos e o que cada um pode enxergar

```
Domain  ◄──  Application  ◄──  Infrastructure
                  ▲                  ▲
                  └──────  API  ─────┘  (Infrastructure só para registrar serviços no Program.cs)
```

| Projeto | O que vai aqui | Pode referenciar |
|---|---|---|
| `ControlService.Domain` | Entidades, objetos de valor e regras de negócio. Nada de EF Core ou ASP.NET. | Nada |
| `ControlService.Application` | Casos de uso (comandos e consultas), validação e as **interfaces** de que eles precisam (repositórios, e-mail, usuário atual). | Domain |
| `ControlService.Infrastructure` | As **implementações** dessas interfaces: EF Core, Identity, envio de e-mail. | Application, Domain |
| `ControlService.API` | Endpoints HTTP, autenticação, autorização e o `Program.cs`, que liga tudo. | Application, Infrastructure |
| `ControlService.AppHost` / `ServiceDefaults` | Ambiente local com Aspire e telemetria. | API |

Os testes em `tests/ControlService.ArchitectureTests` falham se alguma dessas regras for quebrada.

## Pastas por funcionalidade

Dentro de cada projeto, o código fica numa pasta com o nome da funcionalidade, e não do tipo técnico. Exemplo para Usuários:

```
Domain/Users/                 User.cs, Login.cs, Cpf.cs, UserStatus.cs
Application/Users/            CreateUser/ (comando, handler, validador), ListUsers/ …
Infrastructure/Persistence/   Configurations/UserConfiguration.cs
API/Users/                    UsersEndpoints.cs  →  api.MapUserEndpoints()
```

Pastas previstas para a primeira etapa: `Access` (níveis, chaves das telas, acesso efetivo), `Users`, `PermissionProfiles`, `Auth` e `Screens`.

## Configurações comuns

- `Directory.Build.props`: .NET 10, nullable, avisos tratados como erros e analisadores ([ADR-0026](../../docs/adr/0026-enforce-build-quality-settings.md)).
- `Directory.Packages.props`: **todas as versões de pacotes ficam aqui**. No `.csproj`, use `<PackageReference Include="Nome" />` sem versão.
- `tests/Directory.Build.props`: xUnit v3 e Shouldly para todos os projetos de teste.

## Ordem de implementação

1. Domínio puro (objetos de valor, perfis, usuário, acesso efetivo) com testes.
2. Persistência (EF Core, PostgreSQL, autoria, versão, seed dos registros do sistema).
3. Autenticação (Identity, JWT, token de renovação).
4. Autorização por tela.
5. Perfis de permissão.
6. Usuários e links de ativação e troca de senha.

As rotas previstas estão em [docs/05-integracao-com-o-front.md](../../docs/05-integracao-com-o-front.md#rotas-http).

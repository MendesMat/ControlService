# Control Service: back-end

API em C# com .NET 10, organizada em Clean Architecture ([ADR-0005](../../docs/adr/0005-adopt-clean-architecture.md)) com Minimal APIs ([ADR-0003](../../docs/adr/0003-use-minimal-apis-grouped-by-feature.md)). As regras de negócio estão em [`docs/`](../../docs/README.md).

## Como rodar

Pré-requisitos: .NET SDK 10 e Docker Desktop aberto.

```bash
dotnet run --project src/ControlService.AppHost --launch-profile http
```

O Aspire sobe o PostgreSQL, o Mailpit (caixa de e-mails de teste) e a API. O console mostra o link de login do **painel do Aspire** (`http://localhost:15150/login?t=...`), com os endereços, logs e situação de cada parte.

| O quê | Endereço |
|---|---|
| Painel do Aspire | `http://localhost:15150` (use o link com `?t=` que aparece no console) |
| API | `http://localhost:5283` |
| Documentação interativa da API (Scalar) | `http://localhost:5283/scalar` |
| Caixa de e-mails de teste (Mailpit) | Link do recurso `mailpit` no painel do Aspire |

O perfil `https` também funciona, mas precisa que o certificado de desenvolvimento seja confiável no seu Windows. Essa é uma configuração de segurança da máquina, então faça-a só se quiser, com `dotnet dev-certs https --trust`.

Para rodar os testes:

```bash
dotnet test --solution ControlService.slnx
```

## Docker: o que é cada coisa

Tudo o que o projeto cria no Docker começa com **`controlservice-`**, para ser fácil de achar no Docker Desktop.

| Nome | Tipo | Para que serve | Quem cria |
|---|---|---|---|
| `controlservice-postgres` | Container | Banco de dados PostgreSQL | Aspire, ao rodar o AppHost |
| `controlservice-mailpit` | Container | Caixa de e-mails de teste; nenhum e-mail chega a pessoas reais | Aspire, ao rodar o AppHost |
| `controlservice-postgres-data` | Volume | Os arquivos do banco. É aqui que os dados ficam guardados. | Aspire, ao rodar o AppHost |
| `controlservice-api:dev` | Imagem | A API empacotada para rodar em container (usada na publicação) | Você, com `docker build` (abaixo) |

**Por que os containers continuam ligados depois que paro o sistema?** Eles são *persistentes*: o Aspire reaproveita os mesmos containers no próximo início, o que deixa a subida mais rápida e evita conflito de nomes. Ao terminar o dia, pare os dois no Docker Desktop (botão ■). Da próxima vez, o Aspire os liga de novo sozinho.

**Container × volume.** O container é o "programa" do banco e pode ser apagado e recriado sem perder nada. O volume é o "HD" com os dados. **Apagar o volume `controlservice-postgres-data` apaga todos os dados do banco.** Faça isso só quando quiser começar do zero.

**A senha do banco** é gerada pelo Aspire na primeira execução e guardada nos *user secrets* do AppHost, fora do repositório.

### Gerar e testar a imagem da API

Rode na pasta `backend/ControlService`:

```bash
docker build -f src/ControlService.API/Dockerfile -t controlservice-api:dev .
```

```bash
docker run --rm --name controlservice-api -p 127.0.0.1:8080:8080 -e ASPNETCORE_ENVIRONMENT=Development controlservice-api:dev
```

Depois abra `http://localhost:8080/health`. Publique sempre as portas em `127.0.0.1` (como acima): assim o container só é acessível do seu computador, e o `localhost` funciona. Nesta máquina, publicar sem o `127.0.0.1` faz `localhost` travar, por causa do encaminhamento IPv6 do Docker Desktop.

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

# Decisões de arquitetura

Este documento explica como o back-end do Control Service é construído e por quê. Ele está em português porque também é o material de estudo do dono do projeto: a meta é conseguir explicar cada decisão com as próprias palavras.

Cada decisão responde às mesmas seis perguntas:

1. **O que é.**
2. **Que problema resolve.**
3. **O que acontece sem isso.**
4. **Quanto custa.**
5. **Quais são as alternativas mais simples.**
6. **Por que escolhemos assim.**

No fim de cada uma há dois atalhos: **onde ver no código** e **em uma frase**, que é a resposta curta para uma entrevista.

As regras de negócio não estão aqui: elas ficam em [product/](product/). Este documento trata só de *como* o sistema é construído.

## Bloco A — Estrutura e fluxo do código

### O caminho de uma requisição

Antes das decisões, o mapa. Este é o caminho real de "trocar a senha" (`POST /api/v1/auth/change-password`); todas as outras operações seguem o mesmo desenho.

```
Requisição
   │
   ▼
Endpoint (API)              lê o token, monta o command e chama o handler
   │
   ▼
Validação (Application)     confere o formato dos dados; se estiver errado, responde aqui mesmo
   │
   ▼
Handler (Application)       executa o caso de uso: busca, decide, salva
   │         │
   │         ▼
   │      Domínio           as regras de negócio (agregados e Value Objects)
   ▼
Repositório (Infrastructure)   fala com o banco, atrás de uma interface
   │
   ▼
Result ──► Endpoint ──► resposta 200, ou um JSON de erro padronizado
```

1. A requisição chega com o token, e o ASP.NET confere se ele é válido.
2. O endpoint `ChangePassword` lê do token o id de quem está logado, monta o `ChangePasswordCommand` e chama o handler.
3. Antes do handler roda o `ChangePasswordValidator`: senha com o tamanho mínimo, confirmação igual à senha. Se algo estiver errado, a resposta sai daqui, com a mensagem de cada campo, e o handler nem é executado.
4. O `ChangePasswordHandler` busca o usuário, confere as regras que dependem do banco, chama o domínio, salva e abre uma sessão nova.
5. O handler devolve um `Result`. O endpoint o traduz: sucesso vira `200`; falha vira um JSON de erro com o status certo.

Cada peça desse caminho é uma das decisões abaixo.

### 1. .NET 10 e C# 14

1. **O que é.** Todos os projetos usam o .NET 10. A versão do SDK fica fixada no `global.json`, para que a máquina de desenvolvimento e o CI compilem com a mesma versão.
2. **Que problema resolve.** As versões pares do .NET têm suporte longo, de três anos. O .NET 10 saiu em novembro de 2025 e tem suporte até novembro de 2028. O .NET 8 e o .NET 9 perdem o suporte em 10 de novembro de 2026.
3. **O que acontece sem isso.** Um projeto começado no .NET 8 ficaria sem suporte em pouco tempo e teria de migrar no meio do desenvolvimento.
4. **Quanto custa.** Alguns pacotes demoram a suportar a versão nova. Antes de adicionar um pacote, é preciso conferir se ele funciona com `net10.0`.
5. **Alternativas mais simples.** O .NET 8, que ainda é o mais comum em sistemas existentes.
6. **Por que escolhemos assim.** Não há ganho em começar numa versão que está acabando.

- **Onde ver no código:** [global.json](../backend/ControlService/global.json), [Directory.Build.props](../backend/ControlService/Directory.Build.props).
- **Em uma frase:** escolhi a versão de suporte longo mais recente, para não ter de migrar durante a vida do projeto.

### 2. Quatro projetos em camadas, com pastas por funcionalidade

1. **O que é.** O código de produção fica em quatro projetos, e cada um só pode referenciar os que estão "para dentro":

   ```
   API ──────► Application ──────► Domain
                    ▲
   Infrastructure ──┘
   ```

   | Projeto | Contém | Pode referenciar |
   |---|---|---|
   | `Domain` | Agregados, Value Objects, regras de negócio | Nada: nem pacotes, nem EF Core, nem ASP.NET Core |
   | `Application` | Casos de uso (handlers), validadores e as interfaces de que eles precisam | `Domain` |
   | `Infrastructure` | EF Core, repositórios, Identity | `Application`, `Domain` |
   | `API` | Endpoints, autenticação, montagem do sistema | `Application`; `Infrastructure` só para registrar os serviços |

   Dentro de cada projeto, as pastas são por funcionalidade (`Users`, `Auth`, `PermissionProfiles`), não por tipo técnico.
2. **Que problema resolve.** As regras de negócio não dependem de banco nem de web. Por isso os testes de domínio rodam em cerca de um segundo, sem subir nada. E quem procura "onde fica o login" acha a pasta `Auth` em cada projeto.
3. **O que acontece sem isso.** Num projeto único com pastas, tudo funciona, mas nada impede uma classe de domínio de usar o EF Core. A separação passa a depender só de disciplina.
4. **Quanto custa.** Uma funcionalidade nova toca em vários projetos, e cruzar uma camada exige uma interface (decisão 5 mostra quais).
5. **Alternativas mais simples.** Um projeto só, com pastas. Ou um projeto por área do menu (Gerenciamento, Comercial, Financeiro), que foi a primeira tentativa: ele separa áreas de negócio, que dependem umas das outras o tempo todo, e não impede o domínio de usar o banco.
6. **Por que escolhemos assim.** É o desenho em camadas que o dono já conhece, é muito pedido em vagas .NET, e a regra de dependência é conferida por testes: se uma camada referenciar a errada, o build falha.

- **Onde ver no código:** [LayerDependencyTests.cs](../backend/ControlService/tests/ControlService.ArchitectureTests/LayerDependencyTests.cs).
- **Em uma frase:** as regras de negócio ficam num projeto que não conhece banco nem web, e um teste quebra o build se alguém violar isso.

### 3. Domínio com agregados e Value Objects

1. **O que é.** Três tipos de classe no projeto `Domain`:
   - **Agregado:** uma entidade que guarda os próprios dados e só os altera por métodos com nome de negócio. `User` e `PermissionProfile` são agregados. Escreve-se `user.Deactivate(by, now)`, nunca `user.Status = ...`; não existe `set` público. Um agregado referencia outro só pelo id.
   - **Value Object:** um tipo pequeno e imutável que só existe se for válido. `Cpf.Create("111.111.111-11")` devolve um erro, e não um CPF inválido. Os outros são `Login`, `EmailAddress`, `PhoneNumber`, `Cep`, `BloodType`, `ScreenKey` e `AccessLevel`.
   - **Serviço de domínio:** uma regra que envolve mais de um agregado. `EffectiveAccess` calcula o nível de uma pessoa a partir de todos os perfis dela.
2. **Que problema resolve.** Cada regra fica em um lugar só e não dá para contorná-la. "O Admin não pode ser desativado" está dentro de `User.Deactivate`, então vale para qualquer tela ou endpoint que chegue ali.
3. **O que acontece sem isso.** Com entidades só de `get` e `set`, a regra vai para cada service ou handler que mexe na entidade. Ela acaba repetida, e um dia alguém esquece uma das cópias.
4. **Quanto custa.** Mais classes, e o EF Core precisa de um conversor para cada Value Object.
5. **Alternativas mais simples.** Entidades anêmicas (só dados) com a validação nos handlers.
6. **Por que escolhemos assim.** As regras deste sistema são do negócio, não de uma tela, e ficam testáveis sem banco. **Regra de proporção:** um campo só vira Value Object quando tem regra de formato ou validação; um método de comportamento só existe onde há regra ou mudança de estado. Um cadastro simples, com nome e descrição, é uma entidade pequena.

- **Onde ver no código:** [User.cs](../backend/ControlService/src/ControlService.Domain/Users/User.cs), [Cpf.cs](../backend/ControlService/src/ControlService.Domain/Users/Cpf.cs), [EffectiveAccess.cs](../backend/ControlService/src/ControlService.Domain/Access/EffectiveAccess.cs), [AggregateTests.cs](../backend/ControlService/tests/ControlService.ArchitectureTests/AggregateTests.cs).
- **Em uma frase:** a entidade protege as próprias regras, e um valor inválido nem chega a ser criado.

### 4. Endpoints em Minimal APIs

1. **O que é.** Cada endpoint é um método registrado com `api.MapGet("/me", Me)`, numa classe por funcionalidade (`AuthEndpoints`). Não há classes Controller.

   ```csharp
   // Aqui
   api.MapGet("/me", Me);
   private static async Task<...> Me(ClaimsPrincipal principal, IQueryHandler<GetMeQuery, MeResponse> handler, ...)

   // O mesmo com um Controller
   [HttpGet("me")]
   public async Task<IActionResult> Me(...)   // o handler viria pelo construtor
   ```

2. **Que problema resolve.** Menos cerimônia: sem atributos nem classe base. Uma regra que vale para um grupo de rotas fica em um lugar só; é assim que a troca obrigatória da senha bloqueia tudo o que está em `/api/v1`.
3. **O que acontece sem isso.** Com Controllers, o sistema funcionaria igual. Não se perde funcionalidade.
4. **Quanto custa.** As dependências chegam como parâmetros do método, e não pelo construtor, o que estranha no começo. É preciso uma convenção (um `Map...Endpoints` por funcionalidade) para o `Program.cs` não crescer.
5. **Alternativas mais simples.** Controllers, o estilo mais comum em sistemas existentes.
6. **Por que escolhemos assim.** É o estilo padrão dos modelos atuais do ASP.NET Core, já está construído e testado, e a diferença para um Controller é pequena. Reavaliada em 2026-10-05: mantida.

- **Onde ver no código:** [AuthEndpoints.cs](../backend/ControlService/src/ControlService.API/Auth/AuthEndpoints.cs), [ApiV1Group.cs](../backend/ControlService/src/ControlService.API/Common/ApiV1Group.cs).
- **Em uma frase:** um endpoint aqui é um método de controller sem a classe em volta; sei fazer dos dois jeitos.

### 5. Um handler por caso de uso, sem MediatR

1. **O que é.** Cada operação do sistema é uma classe com um método só, `Handle`. É um Service de um método:

   | Num Service tradicional | Aqui |
   |---|---|
   | `UserService`, com um método por operação | Uma classe por operação: `SignInHandler`, `ChangePasswordHandler` |
   | O DTO de entrada | O *command* (altera algo) ou a *query* (só consulta) |

   Os handlers implementam `ICommandHandler<TCommand, TResponse>` ou `IQueryHandler<TQuery, TResponse>`. O endpoint recebe o handler por injeção de dependência e o chama direto.
2. **Que problema resolve.** Cada classe fica pequena e recebe só as dependências daquela operação. Separar consulta de alteração permite que uma consulta devolva os dados direto, sem passar pelas regras de escrita.
3. **O que acontece sem isso.** Um `UserService` com oito métodos e todas as dependências de todos eles.
4. **Quanto custa.** Mais arquivos (command, handler e validador por operação), e cada handler é registrado à mão.
5. **Alternativas mais simples.** Services com vários métodos. Ou a biblioteca MediatR, em que o endpoint chama `mediator.Send(command)` e ela encontra o handler: desde julho de 2025 ela tem edição comercial (a versão 12 continua aberta, mas sem atualizações).
6. **Por que escolhemos assim.** O formato é o mesmo do MediatR, então conhecer um ajuda a entender o outro, sem depender da biblioteca. E "ir para a implementação" a partir do endpoint leva direto ao handler.

**As interfaces do projeto são de dois tipos.** Vale saber separar:

| Interface | Definida em | Implementada em | Para quê |
|---|---|---|---|
| `IUserRepository`, `IPermissionProfileRepository`, `IUnitOfWork`, `ICredentialStore`, `ISessionStore` | `Application` | `Infrastructure` | **Inversão de dependência:** o handler precisa do banco, mas a `Application` não pode depender do EF Core |
| `IAccessTokenIssuer`, `ICurrentUser` | `Application` | `API` | **Inversão de dependência**, pelo mesmo motivo |
| `ICommandHandler<,>`, `IQueryHandler<,>` | `Application` | `Application` (os próprios handlers) | **Formato comum:** todo handler tem a mesma "cara", o que permite embrulhar qualquer um deles (decisão 6) |

As sete primeiras também são o que permite testar um handler com um repositório falso, em memória. As duas últimas não invertem dependência nenhuma: a API já pode enxergar a `Application`. Foram reavaliadas em 2026-10-05 e mantidas.

- **Onde ver no código:** [ICommandHandler.cs](../backend/ControlService/src/ControlService.Application/Common/ICommandHandler.cs), [SignInHandler.cs](../backend/ControlService/src/ControlService.Application/Auth/SignIn/SignInHandler.cs), e o registro em [AuthServiceCollectionExtensions.cs](../backend/ControlService/src/ControlService.API/Auth/AuthServiceCollectionExtensions.cs).
- **Em uma frase:** cada caso de uso é uma classe pequena; é o formato do MediatR, sem a biblioteca.

### 6. Validação com FluentValidation, aplicada por um decorator

1. **O que é.** Três peças:
   - **Validator:** uma classe que confere só o formato dos dados de um command, sem acessar o banco. Faz o papel de `[Required]` e `[MinLength]`, mas em uma classe separada, com as mensagens em português.
   - **Decorator:** uma classe que implementa a mesma interface de outra e a embrulha. `ValidatingCommandHandler` recebe o handler de verdade, roda o validador e só chama o handler se os dados estiverem certos. Quem usa não percebe a diferença.
   - **Um decorator só para todos os handlers.** Para validar um handler novo, escreve-se um *Validator* novo; o decorator é sempre o mesmo.

   A validação tem três lugares, e cada regra vai para um só:

   | Tipo de regra | Onde fica | Exemplo |
   |---|---|---|
   | Formato da entrada | Validator | Senha com pelo menos 8 caracteres; confirmação igual à senha |
   | Regra que precisa do banco | Handler | Já existe um usuário com esse login |
   | Regra de negócio | Domínio | Ninguém desativa a própria conta |

2. **Que problema resolve.** O back-end não pode confiar no navegador. Com o decorator, o handler só contém o caso de uso: quando ele roda, os dados já estão no formato certo. Os erros voltam por campo (`address.cep`), para a tela mostrar cada mensagem no lugar.
3. **O que acontece sem isso.** O handler chamaria o validador na primeira linha. Funciona, mas dá para esquecer.
4. **Quanto custa.** Dois conceitos a mais para explicar (interface genérica e decorator), e o registro de um handler validado é feito à mão, em quatro linhas.
5. **Alternativas mais simples.** Data Annotations, que são limitadas para regras como "se preenchido, deve ser válido". Ou chamar o validador dentro do handler, sem decorator.
6. **Por que escolhemos assim.** A validação é automática e o handler fica só com a regra de negócio. Reavaliada em 2026-10-05, com as alternativas "validar dentro do handler" e "handlers sem interface": o dono escolheu manter.

- **Onde ver no código:** [ValidatingCommandHandler.cs](../backend/ControlService/src/ControlService.Application/Common/ValidatingCommandHandler.cs), [ChangePasswordValidator.cs](../backend/ControlService/src/ControlService.Application/Auth/ChangePassword/ChangePasswordValidator.cs).
- **Em uma frase:** um "porteiro" confere os dados antes do handler; se estiverem errados, o handler nem roda.

### 7. Erros esperados como `Result`; erro HTTP em Problem Details

1. **O que é.** Uma falha prevista (login em uso, perfil ainda atribuído, senha errada) não é uma exceção: é um valor de retorno. Os handlers e os métodos do domínio devolvem `Result` ou `Result<T>`, que carregam o valor ou um `Error` com um `code` estável e uma mensagem em português.

   ```csharp
   return Result<SessionGrant>.Failure(AuthErrors.InvalidCredentials);
   ```

   Na API, `error.ToProblem()` transforma o erro em Problem Details, o formato padrão de erro HTTP (RFC 9457), usando uma tabela única de `code` → status.
2. **Que problema resolve.** A assinatura do método mostra que ele pode falhar, e o front-end recebe sempre o mesmo formato de erro, com um `code` para decidir o que fazer.
3. **O que acontece sem isso.** Com exceção para tudo, o caminho de erro fica escondido em blocos `catch`, mais difícil de acompanhar e de testar.
4. **Quanto custa.** Um `if (result.IsFailure)` depois de cada chamada, e uma linha na tabela para cada código novo. Um código sem linha na tabela é tratado como bug: vira o erro 500 genérico.
5. **Alternativas mais simples.** Lançar exceções e tratá-las num middleware. Ou uma biblioteca pronta de `Result`.
6. **Por que escolhemos assim.** O tipo `Result` é pequeno e feito em casa, sem dependência. Exceção fica só para o inesperado (banco fora do ar, bug), que responde 500 com uma mensagem genérica; o detalhe vai só para o log.

- **Onde ver no código:** [Result.cs](../backend/ControlService/src/ControlService.Domain/Common/Result.cs), [Error.cs](../backend/ControlService/src/ControlService.Domain/Common/Error.cs), [ErrorResults.cs](../backend/ControlService/src/ControlService.API/Common/ErrorResults.cs), [ErrorStatusCodes.cs](../backend/ControlService/src/ControlService.API/Common/ErrorStatusCodes.cs). Os códigos estão em [api/conventions.md](api/conventions.md#error-codes).
- **Em uma frase:** erro esperado é retorno, não exceção, e a API responde sempre no mesmo formato.

### 8. Mapeamento manual entre objetos

1. **O que é.** A conversão entre agregado, command e resposta é escrita à mão, em métodos pequenos ao lado de cada modelo (`SessionResponse.From(...)`).
2. **Que problema resolve.** O que entra e o que sai da API fica explícito. Acrescentar um campo obriga a decidir se ele deve ser exposto.
3. **O que acontece sem isso.** Uma biblioteca copiaria os campos de mesmo nome sozinha, e um erro de configuração só apareceria com o sistema rodando.
4. **Quanto custa.** Algumas linhas repetitivas por modelo.
5. **Alternativas mais simples.** Esta já é a mais simples. A tradicional é o AutoMapper, que desde julho de 2025 tem edição comercial.
6. **Por que escolhemos assim.** Menos uma dependência, e o compilador avisa quando algo não bate.

- **Onde ver no código:** [SessionResponse.cs](../backend/ControlService/src/ControlService.API/Auth/SessionResponse.cs).
- **Em uma frase:** prefiro algumas linhas explícitas a uma biblioteca que copia campos por conta própria.

### 9. Documentação da API com OpenAPI e Scalar

1. **O que é.** O documento OpenAPI (a descrição formal da API) é gerado a partir dos próprios endpoints, com o pacote da Microsoft. O Scalar mostra esse documento como uma página interativa em `/scalar`, no ambiente de desenvolvimento.
2. **Que problema resolve.** A documentação nunca fica diferente do código, e dá para testar cada rota pelo navegador.
3. **O que acontece sem isso.** A documentação seria escrita à mão e envelheceria.
4. **Quanto custa.** Quase nada: três linhas no `Program.cs`.
5. **Alternativas mais simples.** O Swashbuckle (Swagger), que deixou de vir nos modelos do ASP.NET Core a partir do .NET 9.
6. **Por que escolhemos assim.** É o caminho padrão atual. O plano de guardar um `openapi.json` no repositório e conferi-lo no CI foi descartado: a página `/scalar` já mostra o contrato.

- **Onde ver no código:** [Program.cs](../backend/ControlService/src/ControlService.API/Program.cs).
- **Em uma frase:** a documentação da API é gerada do código, então não envelhece.

## Bloco B — Dados

### 10. PostgreSQL

1. **O que é.** O banco de dados relacional do sistema, na versão 18. Em desenvolvimento e nos testes, ele roda num contêiner Docker.
2. **Que problema resolve.** Um ERP é feito de dados que dependem uns dos outros (cliente → serviço → cobrança). Um banco relacional garante essa ligação com chaves estrangeiras e transações.
3. **O que acontece sem isso.** Num banco de documentos, a integridade entre os cadastros ficaria por conta do código.
4. **Quanto custa.** O Docker precisa estar rodando, inclusive para os testes de integração.
5. **Alternativas mais simples.** O SQLite, que é um arquivo só, mas se comporta diferente em concorrência e em tipos de dado. Ou o SQL Server, o mais comum em vagas .NET, que tem licença paga em produção.
6. **Por que escolhemos assim.** É gratuito, e com o EF Core o código é quase o mesmo que seria para o SQL Server. Os testes de integração rodam contra o PostgreSQL de verdade, e não contra um substituto.

- **Onde ver no código:** [DependencyInjection.cs](../backend/ControlService/src/ControlService.Infrastructure/DependencyInjection.cs).
- **Em uma frase:** dados de ERP são relacionais, e escolhi um banco relacional gratuito que os testes usam de verdade.

### 11. EF Core com repositório e unidade de trabalho

1. **O que é.** O handler não conhece o `DbContext`. Ele pede o agregado a um repositório, altera e chama a unidade de trabalho para salvar. Os dois ficam atrás de interfaces:

   ```csharp
   // No handler (Application)
   var user = await users.GetByIdAsync(command.UserId, cancellationToken);
   user.CompleteFirstAccess(timeProvider.GetUtcNow());
   var saved = await unitOfWork.SaveChangesAsync(cancellationToken);

   // Na Infrastructure
   public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
   {
       public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
           dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
   }
   ```

   Três detalhes deste projeto:
   - Cada repositório tem só os métodos que algum handler usa. Não há repositório genérico.
   - Cada Value Object tem um conversor na configuração do EF Core: `HasConversion(login => login.Value, value => Login.Create(value).Value)`.
   - As tabelas são criadas por *migrations*, arquivos que descrevem cada mudança do banco. Elas são aplicadas sozinhas só em desenvolvimento.
2. **Que problema resolve.** A `Application` não pode depender do EF Core (decisão 2), e o `DbContext` é do EF Core. A interface resolve isso. De quebra, um handler pode ser testado com um repositório falso, que guarda os dados numa lista.
3. **O que acontece sem isso.** O handler receberia o `AppDbContext`. A `Application` passaria a depender do EF Core, e todo teste de handler precisaria de um banco.
4. **Quanto custa.** Uma interface e uma classe por agregado, e uma migration a cada mudança de modelo.
5. **Alternativas mais simples.** Usar o `DbContext` direto no handler, como muitos projetos atuais fazem. Ou escrever o SQL à mão com o Dapper.
6. **Por que escolhemos assim.** É o desenho que o dono já domina e é coerente com a regra de camadas. **O sistema sempre grava no PostgreSQL:** o repositório em memória existe só no projeto de testes, e é a exceção. A maioria das operações é testada pelo endpoint, contra o banco de verdade. Reavaliada em 2026-10-05: mantida.

- **Onde ver no código:** [IUserRepository.cs](../backend/ControlService/src/ControlService.Application/Users/IUserRepository.cs), [UserRepository.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/UserRepository.cs), [UnitOfWork.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/UnitOfWork.cs), [UserConfiguration.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/Configurations/UserConfiguration.cs), e o repositório falso em [InMemoryUserRepository.cs](../backend/ControlService/tests/ControlService.Application.Tests/Fakes/InMemoryUserRepository.cs).
- **Em uma frase:** o handler fala com o banco por uma interface, então a regra de negócio não depende do EF Core e pode ser testada sem banco.

### 12. Ids UUID v7, gerados no domínio

1. **O que é.** O id de cada registro é um `Guid` criado pelo próprio agregado, e não um número gerado pelo banco:

   ```csharp
   public static User Create(Login login, EmailAddress email, string displayName, string fullName) =>
       new(Guid.CreateVersion7(), isSystem: false, UserStatus.Pending, login, email, displayName, fullName);
   ```

   A versão 7 do UUID começa pela data e hora, então os ids saem em ordem crescente.
2. **Que problema resolve.** O objeto já nasce com id, antes de ser salvo. O id não revela quantos registros existem, e ninguém adivinha o próximo trocando `/users/42` por `/users/43`.
3. **O que acontece sem isso.** Com `int` auto-incremento, o id só existe depois do `INSERT`. Com um Guid aleatório (versão 4), cada registro novo cai num ponto qualquer do índice do banco, que fica fragmentado e mais lento.
4. **Quanto custa.** 16 bytes em vez de 4, e ids ruins de ler e de ditar.
5. **Alternativas mais simples.** `int` gerado pelo banco.
6. **Por que escolhemos assim.** `Guid.CreateVersion7()` já vem no .NET, sem pacote, e junta a vantagem do Guid (id antes de salvar) com a do número (ordem crescente).

- **Onde ver no código:** [User.cs](../backend/ControlService/src/ControlService.Domain/Users/User.cs), [SystemIds.cs](../backend/ControlService/src/ControlService.Domain/Common/SystemIds.cs).
- **Em uma frase:** o id é um Guid ordenado por tempo, criado junto com o objeto, que não entrega a contagem de registros.

### 13. Concorrência otimista

1. **O que é.** Cada registro tem uma versão. Quem edita devolve a versão que leu; se o registro mudou nesse meio-tempo, o salvamento é recusado. No PostgreSQL a versão é a coluna de sistema `xmin`, que o próprio banco troca a cada `UPDATE`. No código, basta uma linha por agregado:

   ```csharp
   builder.Property(user => user.Version).IsRowVersion();
   ```

   Quando as versões não batem, o EF Core lança `DbUpdateConcurrencyException`, e a unidade de trabalho a transforma num `Result` de falha com a mensagem "Este cadastro foi alterado por {nome} enquanto você editava. Recarregue para ver a versão atual." Na API, a versão vai no cabeçalho `ETag` e volta no `If-Match`.
2. **Que problema resolve.** Duas pessoas abrem o mesmo cadastro. Sem controle, a segunda a salvar apaga o trabalho da primeira sem perceber.
3. **O que acontece sem isso.** Vale o último que salvou, em silêncio.
4. **Quanto custa.** A tela precisa guardar a versão, reenviá-la e tratar o erro 409.
5. **Alternativas mais simples.** Não tratar. Ou travar o registro enquanto alguém edita (concorrência pessimista), que é pior: a trava fica presa se a pessoa fechar o navegador.
6. **Por que escolhemos assim.** É uma regra de negócio do sistema (CNV-12 e CNV-13), e "otimista" significa apostar que o conflito é raro: ninguém espera, e só o caso raro recebe um aviso.

- **Onde ver no código:** [UnitOfWork.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/UnitOfWork.cs), [AuditedAggregate.cs](../backend/ControlService/src/ControlService.Domain/Common/AuditedAggregate.cs). As regras estão em [product/conventions.md](product/conventions.md) e em [api/conventions.md](api/conventions.md).
- **Em uma frase:** cada registro tem uma versão; se alguém alterou enquanto eu editava, o sistema recusa e avisa quem foi.

### 14. Campos de auditoria preenchidos automaticamente

1. **O que é.** Todo agregado herda de `AuditedAggregate`, que tem `CreatedAt`, `CreatedBy`, `UpdatedAt` e `UpdatedBy`. Quem preenche é um *interceptor* do EF Core: uma classe que o EF chama logo antes de cada `SaveChanges`.

   ```csharp
   if (entry.State == EntityState.Added)
   {
       entry.Property(nameof(AuditedAggregate.CreatedAt)).CurrentValue = now;
       entry.Property(nameof(AuditedAggregate.CreatedBy)).CurrentValue = userId;
   }
   ```

2. **Que problema resolve.** Nenhum handler precisa lembrar de gravar quem alterou e quando. Salvar sem um usuário logado lança uma exceção, então nada é gravado sem autor.
3. **O que acontece sem isso.** Cada método do domínio receberia "quem" e "quando" e preencheria os campos. Um dia alguém esqueceria.
4. **Quanto custa.** É a peça que age "por fora": lendo o handler, não se vê os campos sendo preenchidos. E tem um trecho delicado: quando só muda uma lista interna do agregado (os perfis de um usuário, que ficam em outra tabela), o interceptor força o `UPDATE` da linha principal, para a versão da decisão 13 mudar também.
5. **Alternativas mais simples.** Preencher os campos dentro dos métodos do domínio, de forma explícita.
6. **Por que escolhemos assim.** A regra CNV-10 exige esses campos em todo cadastro, e o sistema terá muitos. O automático não depende de memória. Reavaliada em 2026-10-05: mantida.

- **Onde ver no código:** [AuditFieldsInterceptor.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/AuditFieldsInterceptor.cs), [AuditedAggregate.cs](../backend/ControlService/src/ControlService.Domain/Common/AuditedAggregate.cs).
- **Em uma frase:** "quem criou" e "quem alterou" são gravados num ponto só, antes de todo salvamento, e não em cada handler.

### 15. Desativar em vez de excluir

1. **O que é.** Um usuário nunca é apagado do banco. `user.Deactivate(by, now)` muda o status para inativo e grava quem desativou e quando; `user.Reactivate()` desfaz.
2. **Que problema resolve.** Os campos "criado por" e "alterado por" de outros registros apontam para usuários. Apagar um usuário quebraria esse histórico.
3. **O que acontece sem isso.** Ou o `DELETE` falha por causa da chave estrangeira, ou o histórico perde o autor.
4. **Quanto custa.** O login e as consultas precisam considerar o status.
5. **Alternativas mais simples.** O `DELETE` de verdade.
6. **Por que escolhemos assim.** É regra de negócio, e não preferência técnica. Vale para usuários; cada cadastro futuro define o seu caso no documento da funcionalidade.

- **Onde ver no código:** [User.cs](../backend/ControlService/src/ControlService.Domain/Users/User.cs).
- **Em uma frase:** quem já assinou alterações no sistema não pode sumir do banco, então a conta é desativada, e não apagada.

### 16. Registros do sistema: criados na inicialização e protegidos

1. **O que é.** O usuário Admin e o perfil Gerenciador têm ids fixos (`SystemIds`). O `SystemRecordsSeeder` os insere quando o banco é criado ou migrado, e só se estiverem faltando. O agregado os protege com a marca `IsSystem`:

   ```csharp
   private Result EnsureNotSystem() => IsSystem
       ? Result.Failure(new Error(
           "system_record",
           "O usuário Admin é do sistema e não pode ser alterado nem desativado."))
       : Result.Success();
   ```

2. **Que problema resolve.** Um banco vazio precisa de alguém que consiga entrar. E ninguém pode se trancar para fora do sistema desativando o Admin.
3. **O que acontece sem isso.** Um script manual depois de cada instalação, e a proteção dependeria de cada tela lembrar de conferir.
4. **Quanto custa.** O e-mail e a senha inicial do Admin vêm da configuração, e a aplicação não inicia sem eles.
5. **Alternativas mais simples.** Inserir os registros dentro da própria migration, o que deixaria a senha inicial no repositório.
6. **Por que escolhemos assim.** Reiniciar a aplicação nunca duplica nem altera esses registros, a senha fica fora do código, e a proteção está no domínio, onde vale para qualquer endpoint.

- **Onde ver no código:** [SystemRecordsSeeder.cs](../backend/ControlService/src/ControlService.Infrastructure/Persistence/SystemRecordsSeeder.cs), [SystemIds.cs](../backend/ControlService/src/ControlService.Domain/Common/SystemIds.cs), [User.cs](../backend/ControlService/src/ControlService.Domain/Users/User.cs).
- **Em uma frase:** o Admin e o perfil Gerenciador nascem com o banco e o próprio domínio impede que sejam alterados.

### 17. Listas paginadas no servidor

1. **O que é.** A tela pede só o que vai mostrar ("página 1, 10 por página, busca 'silva'"). O servidor filtra, ordena, conta e devolve `{ items, page, pageSize, totalCount }`. **Ainda não está construído:** a primeira lista chega com o cadastro de usuários. Por enquanto é a regra API-07 do contrato.
2. **Que problema resolve.** Não se envia ao navegador mil registros para mostrar dez.
3. **O que acontece sem isso.** A tela baixa tudo e filtra em JavaScript. Funciona com 50 registros e trava com 5.000.
4. **Quanto custa.** Cada lista recebe parâmetros de página, busca e ordenação, e faz duas consultas: os itens e o total.
5. **Alternativas mais simples.** Devolver a lista inteira.
6. **Por que escolhemos assim.** O front-end já foi desenhado com esse formato, e mudar o contrato de uma lista depois que a tela existe custa mais do que nascer paginada.

- **Onde ver no código:** ainda não há; a regra está em [api/conventions.md](api/conventions.md).
- **Em uma frase:** o servidor devolve só a página pedida e o total, e o navegador nunca recebe o que não vai mostrar.

## O que ficou de fora

Avaliado e não adotado. Fica registrado para ninguém propor de novo sem um motivo novo.

| Ideia | Por que não |
|---|---|
| Dapper para relatórios | O EF Core com `Select` resolve; só entra se um relatório provar que precisa |
| Guardar assinaturas em armazenamento de objetos (MinIO) | Uma coluna no banco basta, se e quando uma tela precisar da assinatura |
| Usar o `DbContext` direto nos handlers | A `Application` passaria a depender do EF Core, contra a regra de camadas (decisão 2) |
| Guardar um `openapi.json` no repositório e conferi-lo no CI | A página `/scalar` já mostra o contrato; seria um arquivo a mais para manter |
| MediatR e AutoMapper | Duas dependências, hoje com edição comercial, para algo que cabe em poucas linhas do próprio projeto |
| Controllers | Nenhum ganho de funcionalidade; exigiria reescrever o que já está testado |

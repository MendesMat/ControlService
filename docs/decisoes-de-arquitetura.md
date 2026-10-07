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

Este documento substitui os 33 ADRs (registros de decisão de arquitetura) que existiam em `docs/adr/`. Eles continuam no histórico do git.

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

**Registro dos handlers validados.** Um método auxiliar de uma linha para registrar o handler e o decorator juntos foi avaliado em 2026-10-07 e adiado, porque só existe um handler validado. Reavaliar no levantamento da primeira issue que registrar o segundo (#10 ou #11).

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

## Bloco C — Login e permissões

### O que já existe e o que falta

Este bloco descreve o desenho decidido. Parte dele ainda não está no código, e cada decisão diz qual.

| Peça | Estado |
|---|---|
| Login, sessão, renovação, sair, `GET /me` | Construído e testado |
| Bloqueio por tentativas e limite de requisições no login | Construído e testado |
| Cálculo do nível por tela e chaves de tela | Construído e testado |
| Troca obrigatória da senha | Construída, mas hoje só o Admin passa por ela; o desenho decidido é o contrário (decisão 21) |
| Autorização por tela e conferência da conta a cada requisição | **Não construída** (issue #9). Hoje qualquer pessoa logada chama qualquer endpoint |
| Rota com o catálogo de telas (`GET /screens`) | **Não construída** (issue #10) |

### 18. Senhas guardadas pelo ASP.NET Core Identity, separadas do usuário

1. **O que é.** O Identity é a biblioteca da Microsoft para contas. O projeto usa só o núcleo dela: gerar o *hash* da senha (o embaralhamento sem volta que se guarda no lugar da senha) e contar as tentativas erradas. A credencial fica numa tabela própria (`UserCredential`), com o mesmo id do `User`, e o handler fala com ela pela interface `ICredentialStore`:

   ```csharp
   if (await users.CheckPasswordAsync(credential, password))
   {
       await users.ResetAccessFailedCountAsync(credential);
       return CredentialCheck.Succeeded;
   }

   await users.AccessFailedAsync(credential);
   ```

2. **Que problema resolve.** Guardar senha com segurança é fácil de errar, e o Identity já faz certo. Com a credencial fora do cadastro, a senha nunca aparece numa consulta de usuários nem numa resposta da API.
3. **O que acontece sem isso.** O hash e o bloqueio seriam escritos à mão.
4. **Quanto custa.** A tabela vem com colunas que o sistema não usa (o e-mail e o telefone do Identity ficam vazios). E há uma classe que só existe por causa dos testes: `CredentialUserManager` faz o bloqueio usar o relógio da aplicação, que os testes controlam.
5. **Alternativas mais simples.** Uma coluna `PasswordHash` no próprio `User` e o contador de tentativas feito no projeto.
6. **Por que escolhemos assim.** Não se escreve criptografia de senha à mão. O `SignInManager` do Identity ficou de fora porque traz junto a autenticação por cookie, que o sistema não usa.

- **Onde ver no código:** [CredentialStore.cs](../backend/ControlService/src/ControlService.Infrastructure/Auth/CredentialStore.cs), [UserCredential.cs](../backend/ControlService/src/ControlService.Infrastructure/Auth/UserCredential.cs), [ICredentialStore.cs](../backend/ControlService/src/ControlService.Application/Auth/ICredentialStore.cs).
- **Em uma frase:** não escrevo criptografia de senha; uso a da Microsoft e guardo a credencial fora do cadastro.

### 19. Token de acesso JWT de 15 minutos

1. **O que é.** Depois do login, a API devolve um JWT: um texto assinado que diz quem é a pessoa. O navegador o envia em cada requisição, e o ASP.NET confere a assinatura sem ir ao banco. O token carrega três *claims* (campos): `sub`, o id do usuário; `sid`, o id da sessão; e `must_change_password`, só enquanto a troca de senha é obrigatória.

   ```csharp
   var claims = new Dictionary<string, object>
   {
       [JwtRegisteredClaimNames.Sub] = userId.ToString(),
       [SessionIdClaim] = sessionId.ToString(),
   };
   ```

   Não há dado pessoal nem permissão dentro do token.
2. **Que problema resolve.** A API precisa saber quem faz cada requisição sem pedir a senha de novo.
3. **O que acontece sem isso.** A alternativa clássica é o cookie de sessão do próprio ASP.NET.
4. **Quanto custa.** Um token emitido vale até vencer; não dá para cancelá-lo. Por isso ele dura pouco e precisa da decisão 20. A chave que assina os tokens é um segredo, fica fora do repositório, e a aplicação não inicia sem ela.
5. **Alternativas mais simples.** Autenticação só por cookie, que tem menos peças e serviria para este front-end.
6. **Por que escolhemos assim.** É o formato mais pedido em vagas e serve para um aplicativo de celular no futuro. As permissões ficam fora do token de propósito: se estivessem dentro, mudar um perfil só valeria quando o token vencesse.

- **Onde ver no código:** [JwtAccessTokenIssuer.cs](../backend/ControlService/src/ControlService.API/Auth/JwtAccessTokenIssuer.cs), [ConfigureJwtBearer.cs](../backend/ControlService/src/ControlService.API/Auth/ConfigureJwtBearer.cs).
- **Em uma frase:** o token diz só quem é a pessoa e dura 15 minutos; o que ela pode fazer eu leio do banco.

### 20. Sessão com refresh token no banco, trocado a cada uso

1. **O que é.** Junto do JWT vai um *refresh token*: 32 bytes aleatórios que servem só para pedir um JWT novo. No banco fica apenas o hash dele, na tabela `UserSession`. Ele viaja num cookie `HttpOnly` (que o JavaScript da página não consegue ler), enviado só para a rota de renovação. A cada uso ele é trocado por outro, que vale por mais 8 horas:

   ```csharp
   // Um UPDATE só: duas renovações com o mesmo token nunca passam as duas.
   var rotated = await db.Sessions
       .Where(session => session.TokenHash == oldHash && session.ExpiresAt > now)
       .ExecuteUpdateAsync(
           setters => setters
               .SetProperty(session => session.TokenHash, newHash)
               .SetProperty(session => session.ExpiresAt, expiresAt),
           cancellationToken);
   ```

2. **Que problema resolve.** Com o JWT curto, a pessoa teria de entrar de novo a cada 15 minutos. E é o refresh token que permite encerrar uma sessão: sair ou trocar a senha apaga a linha, e a renovação seguinte falha.
3. **O que acontece sem isso.** Ou um JWT longo, que não se cancela, ou login repetido.
4. **Quanto custa.** É a parte mais difícil de explicar do login: dois tokens, um cookie e a troca a cada uso. O front-end precisa renovar quando o token de acesso vence.
5. **Alternativas mais simples.** Um JWT de 8 horas, sem refresh token. Ou o cookie de sessão da decisão 19.
6. **Por que escolhemos assim.** "Oito horas sem nenhuma ação encerram a sessão" é o comportamento que o protótipo do front-end já simula. O front-end renova só quando precisa fazer uma requisição, nunca por temporizador, para que "sem ação" seja mesmo sem ação da pessoa.

- **Onde ver no código:** [SessionStore.cs](../backend/ControlService/src/ControlService.Infrastructure/Auth/SessionStore.cs), [RefreshCookie.cs](../backend/ControlService/src/ControlService.API/Auth/RefreshCookie.cs), [RefreshSessionHandler.cs](../backend/ControlService/src/ControlService.Application/Auth/RefreshSession/RefreshSessionHandler.cs).
- **Em uma frase:** o token curto prova quem sou; o longo fica no banco, onde posso cancelá-lo.

### 21. Senha temporária com troca obrigatória; o Admin é a conta de demonstração

1. **O que é.** Duas regras, decididas pelo dono em 2026-10-05:
   - **Usuário cadastrado.** Quem cadastra define uma senha temporária. A credencial nasce marcada (`MustChangePassword`), o JWT sai com o claim `must_change_password`, e um filtro no grupo `/api/v1` recusa tudo, menos `me`, sair e trocar a senha:

     ```csharp
     return isPending && !mayProceed
         ? ValueTask.FromResult<object?>(AuthErrors.PasswordChangeRequired.ToProblem())
         : next(context);
     ```

   - **Admin.** Nesta fase de portfólio, o Admin é a conta de demonstração: o login `admin` e a senha `admin123` são fixos e ficam à mostra na tela de entrada, para qualquer visitante explorar o sistema inteiro. Por isso o Admin **não** passa pela troca obrigatória e não troca a senha.
2. **Que problema resolve.** Quem cadastrou conhece a senha temporária, então ela não pode continuar valendo. E um avaliador precisa entrar no sistema sem pedir acesso a ninguém.
3. **O que acontece sem isso.** O primeiro acesso dependeria de um link enviado por e-mail: servidor de e-mail, tokens de ativação e mais telas, antes de o produto ter um ciclo completo.
4. **Quanto custa.** A senha temporária é passada por fora do sistema. Não existe "Esqueci minha senha": quem esquece a senha pede a alguém com o nível Gerenciador na tela Usuários que defina outra senha temporária (regra AUTH-29). E uma conta com todas as permissões é pública: um visitante pode excluir perfis e desativar usuários. O dono aceita isso enquanto o sistema for um portfólio; o que nunca se perde são o Admin e o perfil Gerenciador, que o domínio protege (decisão 16).
5. **Alternativas mais simples.** Esta já é a mais simples. A mais completa é o link de ativação por e-mail, adiado até o ciclo do produto estar fechado.
6. **Por que escolhemos assim.** O objetivo agora é um produto que possa ser testado do início ao fim. O que precisa mudar antes de um uso real está em [Antes de ir para o mundo real](#antes-de-ir-para-o-mundo-real).

**O código ainda reflete o plano antigo.** Hoje é o contrário do decidido: só o Admin nasce com a troca obrigatória, e os usuários cadastrados ainda não recebem senha (a issue #11 traz o cadastro). A senha inicial do Admin ainda vem da configuração (`Admin:InitialPassword`). Também sobram do plano antigo o status `pending`, o campo `ActivatedAt` e `User.Activate` com o erro `link_invalid`, o e-mail obrigatório, e as mensagens que falam em "senha inicial" e em "Esqueci minha senha". As regras em [product/](product/) já descrevem o desenho decidido (AUTH-13 a AUTH-15, AUTH-29, USR-07 e USR-35); o código muda nas issues do marco M1.

- **Onde ver no código:** [PasswordChangeRequiredFilter.cs](../backend/ControlService/src/ControlService.API/Auth/PasswordChangeRequiredFilter.cs), [ApiV1Group.cs](../backend/ControlService/src/ControlService.API/Common/ApiV1Group.cs), [ChangePasswordHandler.cs](../backend/ControlService/src/ControlService.Application/Auth/ChangePassword/ChangePasswordHandler.cs).
- **Em uma frase:** a primeira senha é de quem cadastrou, então o sistema obriga a trocá-la antes de liberar qualquer tela.

### 22. Bloqueio por tentativas e limite de requisições só no login

1. **O que é.** Duas proteções diferentes:
   - **Bloqueio por conta.** Cinco senhas erradas seguidas travam aquele login por 15 minutos. Quem conta é o Identity (decisão 18).
   - **Limite por endereço** (*rate limit*). Cada endereço IP pode fazer 20 logins e 60 renovações por minuto. Acima disso, a resposta é 429, com o cabeçalho `Retry-After` dizendo quanto esperar. Quem faz é o middleware que já vem no ASP.NET:

     ```csharp
     api.MapPost("/auth/sign-in", SignIn)
         .AllowAnonymous()
         .RequireRateLimiting(AuthRateLimiting.SignInPolicy)
     ```

2. **Que problema resolve.** O bloqueio protege uma conta de quem tenta adivinhar a senha dela. O limite protege de quem testa a mesma senha em muitas contas, que o bloqueio não enxerga.
3. **O que acontece sem isso.** O login aceitaria tentativas sem fim.
4. **Quanto custa.** Um arquivo e dois valores de configuração. Várias pessoas atrás do mesmo endereço de escritório dividem o limite.
5. **Alternativas mais simples.** Só o bloqueio por conta.
6. **Por que escolhemos assim.** As duas rotas limitadas são as únicas que aceitam requisição sem login. O limite global, para o resto da API, foi descartado: as outras rotas já exigem um token válido.

- **Onde ver no código:** [AuthRateLimiting.cs](../backend/ControlService/src/ControlService.API/Auth/AuthRateLimiting.cs), [CredentialUserManager.cs](../backend/ControlService/src/ControlService.Infrastructure/Auth/CredentialUserManager.cs).
- **Em uma frase:** errar a senha cinco vezes trava a conta; insistir demais no login trava o endereço.

### 23. Permissão por tela, em quatro níveis, pelo maior nível entre os perfis

1. **O que é.** Cada perfil de permissão dá um nível para cada tela: negado, leitor, editor ou gerenciador. Cada nível inclui o que o anterior permite. Uma pessoa pode ter vários perfis, e em cada tela vale o maior nível entre eles; uma tela que nenhum perfil menciona fica negada.

   ```csharp
   var levels = profiles.Select(profile => profile.GetLevel(screen));
   return levels.DefaultIfEmpty(AccessLevel.Denied).Max();
   ```

   O `GET /me` devolve o nível da pessoa em todas as telas.
2. **Que problema resolve.** É a regra do negócio (PERM-01 a PERM-07): o responsável pela empresa decide, tela a tela, o que cada pessoa pode fazer.
3. **O que acontece sem isso.** Com papéis fixos, como `[Authorize(Roles = "Admin")]`, cada combinação de tela e nível seria um papel escrito no código.
4. **Quanto custa.** Calcular o nível exige carregar os perfis da pessoa.
5. **Alternativas mais simples.** Dois ou três papéis fixos.
6. **Por que escolhemos assim.** É regra do produto, e o cálculo é uma função sem banco, testada no domínio. "Negado" é ausência de permissão, e não proibição: acrescentar um perfil nunca tira acesso de ninguém.

- **Onde ver no código:** [EffectiveAccess.cs](../backend/ControlService/src/ControlService.Domain/Access/EffectiveAccess.cs), [AccessLevel.cs](../backend/ControlService/src/ControlService.Domain/Access/AccessLevel.cs), [GetMeHandler.cs](../backend/ControlService/src/ControlService.Application/Auth/GetMe/GetMeHandler.cs). As regras estão em [product/features/permission-profiles.md](product/features/permission-profiles.md).
- **Em uma frase:** a permissão é um nível por tela, e com vários perfis vale o mais alto.

### 24. Autorização conferida no endpoint, lida do banco a cada requisição

**Ainda não está construído:** chega com a issue #9. Os nomes abaixo são os planejados.

1. **O que é.** Cada endpoint declara a tela e o nível mínimo que exige:

   ```csharp
   api.MapPost("/users", Create).RequireScreenAccess(ScreenKeys.Users, AccessLevel.Editor);
   ```

   Por trás dessa linha há um *filtro de endpoint*: uma classe que roda antes do método do endpoint e pode responder no lugar dele. É o mesmo molde do filtro da decisão 21. Serão dois:
   - um no grupo `/api/v1`, que confere se a conta está ativa e responde 401 `account_inactive` se não estiver;
   - um por endpoint, que lê os perfis da pessoa, calcula o nível (decisão 23) e responde 403 se for menor que o exigido.

   Os dois leem o banco a cada requisição, sem cache. A tabela vale para todas as telas:

   | Operação | Nível mínimo |
   |---|---|
   | Listar e ver | Leitor |
   | Criar e editar | Editor |
   | Excluir ou desativar (e reativar) qualquer cadastro | Gerenciador |

   No front-end, a tela com nível negado não aparece no menu, e os botões seguem a mesma tabela. Isso é conforto, não segurança: quem recusa de verdade é a API.
2. **Que problema resolve.** A regra de permissão fica num lugar só, e nenhuma tela precisa lembrar de conferi-la. Mudar um perfil ou desativar alguém vale já na requisição seguinte.
3. **O que acontece sem isso.** É o estado de hoje: qualquer pessoa logada chama qualquer endpoint, e quem é desativado continua com acesso até o token vencer (até 15 minutos).
4. **Quanto custa.** Duas consultas a mais por requisição (o usuário e os perfis dele). Para um ERP de uma empresa, não pesa.
5. **Alternativas mais simples.** Conferir a permissão dentro de cada handler, o que um dia alguém esquece. Ou pôr as permissões no JWT (decisão 19).
6. **Por que escolhemos assim.** O filtro devolve o erro do mesmo jeito que o resto da API (decisão 7) e não traz conceito novo. A alternativa oficial do ASP.NET, *requirement* com *authorization handler* (o que fica por trás de `[Authorize(Policy = "...")]`), exigiria duas classes, e ela só sabe responder 403, o que encaixa mal no 401 da conta desativada. O cache de permissões foi descartado em 2026-10-05: seria uma peça a mais para invalidar, a fim de economizar duas consultas.

- **Onde ver no código:** ainda não há; o filtro que serve de molde é [PasswordChangeRequiredFilter.cs](../backend/ControlService/src/ControlService.API/Auth/PasswordChangeRequiredFilter.cs). A tabela de níveis é a regra PERM-03 em [product/features/permission-profiles.md](product/features/permission-profiles.md).
- **Em uma frase:** cada endpoint diz a tela e o nível que exige, e a resposta vem do banco na hora, sem cache.

### 25. Chaves de tela como constantes estáveis

1. **O que é.** Cada tela tem uma chave fixa, numa classe do domínio, e é ela que fica gravada nas permissões:

   ```csharp
   public const string Users = "gerenciamento/usuarios";
   ```

   O nome que aparece no menu pode mudar; a chave, nunca. Uma chave aposentada não é reaproveitada.
2. **Que problema resolve.** No protótipo, a chave era derivada do nome da tela. Renomear "Relatório de Vendas" mudaria a chave, nenhum perfil teria nível para a chave nova, e a tela sumiria para todos, sem aviso.
3. **O que acontece sem isso.** Corrigir um acento num nome de menu apagaria permissões.
4. **Quanto custa.** Uma tela nova exige uma constante no back-end e uma entrada no menu do front-end.
5. **Alternativas mais simples.** Derivar a chave do nome. Ou usar ids numéricos, que são estáveis, mas ilegíveis no banco.
6. **Por que escolhemos assim.** Separar o identificador do texto exibido custa uma linha por tela. A rota que entrega o catálogo de telas ao front-end (`GET /screens`) chega com a issue #10.

- **Onde ver no código:** [ScreenKeys.cs](../backend/ControlService/src/ControlService.Domain/Access/ScreenKeys.cs), [ScreenKey.cs](../backend/ControlService/src/ControlService.Domain/Access/ScreenKey.cs), [screen-catalog.json](product/screen-catalog.json).
- **Em uma frase:** a permissão aponta para um código que nunca muda, e não para o nome que aparece no menu.

## Bloco D — Qualidade e operação

### 26. Testes em quatro projetos, cada comportamento em uma camada, contra PostgreSQL de verdade

1. **O que é.** Quatro projetos de teste, com cerca de 200 testes em outubro de 2026:

   | Projeto | O que testa | Testes | Precisa de |
   |---|---|---|---|
   | `Domain.Tests` | Value Objects, agregados, `EffectiveAccess` | 94 | Nada |
   | `Application.Tests` | Handlers, com fakes (decisão 27) | 34 | Nada |
   | `Api.IntegrationTests` | Chamadas HTTP de verdade, login, banco | 69 | Docker |
   | `ArchitectureTests` | Regras entre as camadas (decisão 28) | 4 | Nada |

   As ferramentas são o xUnit v3, que é o framework de testes, e o Shouldly, que escreve as conferências (`result.IsSuccess.ShouldBeTrue()`). Os testes de integração sobem a API inteira em memória (`WebApplicationFactory`) contra um PostgreSQL real, num contêiner que o próprio teste liga e desliga (Testcontainers). Há um contêiner só para todos os testes:

   ```csharp
   private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.3").Build();
   ```

   Cada comportamento é testado em **uma camada só**, a mais barata que o prova: a regra de um Value Object, no domínio; uma operação, pelo endpoint.
2. **Que problema resolve.** A regra de negócio é testada rápido e sem banco. O que depende do banco (índice único, concorrência, chave estrangeira) é testado no mesmo banco que roda em produção.
3. **O que acontece sem isso.** Com o banco em memória do EF Core, um teste de unicidade passaria sem o índice existir, porque esse provedor não aplica índices nem restrições.
4. **Quanto custa.** Os testes de integração exigem o Docker ligado e rodam em sequência, porque dividem o mesmo banco. Cada teste cria os próprios dados, com nomes únicos.
5. **Alternativas mais simples.** O banco em memória ou o SQLite. Ou só testes de unidade.
6. **Por que escolhemos assim.** As regras de negócio são o centro do projeto, e várias delas só existem de verdade dentro do PostgreSQL.

- **Onde ver no código:** [CpfTests.cs](../backend/ControlService/tests/ControlService.Domain.Tests/Users/CpfTests.cs), [SignInApiTests.cs](../backend/ControlService/tests/ControlService.Api.IntegrationTests/Auth/SignInApiTests.cs), [PostgresContainerFixture.cs](../backend/ControlService/tests/ControlService.Api.IntegrationTests/Common/PostgresContainerFixture.cs), [tests/Directory.Build.props](../backend/ControlService/tests/Directory.Build.props).
- **Em uma frase:** a regra de negócio eu testo sem banco, e o que depende do banco eu testo num PostgreSQL de verdade, e não numa imitação.

### 27. Teste antes do código, com lista numerada e fakes em memória

1. **O que é.** Duas práticas:
   - **Teste primeiro** (*test-first*). Todo comportamento começa por um teste que falha; depois vem o código que o faz passar, e por fim a limpeza (Red → Green → Refactor). Cada issue tem uma lista numerada de testes (`T01`, `T02`), que aparece igual na issue, no pull request e no relatório final, com o resultado de cada um.
   - **Fakes em memória.** O `SignInHandler` recebe interfaces, como `IUserRepository`. Em produção, quem a implementa é o repositório que fala com o PostgreSQL. No teste de handler, quem a implementa é um *fake*: uma classe do próprio projeto de testes, que guarda os dados numa lista.

     ```csharp
     internal sealed class InMemoryUserRepository : IUserRepository
     {
         private readonly List<User> _users = [];

         public void Add(User user) => _users.Add(user);

         public Task<User?> GetByLoginAsync(Login login, CancellationToken cancellationToken) =>
             Task.FromResult(_users.Find(user => user.Login.Value == login.Value));
     }
     ```

     O teste confere o **resultado**, e não as chamadas internas:

     ```csharp
     var bed = new AuthTestBed();
     var user = bed.AddActiveUser("ana.souza", "senha-da-ana");

     var result = await bed.CreateSignInHandler()
         .Handle(new SignInCommand("ana.souza", "senha-da-ana"), TestContext.Current.CancellationToken);

     result.IsSuccess.ShouldBeTrue();
     var session = bed.Sessions.SessionsOf(user.Id).ShouldHaveSingleItem();
     ```

2. **Que problema resolve.** O teste escrito antes descreve a regra, e não o código que já existe. E o fake deixa refatorar o handler: o teste só quebra se o resultado mudar.
3. **O que acontece sem isso.** Testes escritos depois tendem a confirmar o que o código já faz. Com um *mock* (um objeto que uma biblioteca fabrica durante o teste, e que confere quais métodos foram chamados), o teste quebra quando se troca o método chamado, mesmo com o resultado igual.
4. **Quanto custa.** Mais tempo por funcionalidade, e uma classe fake para cada interface da `Application`; hoje são sete. O fake não prova que o SQL funciona: o repositório de verdade é testado contra o PostgreSQL (decisão 26).
5. **Alternativas mais simples.** Testar depois. Teste primeiro só no domínio. Mocks em todos os testes.
6. **Por que escolhemos assim.** O conjunto de testes é a rede de segurança para refatorar, e precisa sobreviver às refatorações. Em 2026-10-05 a forma de trabalho mudou: antes havia uma pausa a cada teste, para o dono aprovar; agora a issue é executada de uma vez, e a evidência é o relatório final. A regra do teste primeiro não mudou. Uma peça sem um teste que possa falhar antes (configuração, migrations) é nomeada na issue junto com o teste que a cobre.

- **Onde ver no código:** a pasta [Fakes](../backend/ControlService/tests/ControlService.Application.Tests/Fakes/), [AuthTestBed.cs](../backend/ControlService/tests/ControlService.Application.Tests/Auth/AuthTestBed.cs), [SignInTests.cs](../backend/ControlService/tests/ControlService.Application.Tests/Auth/SignInTests.cs). As regras de trabalho estão no [AGENTS.md](../AGENTS.md#tests).
- **Em uma frase:** escrevo o teste antes do código, e no lugar do banco uso uma classe simples em memória, para o teste conferir o resultado e não as chamadas.

### 28. Testes de arquitetura

1. **O que é.** Testes que leem os projetos já compilados e falham se uma camada usar o que não pode. Usam o pacote NetArchTest:

   ```csharp
   [Fact]
   public void Domain_does_not_depend_on_other_layers_or_frameworks() =>
       AssertNoDependency(DomainAssembly,
           ApplicationNamespace, InfrastructureNamespace, ApiNamespace, EntityFrameworkCore, AspNetCore);
   ```

   | Regra | O que proíbe |
   |---|---|
   | Domínio | Usar `Application`, `Infrastructure`, `API`, EF Core ou ASP.NET Core |
   | Application | Usar `Infrastructure`, `API`, EF Core ou ASP.NET Core |
   | Infrastructure | Usar a `API` |
   | Agregados | Ter uma propriedade com `set` público |

2. **Que problema resolve.** A regra de camadas (decisão 2) e a do agregado que protege os próprios dados (decisão 3) deixam de depender de atenção na revisão.
3. **O que acontece sem isso.** As referências entre projetos impedem uma parte: o `Domain` não referencia nenhum outro projeto. Mas nada impediria a `Application` de usar um pacote do EF Core, e isso só seria notado meses depois.
4. **Quanto custa.** Um pacote e um projeto com quatro testes. A lista de agregados do último teste é manual: um agregado novo precisa ser acrescentado a ela.
5. **Alternativas mais simples.** Só a revisão de código.
6. **Por que escolhemos assim.** São poucas linhas para uma garantia que não depende de memória. **Planejada:** uma quinta regra, que proíbe a `API` de usar tipos da `Infrastructure` fora do `Program.cs`, para nenhum endpoint consultar o banco sem passar por um handler. Hoje o código já a cumpre; o teste entra numa issue própria, antes dos próximos endpoints.

- **Onde ver no código:** [LayerDependencyTests.cs](../backend/ControlService/tests/ControlService.ArchitectureTests/LayerDependencyTests.cs), [AggregateTests.cs](../backend/ControlService/tests/ControlService.ArchitectureTests/AggregateTests.cs).
- **Em uma frase:** as regras entre as camadas são testes: quem as viola quebra o build.

### 29. As mesmas regras de qualidade em todos os projetos

1. **O que é.** Um arquivo, o `Directory.Build.props`, vale para todos os projetos da solução:

   ```xml
   <Nullable>enable</Nullable>
   <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
   <AnalysisLevel>latest-recommended</AnalysisLevel>
   <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
   ```

   | Opção | O que faz |
   |---|---|
   | `Nullable` | O compilador avisa onde um valor pode ser `null` |
   | `TreatWarningsAsErrors` | Um aviso quebra o build |
   | `AnalysisLevel` | Liga os analisadores recomendados do .NET, que apontam erros comuns |
   | `EnforceCodeStyleInBuild` | As regras de estilo do `.editorconfig` (campo privado com `_`, por exemplo) valem no build, e não só no editor |

   A **formatação** é o texto do código: espaços, recuo, linhas em branco, ordem dos `using`. O padrão está no `.editorconfig`, e o comando `dotnet format` reescreve os arquivos para segui-lo. O script `check.ps1` formata, compila e testa de uma vez.
2. **Que problema resolve.** Os avisos não se acumulam, e um projeto novo já nasce com as regras.
3. **O que acontece sem isso.** Cada projeto teria as suas opções, e os avisos de `null` seriam ignorados até virarem `NullReferenceException`.
4. **Quanto custa.** Um aviso pequeno trava o trabalho até ser corrigido. Só se suprime um aviso no próprio local, com a justificativa.
5. **Alternativas mais simples.** Deixar os avisos como avisos. Configurar cada projeto.
6. **Por que escolhemos assim.** Um aviso que não quebra o build acaba ignorado.

**O CI confere a formatação.** O trabalho **Build and test** roda `dotnet format ControlService.slnx --verify-no-changes --no-restore` antes de compilar: o comando confere sem alterar, e um arquivo fora do padrão reprova o pull request. O `check.ps1` continua corrigindo a formatação na máquina de quem desenvolve.

- **Onde ver no código:** [Directory.Build.props](../backend/ControlService/Directory.Build.props), [.editorconfig](../backend/ControlService/.editorconfig), [check.ps1](../backend/ControlService/check.ps1).
- **Em uma frase:** as regras de qualidade ficam num arquivo só, valem para todos os projetos, e um aviso quebra o build.

### 30. As versões dos pacotes num arquivo só, com auditoria e atualização automática

1. **O que é.** Três peças:
   - **Versões centralizadas.** O `Directory.Packages.props` declara a versão de cada pacote uma vez, e os projetos citam o pacote sem versão:

     ```xml
     <!-- Directory.Packages.props -->
     <PackageVersion Include="FluentValidation" Version="12.1.1" />

     <!-- ControlService.Application.csproj -->
     <PackageReference Include="FluentValidation" />
     ```

   - **Auditoria.** Com `NuGetAudit` no modo `all`, a restauração dos pacotes avisa quando um deles tem uma vulnerabilidade conhecida, mesmo que seja dependência de outro pacote. Como um aviso é um erro (decisão 29), o build quebra.
   - **Dependabot.** Um serviço do GitHub que abre, toda semana, pull requests com as atualizações de pacotes, agrupadas por família.
2. **Que problema resolve.** Dois projetos nunca usam versões diferentes do mesmo pacote, e uma vulnerabilidade não passa em silêncio.
3. **O que acontece sem isso.** As versões ficariam espalhadas em dez arquivos `.csproj` e divergiriam com o tempo.
4. **Quanto custa.** Uma vulnerabilidade recém-publicada pode quebrar o build de um dia para o outro, sem ninguém ter mexido no código. E os pull requests do Dependabot precisam de revisão.
5. **Alternativas mais simples.** A versão em cada `.csproj`, atualizada à mão.
6. **Por que escolhemos assim.** É uma opção do próprio .NET, sem pacote extra. Regra do projeto: nenhuma versão é declarada fora desse arquivo.

**Regra de manutenção.** Uma versão só é declarada quando um projeto a usa. `MailKit` (envio de e-mail), `Microsoft.Extensions.Caching.Hybrid` (cache de permissões) e `FluentValidation.DependencyInjectionExtensions` foram removidos por falta de uso. O último pode voltar se houver justificativa, por exemplo registrar os validadores automaticamente.

- **Onde ver no código:** [Directory.Packages.props](../backend/ControlService/Directory.Packages.props), [dependabot.yml](../.github/dependabot.yml).
- **Em uma frase:** cada pacote tem a versão declarada num lugar só, e o build quebra se algum tiver uma vulnerabilidade conhecida.

### 31. .NET Aspire para rodar o sistema na máquina de desenvolvimento

1. **O que é.** O Aspire é uma ferramenta do .NET para o desenvolvimento local. Um projeto, o `AppHost`, descreve em C# o que o sistema precisa para rodar:

   ```csharp
   var postgres = builder.AddPostgres("postgres")
       .WithContainerName($"{NamePrefix}-postgres")
       .WithLifetime(ContainerLifetime.Persistent)
       .WithDataVolume($"{NamePrefix}-postgres-data");

   var database = postgres.AddDatabase("controlservice");
   ```

   Um comando (`dotnet run --project src/ControlService.AppHost`) sobe o PostgreSQL num contêiner, entrega a *connection string* à API, inicia a API e abre um painel com os logs e os tempos de cada requisição. Um segundo projeto, o `ServiceDefaults`, é o modelo do Aspire para a telemetria e os *health checks* (decisão 32).
2. **Que problema resolve.** Quem clona o repositório roda o sistema inteiro só com o Docker e o SDK do .NET, sem instalar o PostgreSQL nem editar uma connection string.
3. **O que acontece sem isso.** Um `docker-compose.yml`, mais a connection string copiada à mão para a configuração.
4. **Quanto custa.** Dois projetos a mais na solução, e uma ferramenta nova, que muda rápido. Serve só ao desenvolvimento: a publicação usa a imagem da decisão 33.
5. **Alternativas mais simples.** O Docker Compose, que é mais conhecido. Ou o PostgreSQL instalado na máquina.
6. **Por que escolhemos assim.** Já está construído, e o painel entrega a observabilidade local sem nenhuma configuração.

- **Onde ver no código:** [AppHost.cs](../backend/ControlService/src/ControlService.AppHost/AppHost.cs), [Extensions.cs](../backend/ControlService/src/ControlService.ServiceDefaults/Extensions.cs), e a linha `builder.AddServiceDefaults()` do [Program.cs](../backend/ControlService/src/ControlService.API/Program.cs).
- **Em uma frase:** um comando sobe o banco e a API já ligados um ao outro, e abre um painel para ver o que acontece.

### 32. Observabilidade com o `ILogger` do .NET e o OpenTelemetry

1. **O que é.** Observabilidade é conseguir ver o que o sistema fez. São três peças:
   - **Logs:** o `ILogger`, que já vem no .NET, sem biblioteca extra.
   - **Traces e métricas:** o OpenTelemetry, um padrão aberto que não prende o código a um fornecedor. Um *trace* é o caminho de uma requisição, com o tempo de cada etapa e de cada consulta ao banco.
   - **Destino:** na máquina local, o painel do Aspire. O envio só é ligado quando existe um endereço configurado:

     ```csharp
     var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
     ```

   Um teste (`AuthLoggingTests`) entra no sistema, renova a sessão e troca a senha, e confere que nenhuma senha, token ou cookie foi escrito nos logs.

   **Não confundir com a auditoria.** "Quem criou" e "quem alterou" cada registro são dados do sistema, gravados no banco (decisão 14). O log é um texto técnico, para quem desenvolve investigar um problema, e é descartado com o tempo.
2. **Que problema resolve.** Quando algo falha, dá para achar a requisição e ver em que etapa ela parou.
3. **O que acontece sem isso.** Só o console, sem ligação entre uma requisição e as consultas que ela fez.
4. **Quanto custa.** Cinco pacotes do OpenTelemetry, todos dentro do `ServiceDefaults`.
5. **Alternativas mais simples.** Só o `ILogger`, escrevendo no console.
6. **Por que escolhemos assim.** O `ILogger` já grava logs estruturados, e o OpenTelemetry já os leva ao painel. O Serilog, uma biblioteca de logs muito usada, estava no plano e foi descartado em 2026-10-05: seria uma dependência a mais para o mesmo resultado.

**O que falta em relação ao plano antigo,** e fica para quando o sistema for publicado ([Antes de ir para o mundo real](#antes-de-ir-para-o-mundo-real)):
- Não há uma linha de log por requisição com o id de quem a fez.
- Os *health checks* (endereços que respondem se a API está no ar e se alcança o banco) são `/health` e `/alive`, e só existem em desenvolvimento.
- Fora da máquina local, a telemetria não tem destino.

- **Onde ver no código:** [Extensions.cs](../backend/ControlService/src/ControlService.ServiceDefaults/Extensions.cs), [AuthLoggingTests.cs](../backend/ControlService/tests/ControlService.Api.IntegrationTests/Auth/AuthLoggingTests.cs).
- **Em uma frase:** uso o log que já vem no .NET e um padrão aberto para os traces, e um teste garante que senha e token nunca vão para o log.

### 33. Imagem de contêiner por Dockerfile

1. **O que é.** Uma imagem de contêiner é um pacote com a API e tudo de que ela precisa para rodar. Quem a descreve é o `Dockerfile`, em etapas: uma compila, usando a imagem do SDK; a final tem só o necessário para executar, e roda a API com um usuário sem privilégios.

   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
   USER $APP_UID
   ```

2. **Que problema resolve.** A API roda do mesmo jeito em qualquer hospedagem que aceite contêineres.
3. **O que acontece sem isso.** Publicar exigiria instalar o .NET no servidor e copiar os arquivos.
4. **Quanto custa.** Um arquivo para manter: cada projeto novo de que a API dependa precisa de uma linha `COPY`. Hoje a imagem não é usada em lugar nenhum; o CI só confere que ela é montada (decisão 34).
5. **Alternativas mais simples.** Deixar o SDK do .NET gerar a imagem, sem Dockerfile (`dotnet publish /t:PublishContainer`), que era o plano original. Ou não ter imagem até existir onde publicá-la.
6. **Por que escolhemos assim.** O Dockerfile já existe, o CI o valida, e é o formato que qualquer pessoa da área reconhece.

- **Onde ver no código:** [Dockerfile](../backend/ControlService/src/ControlService.API/Dockerfile), [.dockerignore](../backend/ControlService/.dockerignore).
- **Em uma frase:** a API vira uma imagem que roda igual em qualquer lugar, com um usuário sem privilégios.

### 34. Integração contínua no GitHub Actions; a publicação é manual

1. **O que é.** A integração contínua (CI) roda no GitHub a cada pull request e a cada mudança na `main`, em dois trabalhos:
   - **Build and test:** restaura os pacotes, confere a formatação (decisão 29), compila em `Release` (um aviso é um erro) e roda todos os testes, com o PostgreSQL de verdade. A cobertura de código é guardada como anexo da execução.
   - **Build API image:** monta a imagem do Dockerfile, sem publicá-la.

   A `main` é protegida por uma regra do repositório: só recebe mudanças por pull request, e só com os dois trabalhos verdes. O fluxo usa um token só de leitura:

   ```yaml
   permissions:
     contents: read
   ```

2. **Que problema resolve.** "Na minha máquina funciona" deixa de valer como prova. O selo verde no README é a evidência.
3. **O que acontece sem isso.** Um pull request poderia quebrar o build ou um teste sem ninguém perceber.
4. **Quanto custa.** Alguns minutos por pull request. A cobertura é coletada, mas não tem meta nem selo.
5. **Alternativas mais simples.** Rodar o `check.ps1` à mão antes de cada merge.
6. **Por que escolhemos assim.** É gratuito para um repositório público e fica à vista de quem avalia o portfólio. Ficaram de fora, em 2026-10-05, a publicação automática e a conferência de um `openapi.json`.

**A demonstração pública.** O sistema terá uma demonstração aberta a visitantes (decisão 21), publicada à mão. Onde hospedar ainda não foi decidido: a escolha será feita quando o front-end entrar no repositório, porque depende dele. O que falta para publicar está em [Antes de ir para o mundo real](#antes-de-ir-para-o-mundo-real).

- **Onde ver no código:** [ci.yml](../.github/workflows/ci.yml).
- **Em uma frase:** todo pull request é compilado e testado num ambiente limpo antes de poder entrar na `main`.

## Antes de ir para o mundo real

O sistema é, por enquanto, um portfólio: precisa ser fácil de visitar e ter o ciclo testável do início ao fim. O que está abaixo é aceito nesta fase e precisa ser revisto antes de um uso real.

| Hoje, no portfólio | Antes de um uso real |
|---|---|
| O login `admin` e a senha `admin123` são fixos e ficam à mostra na tela de entrada, e o Admin não troca a senha (decisão 21) | Tirar a senha fixa do código e da tela, e permitir que o Admin troque a própria senha. O Admin e o perfil Gerenciador continuam: são a garantia de que sempre existe alguém com todas as permissões |
| Não existe "Esqueci minha senha" | Links de ativação e de redefinição por e-mail |
| A senha temporária é passada por fora do sistema | O mesmo link de ativação |
| As migrations só são aplicadas sozinhas em desenvolvimento (decisão 11) | Um passo da publicação que aplique as migrations antes de a versão nova subir |
| A imagem da API é montada no CI, mas não é publicada em lugar nenhum (decisões 33 e 34) | Escolher a hospedagem do front-end, da API e do banco, e publicar |
| Os health checks só existem em desenvolvimento, e a telemetria só vai para o painel local (decisão 32) | Expor os health checks à hospedagem e definir um destino para os logs e os traces |
| Não há log de quem fez cada requisição (decisão 32) | Uma linha de log por requisição com o id do usuário, sem dados pessoais |
| O refresh token viaja num cookie, e o front-end e a API ainda não têm endereço público (decisão 20) | Definir como os dois ficam sob o mesmo endereço, para o navegador aceitar o cookie |
| A conta Admin é pública, e um visitante pode alterar ou apagar dados (decisão 21) | Na demonstração, restaurar o banco de tempos em tempos |

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
| Cache de permissões (HybridCache, Redis) | Ler o usuário e os perfis a cada requisição custa duas consultas; o cache seria uma peça a mais para invalidar (decisão 24) |
| Links de ativação e de redefinição de senha por e-mail | Adiado, não descartado: volta quando o ciclo do produto estiver fechado (decisão 21) |
| Limite global de requisições | Só as rotas sem login são limitadas; as outras já exigem um token válido (decisão 22) |
| Permissões dentro do JWT | Mudar um perfil só valeria quando o token vencesse, e o token cresceria a cada tela nova |
| Papéis fixos (`Roles`) | Não expressam "um nível por tela" sem um papel para cada combinação |
| *Requirement* e *authorization handler* do ASP.NET | Dois conceitos a mais para o mesmo resultado de um filtro, e só sabem responder 403 (decisão 24) |
| Autenticação só por cookie | Serviria para este front-end, mas o JWT é o mais pedido em vagas e serve para um aplicativo de celular |
| Provedor de identidade externo (Keycloak, Auth0) | Tiraria do código a parte de login, que é uma das que o portfólio quer mostrar |
| `SignInManager` e `MapIdentityApi` do Identity | Trazem autenticação por cookie e rotas prontas que não emitem o JWT do projeto |
| Banco em memória ou SQLite nos testes | Não aplicam índices nem restrições como o PostgreSQL; um teste passaria sem a regra existir no banco (decisão 26) |
| Biblioteca de mocks (NSubstitute, Moq) | Os fakes em memória conferem o resultado, e não as chamadas internas, e sobrevivem às refatorações (decisão 27) |
| Regra de nomes nos testes de arquitetura (`...Handler`, `...Validator`) | Um nome fora do padrão não causa defeito, e a revisão o pega (decisão 28) |
| Meta mínima de cobertura de código | A cobertura é coletada no CI; uma meta levaria a escrever testes para o número, e não para as regras |
| Docker Compose | O Aspire já sobe o banco e a API com um comando, e entrega a connection string (decisão 31) |
| Serilog | O `ILogger` do .NET já grava logs estruturados, e o OpenTelemetry os leva ao painel (decisão 32) |
| Gerar a imagem pelo SDK, sem Dockerfile | O Dockerfile já existe, é validado no CI e é o formato mais conhecido (decisão 33) |
| Publicação automática (deploy) | A publicação será manual até o ciclo do produto estar fechado (decisão 34) |

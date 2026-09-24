# 5. Integração com o front-end

Esta seção descreve como o front-end conversa com os dados e o que o back-end precisa oferecer: as operações, os comportamentos, as [rotas HTTP](#rotas-http) e o [formato dos erros](#formato-da-resposta-de-erro). As escolhas técnicas do back-end estão nos ADRs, em [adr/](adr/README.md).

## O servidor simulado

Enquanto o back-end não existe, o front-end traz uma camada chamada **servidor simulado** (`createSimulatedServer`, na seção "Servidor simulado" do script). Ela faz o papel do back-end:

- oferece as operações descritas abaixo;
- aplica todas as regras de [03-regras-de-negocio.md](03-regras-de-negocio.md) e [04-permissoes.md](04-permissoes.md): validação, unicidade, versão, autoria, desativação, acesso por nível, links de uso único e bloqueio por tentativas;
- devolve erros no mesmo formato que o back-end deverá usar.

As telas nunca acessam os dados diretamente: elas só chamam as operações do objeto `api`. **Para conectar o back-end real, basta substituir `createSimulatedServer` por um cliente que faça as chamadas HTTP e ofereça os mesmos métodos, com as mesmas respostas e os mesmos erros.** Nenhuma tela precisa mudar.

### Onde a simulação guarda os dados

A simulação usa o banco de dados do artifact do Claude quando a página está aberta dentro do Claude. Se ele não responder em 4 segundos, ou der erro ao carregar os dados, ela usa o próprio navegador (`localStorage`, nas chaves `control-service:dados:*`). Ela mantém cinco coleções:

| Coleção | Conteúdo |
|---|---|
| `users` | Usuários, no formato de [02-modelo-de-dados.md](02-modelo-de-dados.md). |
| `profiles` | Perfis de permissão. |
| `credentials` | Senha cifrada e controle de tentativas de cada usuário. |
| `tokens` | Links de ativação e de troca de senha, guardados só em formato cifrado. |
| `emails` | E-mails "enviados", exibidos em **E-mails de teste**. |

### E-mails de teste

A simulação não envia e-mails de verdade. Os e-mails de ativação e de troca de senha ficam guardados e aparecem no botão **E-mails de teste** (ícone de envelope no topo, e também na tela de entrada), do mais recente para o mais antigo. Cada e-mail tem o botão do link, que abre a tela correspondente. O método `listEmails()` existe só na simulação. No back-end real, esse papel é do Mailpit (ADR-0030).

### Sessão

Na simulação, a sessão fica guardada no navegador (`control-service:sessao`) e dura **8 horas desde a última ação**. A cada operação, o servidor simulado confere se a sessão ainda vale e se a conta continua ativa.

No back-end real, o comportamento é o mesmo, com outro mecanismo (ADR-0019 e ADR-0032):
- **Token de acesso** de 15 minutos, guardado **só na memória** da página.
- **Token de renovação** num cookie `HttpOnly`, válido por **8 horas desde a última renovação**. Cada renovação troca o token e empurra o prazo.
- O front-end renova o token de acesso **só quando vai fazer um pedido** e o token venceu, nunca por um cronômetro. Assim, as 8 horas contam a partir da última ação da pessoa.
- Ao abrir a página, o front-end pede uma renovação. Se der certo, há sessão (`hasSession()`); se não, mostra a tela de entrada.
- A cada pedido, o servidor confere se a conta continua ativa. Uma pessoa desativada recebe `account_inactive` no pedido seguinte, mesmo com o token de acesso ainda válido.
- Os pedidos precisam enviar o cookie (`credentials: 'include'`), e a página precisa ser aberta num endereço `localhost` ou no mesmo site da API. Dentro do artifact do Claude, o cookie não funciona.

### Acesso de demonstração

No primeiro acesso, o login é `admin` com a senha inicial `admin123`, e o sistema pede uma senha nova. A tela de entrada mostra essa instrução.

## Carregamento por página

O front-end pede **só o que vai mostrar**: "página 1 dos usuários com 'silva' no nome, 10 por página". O servidor faz a busca, a ordenação e a contagem, e devolve só aquela página com o total de registros.

**Por que é assim**
- **Desempenho:** telas como Clientes e Contas a Receber podem ter milhares de registros. Carregar tudo deixaria a abertura lenta e o navegador pesado.
- **Segurança:** o navegador receberia dados que a pessoa talvez não tenha permissão de ver.

**Exceção.** Listas pequenas de referência continuam vindo inteiras: perfis de permissão, catálogo de telas e, no futuro, formas de pagamento.

**Unicidade.** Todas as verificações de unicidade (login, nome de exibição, nome de perfil) são feitas pelo servidor.

## Operações

Os nomes da coluna "Método" são os do objeto `api`. As respostas seguem o modelo de [02-modelo-de-dados.md](02-modelo-de-dados.md). Registros individuais incluem também `createdByName`, `updatedByName` e `deactivatedByName`, com os nomes de exibição de quem agiu.

### Acesso

| Operação | Método | Observações |
|---|---|---|
| Entrar | `signIn(login, password)` | Devolve `{ mustChangePassword }`. |
| Sair | `signOut()` | |
| Há sessão ativa? | `hasSession()` | Usado ao abrir o sistema. |
| Trocar a senha inicial | `changePassword(password, confirmation)` | Só para a conta com troca obrigatória. |
| Conferir um link | `checkLink(token, purpose)` | `purpose` é `activation` ou `reset`. Devolve `{ valid, displayName, login }`. |
| Ativar conta | `activateAccount(token, password, confirmation)` | |
| Pedir troca de senha | `requestPasswordReset(login)` | Sempre responde a mesma mensagem. |
| Trocar senha pelo link | `resetPassword(token, password, confirmation)` | |
| Quem está conectado | `me()` | `{ id, displayName, login, levels }`, com o nível efetivo em cada tela. |
| Catálogo de telas | `listScreens()` | Áreas, telas e chaves. |

### Usuários

| Operação | Método | Nível mínimo |
|---|---|---|
| Listar por página | `listUsers({ page, pageSize, search, status })` | Leitor |
| Consultar um | `getUser(id)` | Leitor |
| Sugerir login | `suggestLogin(fullName)` | Editor |
| Cadastrar | `createUser(data)` → `{ user, emailSent }` (o usuário é gravado antes do envio, e `emailSent` diz se o e-mail saiu; ver ADR-0031) | Editor |
| Alterar | `updateUser(id, data, version)` | Editor |
| Reenviar acesso | `resendAccess(id)` → `{ emailSent, email }` | Editor |
| Desativar | `deactivateUser(id, version)` | Gerenciador |
| Reativar | `reactivateUser(id, version)` → `{ user, emailSent }` | Gerenciador |

`status` aceita `current` (ativos e pendentes, o padrão), `pending`, `inactive` ou `all`. A busca procura no nome de exibição, no nome completo, no login e nos dígitos do CPF, ignorando acentos e maiúsculas. `listUsers` devolve `{ items, page, pageSize, totalCount }`, e cada item traz `profileNames` para a coluna de perfis.

### Perfis de permissão

| Operação | Método | Nível mínimo |
|---|---|---|
| Listar | `listProfiles()` | Leitor em Permissões **ou** em Usuários |
| Consultar um | `getProfile(id)` | Leitor |
| Cadastrar | `createProfile(data)` | Editor |
| Alterar | `updateProfile(id, data, version)` | Editor |
| Excluir | `deleteProfile(id, version)` | Gerenciador |

A lista de perfis também é liberada para quem pode ver Usuários, porque o cadastro de usuário precisa mostrar os perfis para escolha. Cada perfil listado traz `userCount`, `grantedCount` e `screenCount`.

Duplicar um perfil é feito pelo próprio front-end, que abre um cadastro novo preenchido com os dados do original.

## Rotas HTTP

Cada método do objeto `api` corresponde a uma rota do back-end real. Todas começam com `/api/v1` (ADR-0003).

### Acesso

| Método do front | Rota | Quem pode |
|---|---|---|
| `signIn` | `POST /api/v1/auth/sign-in` | Qualquer pessoa |
| `hasSession` | `POST /api/v1/auth/refresh` (usa o cookie de renovação) | Qualquer pessoa |
| `signOut` | `POST /api/v1/auth/sign-out` | Conectado |
| `changePassword` | `POST /api/v1/auth/change-password` | Conectado |
| `checkLink` | `POST /api/v1/auth/links/check` | Qualquer pessoa |
| `activateAccount` | `POST /api/v1/auth/activate` | Qualquer pessoa |
| `requestPasswordReset` | `POST /api/v1/auth/password-reset/request` | Qualquer pessoa |
| `resetPassword` | `POST /api/v1/auth/password-reset` | Qualquer pessoa |
| `me` | `GET /api/v1/me` | Conectado |
| `listScreens` | `GET /api/v1/screens` | Conectado |

O token dos links vai sempre **no corpo** do pedido, nunca no endereço, para não ficar registrado em logs (ADR-0030).

### Usuários

| Método do front | Rota | Nível mínimo |
|---|---|---|
| `listUsers` | `GET /api/v1/users?page=&pageSize=&search=&status=` | Leitor |
| `getUser` | `GET /api/v1/users/{id}` | Leitor |
| `suggestLogin` | `POST /api/v1/users/login-suggestion` (nome completo no corpo) | Editor |
| `createUser` | `POST /api/v1/users` | Editor |
| `updateUser` | `PUT /api/v1/users/{id}` | Editor |
| `resendAccess` | `POST /api/v1/users/{id}/resend-access` | Editor |
| `deactivateUser` | `POST /api/v1/users/{id}/deactivate` | Gerenciador |
| `reactivateUser` | `POST /api/v1/users/{id}/reactivate` | Gerenciador |

### Perfis de permissão

| Método do front | Rota | Nível mínimo |
|---|---|---|
| `listProfiles` | `GET /api/v1/permission-profiles` | Leitor em Permissões **ou** em Usuários |
| `getProfile` | `GET /api/v1/permission-profiles/{id}` | Leitor |
| `createProfile` | `POST /api/v1/permission-profiles` | Editor |
| `updateProfile` | `PUT /api/v1/permission-profiles/{id}` | Editor |
| `deleteProfile` | `DELETE /api/v1/permission-profiles/{id}` | Gerenciador |

### Versão e criação

- As consultas de um registro devolvem `version` no corpo e também no cabeçalho `ETag`.
- Alterar, desativar, reativar e excluir enviam a versão no cabeçalho `If-Match` (ADR-0014).
- Cadastrar responde `201 Created`, com o registro criado (incluindo o `id` gerado pelo servidor) e o cabeçalho `Location`.

## Erros

Todo erro vem como `ApiError`, com `status`, `code`, `message` (já em português, pronta para exibir) e `details`.

### Formato da resposta de erro

O back-end real responde no padrão Problem Details (ADR-0009). O cliente HTTP do front-end converte essa resposta em `ApiError`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "code": "validation_failed",
  "message": "Alguns campos precisam ser corrigidos.",
  "errors": {
    "login": ["Já existe um usuário com o login “ana.souza”. Escolha outro."],
    "address.cep": ["O CEP precisa ter 8 números."]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

| Resposta do back-end | `ApiError` |
|---|---|
| `status`, `code`, `message` | `status`, `code`, `message` |
| `errors` (primeira mensagem de cada campo) | `details.fields` |
| `details` (por exemplo `updatedByName`, `userNames`) | `details` |

**Login, nome de exibição ou nome de perfil repetidos** são erros de validação (400 `validation_failed`), com a mensagem no campo, como qualquer outra validação.

| `status` | `code` | Situação | O que o front-end faz |
|---|---|---|---|
| 400 | `validation_failed` | Campos inválidos. `details.fields` traz uma mensagem por campo, pelo caminho do modelo (`login`, `address.cep`, `emergencyContact.phone`). Nas telas de senha, os campos são `password` e `passwordConfirmation`. | Mostra cada mensagem embaixo do campo e, nos cadastros, um aviso no topo com quantos campos faltam corrigir. |
| 401 | `invalid_credentials` | Login ou senha errados. | Mostra a mensagem na tela de entrada. |
| 401 | `session_expired` | A sessão terminou. | Mostra a tela de entrada por cima do sistema, sem fechar as abas. |
| 401 | `account_inactive` | A conta foi desativada **enquanto a pessoa usava o sistema** (qualquer pedido depois de entrar). | Mostra a tela de entrada com a mensagem. |
| 401 | `password_change_required` | A senha inicial ainda não foi trocada. | Mostra a tela de troca de senha. |
| 403 | `forbidden` | Operação acima do nível da pessoa. | Atualiza o menu e mostra a mensagem de falta de acesso. |
| 403 | `account_inactive` | **Ao entrar**: senha correta, mas a conta está desativada. | Mostra a mensagem na tela de entrada. |
| 404 | `not_found` | Registro inexistente. | Mostra "Registro não encontrado". |
| 409 | `concurrency_conflict` | Outra pessoa alterou o registro. `details.updatedByName` traz quem. | Pergunta se a pessoa quer recarregar ou continuar na tela. |
| 409 | `profile_in_use` | Perfil com usuários. `details.userNames` traz quem. | Explica quem usa o perfil. |
| 409 | `system_record`, `self_deactivation`, `not_pending`, `not_inactive`, `email_missing` | Regras de negócio recusadas. | Mostra a mensagem num aviso. |
| 410 | `link_invalid` | Link vencido ou já usado. | Mostra a tela de link inválido. |
| 429 | `locked_out` | Bloqueio por tentativas. | Mostra a mensagem de espera. |

Qualquer outra falha mostra uma mensagem genérica e mantém o que a pessoa digitou.

# 2. Modelo de dados

Esta seção descreve os registros do sistema como foram **decididos**: é o modelo que o back-end deve implementar e que o servidor simulado do front-end já segue. As poucas diferenças restantes estão em [Diferenças em relação ao front-end atual](#diferenças-em-relação-ao-front-end-atual), no fim deste documento.

Há dois tipos de registro: **usuários** (coleção `users`) e **perfis de permissão** (coleção `profiles`).

## Convenções gerais

**Nomes de campos** em inglês e camelCase, como no código.

**Campos opcionais** deixados em branco são texto vazio (`""`), nunca `null` e nunca omitidos. Todo texto é gravado sem espaços no início e no fim.

**CPF, telefone e CEP** são dados informativos: servem para consulta e exibição, e nenhuma outra regra depende deles. São gravados **só com dígitos**, e quem exibe aplica a máscara (ver [03-regras-de-negocio.md](03-regras-de-negocio.md#formato-de-cpf-telefone-e-cep)).

**Autoria.** Todo registro tem quatro campos de autoria, preenchidos pelo servidor e nunca pelo navegador:

| Campo | Conteúdo |
|---|---|
| `createdAt` | Data e hora da criação, ISO 8601 UTC. |
| `createdBy` | `id` do usuário que criou. |
| `updatedAt` | Data e hora da última alteração, ISO 8601 UTC. |
| `updatedBy` | `id` do usuário que fez a última alteração. |

Ao exibir um registro, o sistema mostra essas informações com o nome de exibição de quem agiu (ver [03-regras-de-negocio.md](03-regras-de-negocio.md#autoria)).

**Versão.** Todo registro tem uma `version`, um valor opaco que muda a cada alteração. Quem lê o registro recebe a versão e precisa devolvê-la ao salvar ou desativar, para o sistema detectar se outra pessoa alterou o registro no meio do caminho (ver [03-regras-de-negocio.md](03-regras-de-negocio.md#várias-pessoas-editando-ao-mesmo-tempo)).

**Identificadores.** Cada registro tem um `id` no formato GUID (UUID v7), **gerado pelo servidor** no momento do cadastro. Quem cadastra não envia o `id`: ele vem na resposta. Os dois registros do sistema usam GUIDs fixos (ver [Registros do sistema](#registros-do-sistema) e ADR-0012).

## Usuário

### Acesso

| Campo | Tipo | Obrigatório | Formato e observações |
|---|---|---|---|
| `id` | texto | sim | Identificador único. |
| `login` | texto | sim | Nome usado para entrar no sistema, por exemplo `ana.souza`. Único, de 3 a 30 caracteres: letras sem acento, números, ponto, hífen e sublinhado. Gravado em minúsculas. |
| `email` | texto | sim | Para onde vão o link de ativação e o link de troca de senha. **Não precisa ser único**: duas pessoas podem usar o mesmo e-mail, por exemplo o de um setor. |
| `status` | texto | sim | `pending` (aguardando ativação), `active` (ativo) ou `inactive` (desativado). Controlado pelo sistema, nunca editado diretamente. |
| `activatedAt` | texto | não | Data e hora em que a pessoa criou a senha pela primeira vez. `""` enquanto pendente. |
| `deactivatedAt` | texto | não | Data e hora da desativação. `""` se não estiver desativado. |
| `deactivatedBy` | texto | não | `id` de quem desativou. `""` se não estiver desativado. |

A senha, os links de ativação e de troca de senha, e o controle de tentativas de acesso **não fazem parte do registro**. Eles ficam guardados à parte, só em formato cifrado, e nunca são devolvidos a quem consulta o usuário (ver ADR-0019).

### Dados pessoais

| Campo | Tipo | Obrigatório | Formato e observações |
|---|---|---|---|
| `fullName` | texto | sim | Nome completo. |
| `displayName` | texto | sim | Nome curto que identifica a pessoa no sistema e nos registros de atividade, por exemplo "Ana Souza". Único, sem diferenciar maiúsculas, minúsculas e acentos. |
| `cpf` | texto | não | 11 dígitos, sem pontos nem traço: `52998224725`. Quando preenchido, precisa ser válido pelos dígitos verificadores. **Não precisa ser único.** |
| `rg` | texto | não | Texto livre, sem formato nem validação. |
| `birthDate` | texto | não | Data no formato `AAAA-MM-DD`. Não pode ser futura. |
| `bloodType` | texto | não | Um de `A+`, `A-`, `B+`, `B-`, `AB+`, `AB-`, `O+`, `O-`, ou `""`. |
| `phone` | texto | não | 10 dígitos (fixo) ou 11 (celular), com DDD e sem formatação: `21987654321`. |
| `address` | objeto | sim | Sempre presente, mesmo com todos os campos vazios. Ver abaixo. |
| `emergencyContact` | objeto | sim | Sempre presente, mesmo com todos os campos vazios. Ver abaixo. |
| `signature` | texto | não | Imagem da assinatura. Ver [Assinatura](#assinatura). |
| `profileIds` | lista de textos | sim | Ids dos perfis de permissão da pessoa. Pode ser uma lista vazia: sem perfis, a pessoa não tem acesso a nenhuma tela. Ids de perfis que não existem são descartados ao salvar. |

Mais os campos de autoria (`createdAt`, `createdBy`, `updatedAt`, `updatedBy`) e `version`.

### `address`

Todos os campos são opcionais.

| Campo | Formato e observações |
|---|---|
| `cep` | 8 dígitos, sem traço: `20040020`. |
| `street` | Rua. |
| `number` | Número, em texto para aceitar "S/N" ou "120A". |
| `complement` | Complemento, por exemplo "apto 101". |
| `district` | Bairro. |
| `city` | Cidade. O estado (UF) não faz parte do cadastro. |

### `emergencyContact`

Todos os campos são opcionais.

| Campo | Formato e observações |
|---|---|
| `name` | Nome da pessoa a avisar. |
| `relationship` | Parentesco em texto livre, por exemplo "mãe" ou "irmão". |
| `phone` | Mesmo formato de `phone` do usuário. |

### Exemplo

```json
{
  "id": "0192f1a4-7c3e-7b21-9f5a-2d8e4c1b6a70",
  "login": "ana.souza",
  "email": "ana.souza@empresa.com.br",
  "status": "active",
  "activatedAt": "2026-09-12T13:05:44Z",
  "deactivatedAt": "",
  "deactivatedBy": "",
  "fullName": "Ana Paula Souza",
  "displayName": "Ana Souza",
  "cpf": "52998224725",
  "rg": "12.345.678-9",
  "birthDate": "1990-04-17",
  "bloodType": "O+",
  "phone": "21987654321",
  "address": {
    "cep": "20040020",
    "street": "Rua da Assembleia",
    "number": "10",
    "complement": "sala 501",
    "district": "Centro",
    "city": "Rio de Janeiro"
  },
  "emergencyContact": {
    "name": "Marcos Souza",
    "relationship": "irmão",
    "phone": "21998765432"
  },
  "signature": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "profileIds": ["00000000-0000-7000-8000-000000000002", "0192f1a2-1b4d-7e8f-a0c3-5d6e7f8a9b0c"],
  "createdAt": "2026-09-12T12:58:10Z",
  "createdBy": "00000000-0000-7000-8000-000000000001",
  "updatedAt": "2026-09-20T17:41:02Z",
  "updatedBy": "0192f1a3-9e2b-7c4d-8a1f-3b5c7d9e1f20",
  "version": "742"
}
```

### Assinatura

A assinatura é desenhada num quadro de 1200 × 400 pixels, com o mouse ou o dedo, ou enviada como imagem PNG, JPG ou WebP de até 5 MB. Uma imagem enviada é redimensionada para caber no quadro, mantendo a proporção.

Hoje o front-end grava a imagem dentro do registro, como *data URL*: PNG com fundo transparente ou, se passar de 180.000 caracteres, JPEG com fundo branco. O ADR-0018 propõe guardar a imagem num armazenamento de arquivos à parte, deixando no registro só a referência a ela.

**No back-end, a assinatura fica fora da primeira etapa** (login, usuários e permissões). Ela entra quando o ADR-0018 for decidido (ver [07-pendencias.md](07-pendencias.md)).

## Perfil de permissão

| Campo | Tipo | Obrigatório | Formato e observações |
|---|---|---|---|
| `id` | texto | sim | Identificador único. |
| `name` | texto | sim | Nome do perfil. Único entre perfis, inclusive o Gerenciador, sem diferenciar maiúsculas, minúsculas e acentos. |
| `description` | texto | não | Descrição livre. |
| `levels` | lista de objetos | sim | Itens no formato `{ "screen": chave, "level": nível }`. |

Mais os campos de autoria e `version`.

`screen` é a chave fixa de uma tela, como `comercial/clientes` (ver [04-permissoes.md](04-permissoes.md#chaves-das-telas)). `level` é um de `negado`, `leitor`, `editor` ou `gerenciador`. Uma tela que não aparece na lista vale `negado`, o que permite criar telas novas sem alterar perfis existentes.

### Exemplo (resumido)

```json
{
  "id": "0192f1a2-1b4d-7e8f-a0c3-5d6e7f8a9b0c",
  "name": "Vendedor",
  "description": "Equipe comercial",
  "levels": [
    { "screen": "gerenciamento/usuarios", "level": "negado" },
    { "screen": "comercial/clientes", "level": "editor" },
    { "screen": "comercial/roteiro-diario", "level": "editor" },
    { "screen": "financeiro/contas-a-receber", "level": "leitor" }
  ],
  "createdAt": "2026-09-10T11:20:00Z",
  "createdBy": "00000000-0000-7000-8000-000000000001",
  "updatedAt": "2026-09-23T14:10:44Z",
  "updatedBy": "0192f1a4-7c3e-7b21-9f5a-2d8e4c1b6a70",
  "version": "318"
}
```

## Registros do sistema

Dois registros existem sempre e não podem ser alterados, desativados nem excluídos.

| Registro | `id` | Conteúdo |
|---|---|---|
| Usuário **Admin** | `00000000-0000-7000-8000-000000000001` | `login`: `admin`; `displayName`: "Admin"; `fullName`: "Administrador do sistema"; `status`: `active`; `profileIds`: o id do Gerenciador. O e-mail e a senha inicial vêm da configuração do servidor, nunca do código (ver ADR-0022). |
| Perfil **Gerenciador** | `00000000-0000-7000-8000-000000000002` | `name`: "Gerenciador"; `description`: "Acesso total a todas as telas do sistema."; nível `gerenciador` em todas as telas, inclusive nas criadas depois. |

Os dois são marcados com `isSystem: true`, que é o que faz a interface exibi-los como somente leitura. Nos registros comuns, esse campo não existe.

**Os níveis do Gerenciador não são gravados.** O servidor calcula o nível `gerenciador` para todas as telas do catálogo e devolve a lista `levels` completa, como em qualquer outro perfil. Assim, uma tela nova já aparece com nível `gerenciador` nesse perfil sem nenhuma alteração de dados.

## Diferenças em relação ao front-end atual

O front-end, com o servidor simulado, já segue este modelo. As diferenças que restam são estas; o cliente HTTP que substituir o servidor simulado precisa tratá-las:

| Assunto | Modelo decidido | Servidor simulado |
|---|---|---|
| Identificadores | UUID v7 gerados pelo servidor e GUIDs fixos para os registros do sistema (ADR-0012). | UUID v4, gerados pela camada simulada. Os registros do sistema usam `sistema-admin` e `sistema-gerenciador`. |
| Registros do sistema | Gravados no banco (ADR-0022). | Definidos no código da camada simulada e somados às consultas. |
| Assinatura | Ainda em aberto: o ADR-0018 propõe um armazenamento de arquivos à parte. Fica fora da primeira etapa do back-end. | Gravada dentro do registro, como *data URL*. |
| Senhas | Cifradas pelo ASP.NET Core Identity (ADR-0019). | Cifradas com SHA-256 e um valor aleatório por usuário. Serve só para a demonstração. |

Registros criados antes destas decisões são convertidos automaticamente quando o sistema abre: CPF, telefone e CEP passam a ter só dígitos, o login é sugerido a partir do nome, a situação passa a ser pendente, e os campos de autoria e versão são preenchidos. Como esses registros não tinham e-mail, é preciso cadastrar um antes de reenviar o acesso.

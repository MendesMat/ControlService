# 3. Regras de negócio

Estas são as regras decididas para o sistema. O back-end deve aplicá-las todas, porque o navegador não é confiável como única barreira. As mensagens estão escritas exatamente como devem aparecer para as pessoas: foram pensadas para quem tem pouca familiaridade com tecnologia e servem de referência de tom para qualquer mensagem futura.

Todas estas regras já estão aplicadas no front-end, pelo servidor simulado descrito em [05-integracao-com-o-front.md](05-integracao-com-o-front.md).

## Usuários

### Validações ao salvar

| Campo | Regra | Mensagem exibida |
|---|---|---|
| `fullName` | Obrigatório. | Informe o nome completo. |
| `displayName` | Obrigatório. | Informe como o nome vai aparecer no sistema. |
| `displayName` | Único entre todos os usuários, incluindo o Admin e os desativados. A comparação ignora maiúsculas, minúsculas, acentos e espaços nas pontas: "Ana Souza", "ana souza" e "Ána Souza" são o mesmo nome. Serve para identificar cada pessoa sem ambiguidade nos registros de atividade. | Já existe um usuário chamado “*{nome existente}*”. Use um nome diferente, por exemplo acrescentando o sobrenome ou o setor. |
| `login` | Obrigatório. | Informe o login. |
| `login` | De 3 a 30 caracteres, só letras sem acento, números, ponto, hífen e sublinhado. Gravado em minúsculas. | O login deve ter de 3 a 30 caracteres, usando só letras sem acento, números, ponto, hífen ou sublinhado. |
| `login` | Único entre todos os usuários, incluindo o Admin e os desativados, sem diferenciar maiúsculas e minúsculas. | Já existe um usuário com o login “*{login}*”. Escolha outro. |
| `email` | Obrigatório e em formato de e-mail válido. **Não precisa ser único.** | Informe o e-mail. É para ele que o link de ativação será enviado. / Este e-mail não parece válido. Confira se ele tem @ e o domínio, por exemplo ana@empresa.com.br. |
| `cpf` | Opcional. Se preenchido, precisa ser válido pelos dígitos verificadores. CPFs com todos os dígitos iguais, como `111.111.111-11`, são recusados. **Não precisa ser único.** | Este CPF não é válido. Confira os números ou deixe o campo em branco. |
| `birthDate` | Se preenchida, não pode ser posterior à data de hoje. | A data de nascimento não pode ser no futuro. |
| `phone` | Se preenchido, precisa ter 10 ou 11 dígitos. | Digite o telefone com DDD. Ex.: (21) 98765-4321. |
| `address.cep` | Se preenchido, precisa ter 8 dígitos. | O CEP precisa ter 8 números. |
| `emergencyContact.phone` | Mesma regra de `phone`. | Digite o telefone com DDD. Ex.: (21) 98765-4321. |

Quando há erros, a tela mostra no topo quantos campos faltam corrigir e leva a pessoa ao primeiro deles. Cada mensagem aparece embaixo do seu campo, e some assim que a pessoa corrige o campo.

### Sugestão de login

Ao cadastrar um usuário, o sistema sugere um login a partir do nome completo: primeiro e último nome, sem acentos, em minúsculas e separados por ponto. "Ana Paula Souza" gera `ana.souza`. Se esse login já existir, o sistema acrescenta um número: `ana.souza2`, `ana.souza3`. Quem cadastra pode aceitar a sugestão ou digitar outro login.

Casos especiais:
- **Nome de uma palavra só:** a sugestão é essa palavra. "Madalena" gera `madalena`.
- **Caracteres fora da regra do login** (apóstrofo, espaços extras, símbolos) são removidos. "Joana D'Ávila" gera `joana.davila`.
- **Nome longo:** a sugestão é cortada para caber em 30 caracteres, **contando o número** acrescentado quando o login já existe.
- **Resultado com menos de 3 caracteres** (por exemplo, "Li"): o sistema não sugere nada, e quem cadastra digita o login.

### Formato de CPF, telefone e CEP

Esses três dados são **apenas informativos**: servem para consulta e exibição, e nenhuma outra regra depende deles. Por isso:

- **Gravação:** são guardados só com dígitos. O sistema aceita receber o valor com ou sem máscara e remove a formatação antes de gravar.
- **Exibição:** a máscara é aplicada só na hora de mostrar, e o campo se formata enquanto a pessoa digita.

| Campo | Gravado | Exibido |
|---|---|---|
| CPF | `52998224725` | `529.982.247-25` |
| CEP | `20040020` | `20040-020` |
| Telefone com 11 dígitos | `21987654321` | `(21) 98765-4321` |
| Telefone com 10 dígitos | `2134567890` | `(21) 3456-7890` |

A busca de usuários por CPF compara só os dígitos. Qualquer forma de digitar o número encontra a pessoa.

### Cálculo do dígito verificador do CPF

Para o primeiro dígito verificador, os 9 primeiros dígitos são multiplicados pelos pesos 10, 9, 8, …, 2 e somados. O resultado é multiplicado por 10 e dividido por 11; o resto é o dígito, e se o resto for 10, o dígito é 0. Para o segundo, repete-se o processo com os 10 primeiros dígitos e pesos 11, 10, …, 2.

## Acesso ao sistema

### Cadastro e ativação

Ninguém cria a própria conta. Um usuário novo é sempre cadastrado por alguém que tem acesso à tela de Usuários, e ninguém, em nenhum momento, vê ou digita a senha de outra pessoa.

1. Quem cadastra preenche os dados, incluindo login e e-mail, e salva.
2. O usuário é criado com a situação **pendente** (`pending`), e o sistema envia ao e-mail cadastrado um **link de ativação**.
3. A pessoa clica no link e cria a própria senha. A partir daí, o usuário fica **ativo** (`active`) e entra no sistema com login e senha.

O link de ativação é de uso único e vale por **72 horas**. Enquanto o usuário estiver pendente e tiver um e-mail cadastrado, o cadastro dele mostra o botão **Reenviar acesso**, na seção Acesso ao sistema. O botão envia um link novo para o e-mail salvo naquele momento e invalida o link anterior.

Se o e-mail estiver errado, basta corrigi-lo, salvar e reenviar. Se houver alterações ainda não salvas no cadastro, o botão pede para salvar primeiro: *"Este cadastro tem alterações que ainda não foram salvas. Salve primeiro, para o link ir para o e-mail correto."*

O usuário é **sempre gravado antes** do envio do e-mail, porque o link precisa apontar para um cadastro existente e o login e o nome de exibição precisam ficar reservados desde o início. O envio acontece logo em seguida, na mesma operação, e a tela sabe na hora se o e-mail saiu (ADR-0031).

Se o envio do e-mail falhar, o usuário continua cadastrado como pendente e a tela avisa, para que a pessoa use "Reenviar acesso". **Nunca é preciso excluir e cadastrar de novo:** se o e-mail estava errado, basta corrigi-lo, salvar e reenviar. No servidor simulado, o envio nunca falha: os e-mails vão para a caixa de E-mails de teste.

### Entrar

A pessoa entra com **login e senha**. O e-mail não serve para entrar, porque pode ser compartilhado por mais de uma pessoa.

Depois de **5 tentativas erradas seguidas**, o acesso daquele login fica bloqueado por **15 minutos**. A tela de entrada nunca revela se o erro foi no login ou na senha.

### Esqueci minha senha

A pessoa informa o **login**, e não o e-mail, pelo mesmo motivo acima. Se o login existir e estiver ativo, o sistema envia ao e-mail cadastrado um link para criar uma senha nova, de uso único e válido por **2 horas**. A resposta na tela é sempre a mesma, exista o login ou não, para não revelar quais logins existem. Criar a senha nova pelo link também desfaz um bloqueio por tentativas que estiver em andamento.

### Primeiro acesso do Admin

O Admin é a única conta que não recebe link de ativação, porque não há ninguém para cadastrá-lo. Ele entra com uma senha inicial e, logo em seguida, o sistema mostra a tela **"Crie uma senha nova"**: *"Por segurança, a senha inicial precisa ser trocada no primeiro acesso."* Enquanto a senha não for trocada, nenhuma outra tela fica disponível. No servidor simulado, a senha inicial é `admin123`. No back-end real, ela vem da configuração do servidor (ADR-0022).

O Admin "não pode ser alterado" (ver [Registros do sistema](#registros-do-sistema)), mas **pode trocar a própria senha**: a senha não faz parte do registro do usuário, e sim das credenciais guardadas à parte (ver [02-modelo-de-dados.md](02-modelo-de-dados.md#acesso)). O mesmo vale para "Esqueci minha senha".

### Senha

A senha precisa ter pelo menos **8 caracteres**. Não há exigência de símbolos ou números, porque senhas longas são mais seguras e mais fáceis de lembrar do que senhas curtas e complicadas. A pessoa digita a senha duas vezes para confirmar.

Os prazos e limites desta seção são valores iniciais e podem ser ajustados na configuração do servidor.

### Mensagens de acesso

| Situação | Mensagem exibida |
|---|---|
| Usuário cadastrado | Cadastro de *{nome}* criado. Enviamos o link de ativação para *{e-mail}*. |
| Falha no envio do e-mail | Cadastro de *{nome}* criado, mas não conseguimos enviar o e-mail de ativação. Use “Reenviar acesso” para tentar de novo. |
| Acesso reenviado | Enviamos um novo link de ativação para *{e-mail}*. |
| Login ou senha errados | Login ou senha incorretos. |
| Bloqueio por tentativas | Muitas tentativas sem sucesso. Aguarde 15 minutos e tente de novo. |
| Usuário desativado (com senha correta) | Este acesso está desativado. Fale com o responsável pelo sistema. |
| Link de ativação vencido ou já usado | Este link não vale mais. Peça a quem cadastrou você para reenviar o acesso. |
| Link de troca de senha vencido ou já usado | Este link não vale mais. Peça um novo em “Esqueci minha senha”. |
| Pedido de troca de senha | Se esse login existir, enviamos um link para o e-mail cadastrado. Confira sua caixa de entrada. |
| Senha curta | A senha precisa ter pelo menos 8 caracteres. |
| Senhas diferentes | As duas senhas não são iguais. Digite de novo. |
| Senha inicial do Admin trocada | Senha criada. Tudo pronto para começar. |
| Sessão encerrada | Sua sessão terminou. Entre de novo para continuar. Suas abas continuam abertas. |
| Saída pelo botão Sair | Você saiu do sistema. |
| Sair com alterações não salvas | **Sair com alterações não salvas?** Algumas abas têm alterações que ainda não foram salvas. Se você sair agora, elas serão perdidas. |


## Mensagens de confirmação dos cadastros

Mensagens rápidas que aparecem no alto da tela depois de uma ação concluída. As que mencionam e-mail trazem o botão **Ver e-mail**, que abre a caixa de E-mails de teste.

| Situação | Mensagem exibida |
|---|---|
| Usuário alterado | Cadastro de *{nome}* salvo. |
| Usuário desativado | Acesso de *{nome}* desativado. |
| Usuário reativado, já com senha | Acesso de *{nome}* reativado. A pessoa já pode entrar no sistema. |
| Usuário reativado, ainda sem senha | Acesso de *{nome}* reativado. Como a senha ainda não tinha sido criada, enviamos um novo link para *{e-mail}*. |
| Perfil salvo | Perfil *{nome}* salvo. |
| Perfil excluído | Perfil *{nome}* excluído. |

As mensagens citam o cadastro ou o acesso da pessoa, e não a pessoa diretamente ("Cadastro de Ana Souza criado"), para a concordância ficar correta com qualquer nome.

## Desativação e reativação

Usuários **não são excluídos**: são desativados. Registros de autoria e de atividade apontam para usuários, e apagar um deles deixaria essas referências sem dono.

**Ao desativar um usuário:**
- a situação passa a **desativado** (`inactive`), com data e autoria da desativação;
- a pessoa sai do sistema na próxima ação e não consegue mais entrar;
- links de ativação ou de troca de senha pendentes deixam de valer;
- o login e o nome de exibição continuam reservados, e ninguém mais pode usá-los;
- o nome da pessoa continua aparecendo no histórico e na autoria dos registros.

**Na lista de Usuários**, o filtro **Situação** começa em "Ativos e pendentes", então os desativados ficam escondidos. As outras opções são "Só pendentes", "Só desativados" e "Todos".

**Um usuário desativado pode ser reativado**, pelo botão **Reativar usuário** no cadastro dele, sem pedido de confirmação. Se ele já tinha criado a senha, volta a ficar ativo com a mesma senha. Se ainda estava pendente, volta a ficar pendente, e o sistema envia um novo link de ativação.

**Ninguém pode desativar a si mesmo**, para não ficar sem acesso por engano. O Admin nunca pode ser desativado.

A confirmação exibida ao desativar é: *{Nome} não vai mais conseguir entrar no sistema. Você pode reativar depois.* Se o cadastro tiver alterações não salvas, a mensagem acrescenta: *As alterações não salvas neste cadastro serão descartadas.*

Depois de desativar ou reativar, a aba volta para a lista de Usuários.

## Autoria

Todo registro guarda quem o criou, quem fez a última alteração e quando, como descrito em [02-modelo-de-dados.md](02-modelo-de-dados.md#convenções-gerais). Esses dados são preenchidos pelo servidor e não podem ser editados.

No rodapé de cada cadastro aberto, o sistema mostra essas informações com o nome de exibição de quem agiu, no horário de Brasília, em duas partes lado a lado (uma embaixo da outra em telas estreitas):

> Criado por Admin em 12/09/2026 às 09:58
> Última alteração por Bruno Lima em 20/09/2026 às 14:41

Cadastros novos, ainda não salvos, não mostram o rodapé.

Um histórico completo, que registre cada campo alterado, não faz parte desta etapa (ver [07-pendencias.md](07-pendencias.md)).

## Perfis de permissão

### Validações ao salvar

| Campo | Regra | Mensagem exibida |
|---|---|---|
| `name` | Obrigatório. | Dê um nome ao perfil. |
| `name` | Único entre todos os perfis, **incluindo o Gerenciador**, com a mesma comparação usada no nome de exibição: "Financeiro", "financeiro" e "FINANCEIRO" são o mesmo nome. | Já existe um perfil chamado *{nome existente}*. Escolha outro nome. |

### Criação e duplicação

Um perfil novo começa com todas as telas no nível `negado`.

Qualquer perfil, inclusive o Gerenciador, pode ser duplicado. A cópia recebe o nome "Cópia de *{nome original}*", a mesma descrição e os mesmos níveis. Ela só é gravada quando a pessoa salva.

### Exclusão

Perfis podem ser excluídos de verdade, mas só quando **nenhum usuário**, ativo, pendente ou desativado, tem o perfil. O Gerenciador nunca pode ser excluído.

A confirmação exibida é: **Excluir este perfil?** *O perfil {nome} será apagado. Não dá para desfazer.*

Se houver alguém com o perfil, a exclusão é recusada com a mensagem **Este perfil está em uso** e o texto *"{nomes} usa(m) este perfil. Tire o perfil dessa(s) pessoa(s) na tela Usuários e depois volte para excluir."*

### Atalho para mudar uma área inteira

Cada área da tela de Permissões tem o seletor **Mudar todas desta área para**. Ele marca o mesmo nível em todas as telas da área e avisa: *"Telas de {área} marcadas como {nível}. Salve o perfil para confirmar."* Nada é gravado até a pessoa salvar.

## Usuário sem perfil

Um usuário pode existir **sem nenhum perfil de permissão**. Isso equivale a ter nível Negado em todas as telas: a pessoa consegue entrar no sistema, mas o menu fica vazio e a tela de Início mostra *"Você ainda não tem acesso a nenhuma tela. Fale com o responsável pelo sistema."*

Esse é um caminho útil, por exemplo, para cadastrar alguém antes de decidir o que a pessoa vai poder fazer. Mas, para evitar que alguém esqueça de marcar um perfil, a tela pede confirmação ao salvar um usuário sem perfil:

> **Salvar sem nenhum perfil?** *{Nome de exibição}* vai conseguir entrar no sistema, mas não vai ver nenhuma tela até receber um perfil.

As opções são **"Salvar sem perfil"** e **"Escolher um perfil"**, que volta ao formulário.

## Combinação de perfis

Quando um usuário tem mais de um perfil e eles definem níveis diferentes para a mesma tela, **prevalece o maior nível** entre os perfis atribuídos. O cálculo é feito tela por tela. Um nível Negado num perfil nunca retira o acesso concedido por outro. Exemplos e implicações estão em [04-permissoes.md](04-permissoes.md#acesso-efetivo-com-mais-de-um-perfil).

## Visibilidade de telas e áreas

Uma tela com acesso efetivo **Negado** não aparece para a pessoa, e uma área do menu sem nenhuma tela visível também não aparece. As regras completas estão em [04-permissoes.md](04-permissoes.md#o-que-a-pessoa-vê).

## Registros do sistema

O perfil **Gerenciador** e o usuário **Admin** não podem ser alterados, desativados nem excluídos. A interface os exibe em modo somente leitura, com um aviso explicando o motivo. O conteúdo exato está em [02-modelo-de-dados.md](02-modelo-de-dados.md#registros-do-sistema).

## Várias pessoas editando ao mesmo tempo

Todo registro tem uma versão, que muda a cada alteração. Ao salvar, desativar, reativar ou excluir, o sistema confere se a versão ainda é a mesma que a pessoa abriu.

**Se a versão mudou**, alguém salvou no meio do caminho, e o salvamento é **recusado**. Nada é gravado, e o que a pessoa digitou continua na tela. A mensagem mostrada é:

> Este cadastro foi alterado por *{nome de exibição}* enquanto você editava. Recarregue para ver a versão atual.

**A pessoa escolhe entre duas opções:**
- **"Recarregar"** descarta as alterações dela e mostra a versão atual.
- **"Continuar aqui"** mantém a tela como está, para ela copiar o que precisar antes de recarregar.

**Exemplo.** Ana e Bruno abrem o cadastro do Carlos. Ana corrige o telefone e salva. Quando Bruno tenta salvar a correção do RG, o sistema recusa e avisa que Ana alterou o cadastro. Sem essa regra, o salvamento de Bruno desfaria em silêncio a correção de Ana.

A mesma regra protege quem abre o mesmo cadastro em duas abas.

O título do aviso é **Outra pessoa alterou este cadastro**.

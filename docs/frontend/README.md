# 6. Front-end

## Organização do código

O front-end é um único arquivo, `control-service-erp.html`, sem etapa de compilação. Ele contém o HTML da moldura (menu lateral, topo e barra de abas), o CSS e o JavaScript. A única dependência externa é a fonte Atkinson Hyperlegible, carregada do Google Fonts; se ela não carregar, o navegador usa a fonte padrão do sistema.

O script está dividido em seções, marcadas com comentários, nesta ordem:

| Seção | Responsabilidade |
|---|---|
| Catálogo de telas | Menu com as chaves fixas, níveis de acesso, situações de conta, prazos, mensagens e referências da página. |
| Utilidades | Funções genéricas: ícones, escape de HTML, normalização de texto, geração de ids. |
| Armazenamento do servidor simulado | Adaptadores do banco do artifact e do navegador. |
| Registro de usuário com todos os campos | Formato completo de um usuário, com valores vazios. |
| Máscaras e validações | Máscaras de CPF, CEP e telefone; validação de CPF e telefone. |
| Menu lateral, Busca de telas, Abrir e esconder a barra lateral | Comportamento do menu, da busca de telas e do botão Menu. |
| Tema claro e escuro | Alternância e memória do tema escolhido. |
| Navegação, Abas | Abertura de abas, navegação dentro da aba, botão Voltar e histórico. |
| Estrutura comum das telas, Listas, Formulários | Blocos reutilizáveis de página, tabela, campos, erros e botões. |
| Tela de usuários, Assinatura, Tela de permissões | As duas telas prontas. |
| Diálogo e mensagens rápidas | Confirmações e avisos temporários, com ação opcional ("Ver e-mail"). |
| Permissões da pessoa conectada | Nível de cada tela, telas e áreas visíveis, nome no topo. |
| Formatação para exibição | Máscaras na exibição, datas no horário de Brasília, situação da conta, rodapé de autoria. |
| Tratamento de erros do servidor | O que fazer com cada tipo de erro (ver [05](05-integracao-com-o-front.md#erros)). |
| Telas de acesso | Entrar, primeiro acesso, ativar conta, esqueci minha senha, criar senha nova. |
| Entrar e sair do sistema | Abrir o sistema depois de entrar, retomar a sessão, sair. |
| Início | Tela de boas-vindas e mensagens de "sem acesso". |
| Listas por página | Busca, filtro de situação e paginação pedidos ao servidor. |
| Caixa de e-mails de teste | Mostra os e-mails "enviados" pela simulação. |
| Servidor simulado | Todas as operações e regras do back-end (ver [05](05-integracao-com-o-front.md)). |
| Inicialização | Liga os eventos, confere a sessão e abre a tela de entrada ou o sistema. |

## Menu

O menu é gerado a partir da constante `MENU`, uma lista de áreas com o nome, o ícone e as telas. Cada tela declara a **chave** e o **nome** explicitamente (ver [04-permissoes.md](04-permissoes.md#chaves-das-telas)).

O menu mostra só as telas a que a pessoa tem acesso, e esconde as áreas sem nenhuma tela visível (ver [04-permissoes.md](04-permissoes.md#o-que-a-pessoa-vê)). A busca de telas só encontra as telas visíveis.

A busca "Buscar tela…", no topo do menu, filtra as telas enquanto a pessoa digita, ignorando acentos e maiúsculas. Digitar o nome de uma área mostra todas as telas dela. Enter abre o primeiro resultado numa aba nova.

## Abas e navegação

As regras abaixo foram definidas junto com o dono do produto e valem para todas as telas.

**Só o menu abre abas.** Cada clique numa tela do menu abre uma aba nova, mesmo que aquela tela já esteja aberta. É assim que se trabalha com a mesma tela duas vezes, por exemplo dois cadastros de usuário ao mesmo tempo.

**Dentro da aba, a navegação fica na aba.** Botões como "Novo usuário", cliques em linhas de lista, "Duplicar perfil", "Cancelar" e os links do caminho no topo trocam o conteúdo da aba atual. O nome da aba acompanha a tela exibida.

**Exceções que abrem aba nova:**
- O link "Permissões" dentro do cadastro de usuário, para não fazer a pessoa perder o que já preencheu. O texto ao lado avisa que ele abre em outra aba. O link só aparece para quem tem acesso à tela Permissões.
- Ctrl + clique, Shift + clique ou clique com o botão do meio em qualquer link ou linha de lista, como num navegador.

**Voltar.** Cada aba guarda o próprio histórico, de até 20 passos. Quando existe uma tela anterior na aba, aparece um botão com uma seta à esquerda do título. Ao passar o mouse, o botão mostra o destino, por exemplo "Voltar para Usuários". O botão Voltar do navegador tem o mesmo efeito. Depois de salvar, excluir, desativar ou reativar, o formulário sai do histórico, então voltar da lista não reabre um cadastro já concluído.

**Alterações não salvas.** Quando a pessoa edita um formulário, a aba ganha uma bolinha ao lado do nome. Fechar a aba, voltar ou sair da tela pede confirmação ("Descartar as alterações?"). Fechar ou recarregar a página também faz o navegador avisar.

**Depois de salvar, excluir, desativar ou reativar**, a aba volta para a lista, já atualizada. Listas abertas em outras abas se atualizam quando a pessoa volta a elas. Formulários abertos em outras abas não se atualizam, para nunca apagar o que está sendo digitado.

**Início.** A tela de boas-vindas só aparece quando não há nenhuma aba aberta. Nesse caso a barra de abas fica escondida. Ao fechar a última aba, o Início volta.

**Limite.** São no máximo 15 abas abertas. Ao tentar abrir mais, o sistema pede para fechar alguma primeiro.

**Teclado.** Com o foco numa aba, as setas para os lados trocam de aba, Home e End vão para a primeira e a última, e Delete fecha a aba.

## Rotas

O endereço da página acompanha a aba ativa, no formato:

```
#/{area}/{tela}                 lista da tela          #/gerenciamento/usuarios
#/{area}/{tela}/novo            cadastro novo          #/gerenciamento/usuarios/novo
#/{area}/{tela}/{id}            registro existente     #/gerenciamento/usuarios/sistema-admin
#/{area}/{tela}/novo?copiar={id}  cópia de um perfil   #/gerenciamento/permissoes/novo?copiar=sistema-gerenciador
#/                              Início (sem abas)
```

Abrir o sistema por um desses endereços abre a tela correspondente numa aba, depois que a pessoa entra.

As telas de acesso têm rotas próprias, que não abrem abas:

```
#/entrar                        tela de entrada
#/esqueci-senha                 pedir link de troca de senha
#/ativar?token={token}          criar a senha pelo link de ativação
#/redefinir-senha?token={token} criar senha nova pelo link de troca
```

## Preferências guardadas no navegador

Estas informações ficam no `localStorage` de cada pessoa e não são dados do negócio.

| Chave | Conteúdo |
|---|---|
| `control-service:theme` | `light` ou `dark`, quando a pessoa escolheu um tema. Sem essa chave, o sistema segue o tema do computador ou celular. |
| `control-service:sidebar-hidden` | `true` quando a pessoa escondeu o menu lateral no computador. |
| `control-service:abas:{id do usuário}` | Abas abertas de cada pessoa, com a rota e o histórico de cada uma, e o índice da aba ativa. São restauradas quando a mesma pessoa entra de novo. |
| `control-service:sessao` | Sessão da simulação: quem está conectado e até quando. Some ao sair. |

## Temas e cores

Todas as cores são variáveis CSS definidas no início do estilo, em três blocos: tema claro, tema escuro automático e tema escuro escolhido. Os principais tokens são `--bg` (fundo da área de conteúdo), `--surface` (cartões e topo), `--ink` e `--ink-muted` (textos), `--line` (bordas), `--side-*` (menu lateral), `--active-*` (item ativo) e `--danger*` (erros e exclusões). Para ajustar uma cor no sistema inteiro, basta mudar o token nos blocos de tema.

## Acessibilidade

O menu usa botões com `aria-expanded` para as gavetas e marca a tela atual com `aria-current`. A barra de abas segue o padrão de abas do WAI-ARIA (`tablist`, `tab`, `tabpanel`). Erros de formulário são ligados aos campos por `aria-describedby` e marcados com `aria-invalid`. Ao trocar de tela, e também nas telas de acesso, o foco vai para o título, para quem usa leitor de tela. Todas as animações são desligadas quando o sistema operacional pede movimento reduzido.

## Como adicionar uma tela

Estes passos já consideram a decisão de chaves fixas.

1. Acrescente a tela à área certa em `MENU`, com o nome e a **chave** escritos explicitamente. A mesma chave precisa existir no catálogo do servidor. A tela passa a aparecer na tela de Permissões, com nível `negado` em todos os perfis, exceto o Gerenciador.
2. Enquanto a tela não tiver uma função de exibição própria, ela mostra o aviso "Tela em construção".
3. Para dar conteúdo à tela, escreva uma função assíncrona no formato `(tab, entry, param, query)` e registre-a em `SCREENS` com a chave da tela. Ela busca os dados pelo objeto `api` e usa `canAccess(chave, nível)` para decidir quais botões mostrar. `param` é `null` para a lista, `"novo"` para um cadastro novo ou o `id` de um registro; `query` traz os parâmetros depois do `?`. As telas de Usuários e Permissões servem de modelo: `renderScreen` monta a página, `formSectionHtml` e `textFieldHtml` montam o formulário, e `bindFormBehavior` liga validação, máscaras, detecção de alterações e salvamento.
4. Se a tela gravar dados, acrescente as operações e a coleção ao servidor simulado, aplicando as regras de nível com `requireLevel`, e documente o novo registro em [02-modelo-de-dados.md](02-modelo-de-dados.md) e as operações em [05-integracao-com-o-front.md](05-integracao-com-o-front.md).

## Acesso e sessão

**Tela de entrada.** Enquanto ninguém está conectado, o sistema mostra a tela de entrada no lugar de tudo. Ela também oferece "Esqueci minha senha" e, abaixo, o aviso do ambiente de demonstração, com o botão **E-mails de teste**.

**Topo.** Com alguém conectado, o topo mostra as iniciais e o nome de exibição da pessoa, o botão de e-mails de teste e o botão **Sair**. Sair com alterações não salvas pede confirmação.

**Links dos e-mails.** Os links de ativação e de troca de senha abrem as rotas `#/ativar?token=…` e `#/redefinir-senha?token=…`. Se alguém já estiver conectado no mesmo navegador, a tela oferece voltar ao sistema ou sair para entrar com a outra conta.

**Sessão encerrada.** Se a sessão terminar (na simulação, depois de 8 horas sem nenhuma ação) ou a conta for desativada, a tela de entrada aparece **por cima** do sistema, sem fechar as abas. Se a mesma pessoa entrar de novo, ela volta exatamente onde estava, inclusive com formulários ainda não salvos. Se entrar outra pessoa, as abas são trocadas pelas dela.

**Abas guardadas por pessoa.** As abas abertas de cada pessoa ficam guardadas no navegador. Ao sair e entrar de novo, a pessoa reencontra as mesmas abas, na mesma tela.

## Cadastros

**Botões conforme o nível.** Quem é Leitor vê o cadastro em modo somente leitura, com um aviso. Quem é Editor cadastra, altera, duplica perfis e reenvia acesso. Quem é Gerenciador também desativa, reativa e exclui. Os detalhes estão em [04-permissoes.md](04-permissoes.md#o-que-a-pessoa-vê).

**Usuários.** O cadastro tem as seções Dados pessoais, Acesso ao sistema, Telefone e endereço, Contato de emergência, Assinatura e Perfis de permissão.
- Num cadastro novo, o login é sugerido enquanto a pessoa digita o nome completo, até que alguém digite um login por conta própria. O botão de salvar de um cadastro novo se chama **Cadastrar e enviar acesso**.
- A seção Acesso ao sistema mostra a situação da conta e, para contas pendentes com e-mail, o botão **Reenviar acesso**.
- CPF, telefone e CEP são exibidos com máscara, mas enviados e guardados só com dígitos.
- Perfis são opcionais. Salvar um usuário sem nenhum perfil pede confirmação, e a lista mostra "Nenhum perfil" na coluna Perfis.

**Rodapé de autoria.** Todo cadastro aberto mostra quem criou, quem fez a última alteração e quando.

**Versão.** Cada aba guarda a versão do registro aberto e a envia ao salvar. Se outra pessoa salvou antes, aparece o aviso com as opções "Recarregar" e "Continuar aqui".

**Listas.** A lista de usuários é paginada no servidor, com 10 registros por página, busca e filtro de situação. Cada aba lembra a busca, o filtro e a página de cada lista.

## O que depende do back-end real

Todas as decisões registradas em [02](02-modelo-de-dados.md), [03](03-regras-de-negocio.md) e [04](04-permissoes.md) já estão implementadas no front-end, com o servidor simulado. O único passo que falta é trocar o servidor simulado por um cliente HTTP do back-end real, como descrito em [05](05-integracao-com-o-front.md#o-servidor-simulado).

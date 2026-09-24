# 1. Visão geral

## O produto

O Control Service é um ERP para uma empresa de serviços. Ele reúne o cadastro da operação (usuários, produtos, veículos, garantias), a parte comercial (clientes, roteiros, renovações), o financeiro (contas a pagar e a receber) e relatórios.

## Quem usa

O sistema será usado por pessoas com perfis muito diferentes, e a maioria não tem muita familiaridade com tecnologia. Essa é a restrição que orienta todas as decisões de interface, e ela também vale para o que o back-end devolve ao usuário: mensagens de erro, nomes de campos e textos de confirmação precisam ser escritos em linguagem simples, sem termos técnicos.

## Princípios de design

A interface é minimalista e usa apenas tons de branco e cinza, com um tema claro e um tema escuro. A cor fica reservada para o que precisa chamar atenção, como erros e ações de exclusão. Assim, quando o sistema tiver muitos dados na tela, os alertas se destacam em vez de competir com a moldura.

A legibilidade vem antes da estética. O texto tem no mínimo 16 px e usa a fonte Atkinson Hyperlegible, criada para diferenciar bem letras parecidas como I, l e 1. Todos os contrastes atendem ao nível AA das diretrizes de acessibilidade WCAG. Os itens clicáveis têm pelo menos 44 px de altura.

O sistema só mostra o que a pessoa pode usar. Telas e áreas do menu às quais ela não tem acesso simplesmente não aparecem, em vez de aparecerem bloqueadas.

As ações se explicam sozinhas. Os botões dizem o que fazem ("Salvar usuário", "Desativar usuário", "Excluir perfil"), os campos que podem ficar em branco são marcados como "(opcional)", e toda ação que apaga dados, tira o acesso de alguém ou descarta alterações pede confirmação antes.

## Acesso

Todo uso do sistema começa pela tela de entrada, com login e senha. Ninguém cria a própria conta: cada pessoa é cadastrada por alguém com acesso à tela de Usuários e recebe por e-mail um link para criar a própria senha (ver [03-regras-de-negocio.md](03-regras-de-negocio.md#acesso-ao-sistema)).

## Estrutura de navegação

O menu lateral tem quatro **áreas**. Uma área funciona como uma gaveta: clicar nela apenas abre ou fecha a lista de telas que ela contém, e só uma gaveta fica aberta por vez. Cada **tela** abre numa aba dentro do sistema (detalhes em [06-front-end.md](06-front-end.md)).

Cada pessoa vê apenas as telas a que tem acesso, e uma área sem nenhuma tela acessível não aparece no menu (ver [04-permissoes.md](04-permissoes.md#o-que-a-pessoa-vê)).

Cada tela tem uma **chave** fixa, no formato `area/tela`, que identifica a tela nas permissões e nos endereços do sistema. A chave não muda quando o nome da tela muda (ver [04-permissoes.md](04-permissoes.md#chaves-das-telas)).

### Gerenciamento

| Tela | Chave | Situação |
|---|---|---|
| Usuários | `gerenciamento/usuarios` | Pronta |
| Permissões | `gerenciamento/permissoes` | Pronta |
| Perfis CNPJ | `gerenciamento/perfis-cnpj` | Em construção |
| Naturezas de Serviço | `gerenciamento/naturezas-de-servico` | Em construção |
| Objetos de Serviço | `gerenciamento/objetos-de-servico` | Em construção |
| Produtos | `gerenciamento/produtos` | Em construção |
| Garantias | `gerenciamento/garantias` | Em construção |
| Formas de Pagamento | `gerenciamento/formas-de-pagamento` | Em construção |
| Veículos | `gerenciamento/veiculos` | Em construção |

### Comercial

| Tela | Chave | Situação |
|---|---|---|
| Clientes | `comercial/clientes` | Em construção |
| Roteiro Diário | `comercial/roteiro-diario` | Em construção |
| Roteiro Mensal | `comercial/roteiro-mensal` | Em construção |
| Acompanhamento | `comercial/acompanhamento` | Em construção |
| Renovações | `comercial/renovacoes` | Em construção |

### Financeiro

| Tela | Chave | Situação |
|---|---|---|
| Contas a Receber | `financeiro/contas-a-receber` | Em construção |
| Contas a Pagar | `financeiro/contas-a-pagar` | Em construção |

### Relatórios

| Tela | Chave | Situação |
|---|---|---|
| Relatório de Vendas | `relatorios/relatorio-de-vendas` | Em construção |
| RAAE | `relatorios/raae` | Em construção |
| Incongruências | `relatorios/incongruencias` | Em construção |
| Custo x Faturamento | `relatorios/custo-x-faturamento` | Em construção |

A mesma lista está em [catalogo-de-telas.json](catalogo-de-telas.json), pronta para ser lida por código.

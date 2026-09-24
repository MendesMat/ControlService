# 4. Permissões

## Conceito

Permissões são organizadas em **perfis**. Um perfil define, para cada tela do sistema, um **nível de acesso**. Neste documento, "tela" é cada item de submenu do menu lateral, como Usuários ou Contas a Pagar, e "área" é cada grupo do menu, como Gerenciamento ou Financeiro.

Cada usuário recebe nenhum, um ou mais perfis. O acesso final dele a cada tela vem da combinação desses perfis. Um usuário sem nenhum perfil tem nível Negado em todas as telas.

## Níveis de acesso

Os níveis são ordenados: cada um inclui tudo o que o anterior permite.

| Ordem | `id` | Nome na tela | O que permite |
|---|---|---|---|
| 0 | `negado` | Negado | Não vê a tela. |
| 1 | `leitor` | Leitor | Vê as informações, mas não altera nada. |
| 2 | `editor` | Editor | Vê, cadastra e altera informações. |
| 3 | `gerenciador` | Gerenciador | Faz tudo, inclusive excluir e desativar registros. |

Esta é a regra que liga cada nível às ações das telas. Ela **já está aplicada** no sistema, pelo servidor simulado e pelos botões das telas, e foi confirmada em 24/09/2026 (ADR-0020). Cada tela nova segue esta tabela, a menos que documente uma exceção.

| Ação | Nível mínimo |
|---|---|
| Abrir a tela, listar e ver registros | Leitor |
| Cadastrar, alterar, duplicar perfil, reenviar acesso | Editor |
| Excluir perfil, desativar e reativar usuário | Gerenciador |

**Exceção.** A lista de perfis também é liberada para quem tem nível Leitor na tela **Usuários**, mesmo sem acesso à tela Permissões. É ela que alimenta a escolha de perfis no cadastro de usuário.

## Acesso efetivo com mais de um perfil

Quando um usuário tem vários perfis, **vale o nível mais alto de cada tela**. O cálculo é feito tela por tela, de forma independente.

Exemplo: Carla tem os perfis Vendedor e Financeiro.

| Tela | Vendedor | Financeiro | Acesso da Carla |
|---|---|---|---|
| `comercial/clientes` | editor | leitor | **editor** |
| `financeiro/contas-a-receber` | leitor | gerenciador | **gerenciador** |
| `gerenciamento/usuarios` | negado | negado | **negado** |

Em pseudocódigo:

```
acessoEfetivo(usuario, tela) =
  maior nível entre { nivelDoPerfil(perfil, tela) para cada perfil em usuario.profileIds }

nivelDoPerfil(perfil, tela) =
  "gerenciador"                     se perfil for o Gerenciador
  o level do item com screen == tela, se existir
  "negado"                          caso contrário
```

Se `profileIds` estiver vazia, o maior nível entre nenhum perfil é Negado, e isso vale para todas as telas. Um id em `profileIds` que não corresponde a nenhum perfil existente é ignorado.

### O que essa regra implica

**Negado não é uma proibição, é a ausência de permissão.** Se um perfil nega uma tela e outro perfil da mesma pessoa libera, prevalece o que libera. Não existe forma de usar um perfil para retirar um acesso que outro perfil concede.

**Para restringir alguém, retire o perfil que concede o acesso.** Acrescentar um perfil a uma pessoa nunca reduz o que ela já podia fazer. Só retirar um perfil, ou baixar um nível dentro dele, reduz.

**O Gerenciador domina qualquer combinação.** Quem tem o perfil Gerenciador tem nível Gerenciador em todas as telas, qualquer que seja o outro perfil.

**Mudar um perfil afeta todas as pessoas que o usam.** Baixar um nível num perfil só reduz o acesso de quem não recebe um nível maior por outro perfil.

## O que a pessoa vê

O sistema só mostra o que a pessoa pode usar.

**Telas.** Uma tela cujo acesso efetivo é **Negado** não aparece no menu lateral.

**Áreas.** Uma área em que **todas** as telas são Negado também não aparece. Por exemplo: se Carla não tem acesso a Contas a Receber nem a Contas a Pagar, a área Financeiro some do menu dela.

**Busca de telas.** A busca "Buscar tela…", no topo do menu, só encontra telas que a pessoa pode ver.

**Link direto ou aba antiga.** Se a pessoa abrir uma tela sem acesso por um link salvo ou por uma aba restaurada de outra sessão, o sistema mostra *"Você não tem acesso a esta tela. Se precisar dela, fale com o responsável pelo sistema."* Nenhum dado da tela é carregado, porque o servidor recusa o pedido.

**Nenhum acesso.** Se a pessoa não tiver acesso a nenhuma tela, seja por não ter nenhum perfil ou porque todos os perfis dela negam tudo, o menu fica vazio e a tela de Início mostra: *"Você ainda não tem acesso a nenhuma tela. Fale com o responsável pelo sistema."*

**Dentro da tela.** Os botões seguem a regra de ações da seção [Níveis de acesso](#níveis-de-acesso):
- Um **Leitor** vê os cadastros em modo somente leitura, com o aviso *"Você pode consultar este cadastro, mas não alterar. Para mudar alguma informação, fale com quem tem acesso de Editor nesta tela."* Ele não vê os botões "Novo", "Salvar", "Duplicar perfil" e "Reenviar acesso", e o botão "Cancelar" vira "Voltar para a lista".
- Um **Editor** não vê os botões "Excluir perfil", "Desativar usuário" e "Reativar usuário".
- Ninguém vê o botão "Desativar usuário" no próprio cadastro.

**Quem decide é o servidor.** Esconder telas e botões é uma ajuda para a pessoa. A barreira real é o servidor, que recusa qualquer pedido acima do nível dela, mesmo que alguém tente contornar a interface.

### Quando as permissões mudam

Uma mudança de perfil vale no servidor a partir da próxima ação da pessoa, sem ela precisar sair do sistema.

O menu da pessoa é atualizado quando ela entra no sistema, quando recarrega a página e sempre que ela mesma salva, desativa, reativa ou exclui um cadastro. Abrir uma aba nova **não** atualiza o menu. Se a pessoa tentar usar algo que perdeu nesse meio-tempo, o servidor recusa, e o sistema atualiza o menu e mostra a mensagem de falta de acesso.

## Telas novas

Quando uma tela é criada, ela passa a aparecer na tela de Permissões. Todos os perfis existentes ficam com nível Negado nela até alguém mudar e salvar o perfil. Por isso, uma tela nova não aparece para ninguém além de quem tem o perfil Gerenciador. Isso é consequência da regra "sem item, vale Negado" e não exige nenhuma migração de dados.

## Chaves das telas

### O que é a chave

Cada tela tem uma **chave**: o identificador que o sistema usa para gravar as permissões. A chave é diferente do **nome** que aparece no menu.

| O quê | Exemplo | Para quem | Pode mudar? |
|---|---|---|---|
| Chave | `relatorios/relatorio-de-vendas` | O sistema | **Nunca** |
| Nome | "Relatório de Vendas" | As pessoas | Sempre que for preciso |

### Por que a chave não é gerada a partir do nome

Na primeira versão do front-end, a chave era calculada a partir do nome. Parece prático, mas tem um problema grave.

**Exemplo.** Suponha que a empresa decida renomear "Relatório de Vendas" para "Vendas por Período".

- **Com a chave gerada pelo nome:** a chave muda sozinha, de `relatorios/relatorio-de-vendas` para `relatorios/vendas-por-periodo`. Todos os perfis têm níveis gravados com a chave antiga, e nenhum tem a nova. Pela regra "sem item, vale Negado", **a tela some para todo mundo**, menos para o Gerenciador. Ninguém é avisado, e a correção exige reconfigurar os perfis um por um.
- **Com a chave fixa:** a chave continua `relatorios/relatorio-de-vendas`. Só o nome exibido muda, e todas as permissões continuam valendo.

O mesmo acontece com mudanças pequenas, como corrigir um acento ("Relatorio" para "Relatório") ou trocar "x" por "vs." em "Custo x Faturamento".

Essa separação entre **identidade** e **apresentação** é uma prática comum no mercado. É o mesmo motivo pelo qual um produto tem um código interno que não muda quando a descrição muda, e um sistema traduzido usa chaves como `menu.relatorios.vendas` em vez do texto em português.

### Regras

- **Definida no código.** A chave é escrita uma vez, quando a tela é criada, e nunca é recalculada.
- **Formato:** `area/tela`, em minúsculas, sem acentos, com hífens no lugar de espaços. O formato é só uma convenção de legibilidade; depois de criada, a chave não acompanha mudanças no nome.
- **Nunca reaproveitada.** Uma tela removida tem a chave aposentada, e essa chave não volta a ser usada por outra tela.
- **Valores preservados.** As chaves atuais foram mantidas exatamente como estavam, então nenhuma permissão gravada precisa ser migrada.

### Lista de chaves

| Área | Tela | Chave |
|---|---|---|
| Gerenciamento | Usuários | `gerenciamento/usuarios` |
| Gerenciamento | Permissões | `gerenciamento/permissoes` |
| Gerenciamento | Perfis CNPJ | `gerenciamento/perfis-cnpj` |
| Gerenciamento | Naturezas de Serviço | `gerenciamento/naturezas-de-servico` |
| Gerenciamento | Objetos de Serviço | `gerenciamento/objetos-de-servico` |
| Gerenciamento | Produtos | `gerenciamento/produtos` |
| Gerenciamento | Garantias | `gerenciamento/garantias` |
| Gerenciamento | Formas de Pagamento | `gerenciamento/formas-de-pagamento` |
| Gerenciamento | Veículos | `gerenciamento/veiculos` |
| Comercial | Clientes | `comercial/clientes` |
| Comercial | Roteiro Diário | `comercial/roteiro-diario` |
| Comercial | Roteiro Mensal | `comercial/roteiro-mensal` |
| Comercial | Acompanhamento | `comercial/acompanhamento` |
| Comercial | Renovações | `comercial/renovacoes` |
| Financeiro | Contas a Receber | `financeiro/contas-a-receber` |
| Financeiro | Contas a Pagar | `financeiro/contas-a-pagar` |
| Relatórios | Relatório de Vendas | `relatorios/relatorio-de-vendas` |
| Relatórios | RAAE | `relatorios/raae` |
| Relatórios | Incongruências | `relatorios/incongruencias` |
| Relatórios | Custo x Faturamento | `relatorios/custo-x-faturamento` |

A mesma lista, com os níveis, está em [catalogo-de-telas.json](catalogo-de-telas.json).

## Situação no front-end

As regras deste documento já estão aplicadas. O servidor simulado calcula o acesso efetivo e recusa qualquer operação acima do nível da pessoa. O front-end esconde as telas e as áreas negadas, mostra a mensagem de falta de acesso e exibe os botões conforme o nível. As chaves das telas estão declaradas explicitamente no menu.

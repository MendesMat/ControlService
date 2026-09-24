# 7. Pendências e decisões em aberto

Esta lista reúne o que ainda não foi decidido e afeta o back-end ou a integração com ele. As pendências já resolvidas ficam registradas no fim, com o lugar onde a decisão está descrita.

## Em aberto

### Dados

**Onde guardar a assinatura.** Hoje a imagem fica dentro do registro do usuário, como texto de até cerca de 180.000 caracteres. O ADR-0018 propõe um armazenamento de arquivos à parte, com o registro guardando só a referência. A assinatura ficou fora da primeira etapa do back-end e precisa ser decidida antes de ela entrar.

**Histórico completo de alterações.** A autoria registra quem criou e quem fez a última alteração, mas não o que mudou em cada alteração. Um histórico campo a campo ficou para uma etapa futura.

**Atualização entre pessoas.** Uma lista aberta não se atualiza sozinha quando outra pessoa altera um registro. Ela só se atualiza quando a página é recarregada, quando a lista é reaberta ou quando a própria pessoa salva algo. A proteção contra salvamentos simultâneos já está decidida; a atualização automática das listas, não.

### Acesso

**Provedor de e-mail em produção.** O sistema passa a enviar e-mails (ver ADR-0030). O serviço usado no ambiente público ainda não foi escolhido.

**Acesso de demonstração no ambiente público.** No servidor simulado, a tela de entrada mostra o login `admin` e a senha inicial `admin123`, e os e-mails ficam na caixa de E-mails de teste. No back-end real, publicado para avaliadores, falta decidir como alguém entra sem receber um link de ativação: por exemplo, um usuário de demonstração com senha publicada no README, perfis limitados e dados restaurados periodicamente.

**Dados pessoais do Admin.** O Admin não pode ser alterado. Falta decidir se, mesmo assim, será possível preencher dados como telefone e assinatura, ou se ele continua sendo apenas uma conta técnica. Até lá, ele é uma conta técnica.

**Onde o front-end é servido.** O cookie de renovação da sessão só funciona se a página estiver em `localhost` ou no mesmo site da API (ver [05](05-integracao-com-o-front.md#sessão)). Falta decidir se o HTML será servido pela própria API ou por outro endereço, o que define a configuração de CORS.

### Conteúdo

**RAAE.** O nome da tela em Relatórios foi mantido como informado. O significado da sigla e o conteúdo do relatório ainda precisam ser descritos.

**Telas em construção.** Das 20 telas do menu, 18 ainda não têm campos definidos. Cada uma vai acrescentar registros a [02-modelo-de-dados.md](02-modelo-de-dados.md).

**Perfis num cadastro de usuário já aberto.** Um perfil criado em outra aba só aparece nas opções de um cadastro de usuário quando esse cadastro é reaberto. O comportamento foi mantido para não apagar o que está sendo digitado.

## Resolvidas

| Pendência | Decisão | Onde está descrita |
|---|---|---|
| Não existe login | Campo `login` único, e-mail obrigatório e não único, ativação por link enviado ao e-mail, troca de senha pelo login. | [03](03-regras-de-negocio.md#acesso-ao-sistema), ADR-0019, ADR-0030 |
| CPF, telefone e CEP gravados com máscara | São informativos, gravados só com dígitos e formatados na exibição. O CPF não precisa ser único. | [02](02-modelo-de-dados.md#convenções-gerais), [03](03-regras-de-negocio.md#formato-de-cpf-telefone-e-cep), ADR-0006 |
| Chave da tela gerada pelo nome | Chaves fixas, declaradas no código, com os valores atuais preservados. | [04](04-permissoes.md#chaves-das-telas), ADR-0021 |
| Sem autoria nem data de criação | Quem criou, quem alterou e quando, preenchidos pelo servidor e exibidos no rodapé dos cadastros. | [03](03-regras-de-negocio.md#autoria), ADR-0015 |
| Exclusão definitiva de usuários | Usuários são desativados e podem ser reativados. Perfis sem uso continuam podendo ser excluídos. | [03](03-regras-de-negocio.md#desativação-e-reativação), ADR-0016 |
| Duas pessoas salvando o mesmo registro | Controle de versão: o segundo salvamento é recusado, com o nome de quem alterou. | [03](03-regras-de-negocio.md#várias-pessoas-editando-ao-mesmo-tempo), ADR-0014 |
| Lista completa carregada de uma vez | Listas por página, com busca e ordenação no servidor. Listas pequenas de referência continuam inteiras. | [05](05-integracao-com-o-front.md#carregamento-por-página), ADR-0017 |
| Permissões não são aplicadas | Telas negadas e áreas sem telas visíveis não aparecem, e o servidor recusa ações acima do nível. | [04](04-permissoes.md#o-que-a-pessoa-vê), ADR-0020 |
| Níveis de telas removidas ficam órfãos | Telas removidas têm a chave aposentada, e os níveis dela são limpos. | [04](04-permissoes.md#regras), ADR-0021 |
| O que cada nível permite | Tabela confirmada: Leitor vê; Editor cadastra, altera, duplica perfis e reenvia acesso; Gerenciador também exclui, desativa e reativa. Leitor de Usuários vê a assinatura. | [04](04-permissoes.md#níveis-de-acesso), ADR-0020 |
| Formato dos identificadores | UUID v7 gerados pelo servidor; GUIDs fixos para o Admin e o Gerenciador. | [02](02-modelo-de-dados.md#registros-do-sistema), ADR-0012 |
| Estrutura do back-end | Quatro projetos por camada (Domain, Application, Infrastructure, API), com pastas por funcionalidade, e Minimal APIs em todo o sistema. | ADR-0005, ADR-0003 |
| Saber na hora se o e-mail de ativação saiu | O usuário é gravado primeiro, como pendente, e o e-mail é enviado logo em seguida, na mesma operação. | [03](03-regras-de-negocio.md#cadastro-e-ativação), ADR-0031 |
| Duração da sessão no back-end real | 8 horas desde a última ação, com a conta conferida a cada pedido. | [05](05-integracao-com-o-front.md#sessão), ADR-0032 |
| Login ou nome repetido: erro de campo ou conflito | Erro de validação (400), com a mensagem embaixo do campo. | [05](05-integracao-com-o-front.md#formato-da-resposta-de-erro), ADR-0009 |

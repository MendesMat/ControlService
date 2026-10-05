---
name: revisar-issue
description: Revisa a implementação de uma issue do Control Service antes do merge - compara o pull request com a issue, os critérios de aceite, os testes numerados, as regras de negócio e as decisões do projeto, e relata os problemas por severidade, de forma didática. Use quando o dono pedir /revisar-issue ou "revise a issue", de preferência numa conversa nova.
argument-hint: "<número da issue>"
---

# /revisar-issue

Terceiro dos três comandos (`AGENTS.md`, "Workflow: three commands"). Funciona melhor numa **conversa nova**: quem escreveu o código tende a defender o que escreveu.

Issue: `$ARGUMENTS`

## Passos

1. **Leia a issue** (`gh issue view <n> --comments`) e encontre o pull request que a fecha (`gh pr list --state all --search "<n> in:body"`; confira o `Closes #<n>`). Sem pull request, pare e diga.
2. **Leia o pull request:** `gh pr view <pr>`, `gh pr diff <pr>` e o estado do CI.
3. **Leia os documentos** que a issue cita. A revisão é contra a regra escrita, não contra a lembrança dela.
4. **Traga o código:** `gh pr checkout <pr>` e rode `powershell.exe -NoProfile -File check.ps1` em `backend/ControlService`.
5. **Verifique** a lista abaixo.
6. **Entregue o relatório** no formato abaixo e pare. Não altere código durante a revisão.
7. **Depois que o dono escolher** o que aplicar: commits novos na mesma branch, com o teste antes do código; rode o `check.ps1`, envie, espere o CI e relate só o que mudou, com o resultado dos testes afetados.

## O que verificar

- **Requisitos:** o código faz o que cada item do Escopo pede, e nada além dele.
- **Critérios de aceite:** um por um, atendido ou não.
- **Testes:** cada ID da issue tem um método com o comentário `// #<n>-Txx`, e o teste **afirma o resultado esperado da issue**, não só "não deu erro". Confira se o que o relatório de execução disse é verdade.
- **Regras de negócio:** o comportamento bate com a regra citada, inclusive nos casos de erro. As mensagens são iguais às do documento, caractere por caractere.
- **Bugs:** valores nulos e vazios, limites, ordem das verificações, concorrência, o caminho de erro de cada chamada.
- **Segurança:** permissão declarada em cada endpoint, dados pessoais ou senhas em logs e respostas, entrada não validada, informação que revela se um login existe.
- **Arquitetura:** dependência no sentido errado entre as camadas, regra de negócio fora do domínio, fuga do padrão da funcionalidade vizinha sem motivo.
- **Complexidade desnecessária:** abstração, interface, camada ou padrão que a issue não pediu e que o `AGENTS.md` ("Proportionality") não justifica.
- **Duplicação** e **manutenção:** código repetido, nomes que enganam, comentários que apontam para algo que o leitor não consegue abrir.
- **Código e documentação:** divergências. Quando o documento pode ser o lado errado, pergunte; nunca ajuste `docs/` em silêncio.
- **Entrega:** identidade dos commits, nenhuma linha de atribuição, um pull request por issue.

Uma solução mais sofisticada não é melhor por ser mais sofisticada.

## Severidade

| Severidade | Quando |
|---|---|
| **Bloqueante** | Bug, falha de segurança, regra de negócio violada, critério de aceite não atendido, teste que não testa o que diz. Não deve ir para a `main` |
| **Importante** | Não quebra nada hoje, mas custa caro depois: complexidade sem necessidade, duplicação, divergência entre código e documentação, caminho de erro sem teste |
| **Sugestão** | Melhoria opcional. Pode virar uma issue futura |

## Relatório

Em português, do mais grave para o menos grave.

```markdown
# Issue #<n> — <título>: revisão

Pull request: <link> · CI: <estado> · `check.ps1` local: <resultado>

**Veredito:** <"Pode ir para a main" ou "Corrigir antes do merge">, em uma frase.

## Critérios de aceite

| Critério | Atendido? |
|---|---|
| <critério da issue> | Sim / Não: <motivo> |

## Testes

| ID | Confere com a issue? | Observação |
|---|---|---|
| T01 | Sim | |
| T02 | Não | Afirma só o status; a issue pede também a mensagem |

## Achados

### 1. [Bloqueante] <título curto>

`<arquivo>:<linha>` · <ID da regra, se houver>

**Problema.** <O que está errado.>

**Por que isso importa.** <A consequência prática.>

**Opções.** A) <...> B) <...>

**Recomendação.** <Qual opção, e por quê, para este projeto.>

**Impacto.** <O que muda no código, na arquitetura ou na manutenção.>

## O que está bom

<2 ou 3 pontos concretos, para o dono saber o que repetir.>

## Decisões para você

<Lista numerada: os achados que dependem de uma escolha sua.>
```

Sem achados em uma severidade, diga "nenhum". Não invente problema para preencher o relatório.

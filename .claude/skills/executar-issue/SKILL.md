---
name: executar-issue
description: Executa uma issue já levantada do Control Service - implementa só o escopo da issue, com o teste antes do código, sem pausas para aprovação, roda todos os testes numerados e a suíte completa, abre o pull request e entrega o relatório final com o resultado de cada teste. Use quando o dono pedir /executar-issue ou "execute a issue".
argument-hint: "<número da issue>"
---

# /executar-issue

Segundo dos três comandos (`AGENTS.md`, "Workflow: three commands"). A execução é **contínua**: não peça aprovação teste a teste. O dono revisa depois, pelo relatório final, então o relatório precisa permitir conferir cada teste sozinho.

Issue: `$ARGUMENTS`

Se o corpo da issue não tem a seção **Testes**, pare e diga: o `/levantar-issue` vem primeiro.

## Passos

1. **Leia a issue** (`gh issue view <n> --comments`) e os documentos que ela cita.
2. **Releia o código relacionado** antes de modificar: ele pode ter mudado desde o levantamento.
3. **Prepare o git:** identidade, `main` atualizada e uma branch nova (`docs/agents/workflows/git-and-pull-requests.md`, passos 1 e 2).
4. **Para cada teste da lista, na ordem,** um ciclo completo:
   - **Red:** escreva o teste, com o comentário `// #<n>-T01` na linha acima dele. Rode filtrado e confirme que falha **pelo motivo esperado**. Um erro de compilação por um tipo que ainda não existe vale; um erro de digitação no teste não vale.
   - **Green:** o menor código que faz o teste passar.
   - **Refactor:** com os testes verdes, remova duplicação e melhore nomes. "Nada a refatorar" é um resultado válido.
   - Faça o commit ao fim de um ciclo verde, ou de poucos ciclos do mesmo comportamento.
   - Anote para o relatório: o método de teste, o arquivo e se houve Red.
5. **Rode tudo:** `powershell.exe -NoProfile -File check.ps1` em `backend/ControlService`. Corrija o que quebrar, inclusive em testes antigos.
6. **Confira de ponta a ponta** quando a issue entrega um endpoint ou uma tela: suba o AppHost, exercite e pare.
7. **Atualize `docs/`** no mesmo pull request (`docs/agents/guides/documentation.md`).
8. **Abra o pull request** com `Closes #<n>`, o título em inglês no formato Conventional Commits e a lista de testes com o resultado de cada um. Espere o CI.
9. **Entregue o relatório final** e pare. Quem faz o merge é o dono.

## Quando parar no meio

Só nestes casos, explicando nos 6 pontos:

- Uma regra de negócio que nem a issue nem `docs/` respondem.
- A solução exigiria algo que a issue não autoriza: pacote, abstração, camada, padrão, migration, mudança de contrato, arquivo a apagar.
- A arquitetura atual torna o requisito complexo sem necessidade.
- O código real contradiz a issue de um jeito que muda o escopo.

**Um teste que falha não é motivo para parar:** corrija. Depois de duas tentativas sem sucesso no mesmo problema, pare de tentar por tentativa e erro e procure a causa raiz. Se continuar sem solução, siga com os outros testes e relate esse como FALHOU, com o que você tentou.

## Regras dos testes

- Um teste da lista é um método de teste (ou um `[Theory]`, para vários exemplos do mesmo comportamento), com o ID no comentário.
- O nome do método é em inglês e descreve o comportamento: `Wrong_password_is_refused`.
- As mensagens são copiadas do documento da funcionalidade, nunca escritas de memória.
- Use fakes escritos à mão em vez de mocks, um `TimeProvider` falso em vez do relógio real e só dados fictícios.
- **Teste que passa de primeira** (outro ciclo já cobriu a regra): confira que ele falharia se a regra quebrasse, mantenha e marque "sem Red" no relatório. Nunca invente um Red artificial.
- **Teste novo descoberto na execução:** recebe o próximo número livre e aparece como "adicionado na execução", com o motivo.
- **Teste da issue que precisou mudar:** mantém o ID, e o relatório diz o que mudou e por quê.
- **Teste que não faz sentido ou não pôde ser feito:** PULADO, com o motivo. Nunca apague em silêncio, e nunca enfraqueça um teste para ele passar.
- **Correção de bug:** o primeiro teste reproduz o bug e falha pelo motivo descrito no relato.

## Relatório final

Em português, neste formato e nesta ordem. Os IDs e os textos dos testes são os da issue, sem alteração.

```markdown
# Issue #<n> — <título>: relatório de execução

Pull request: <link> · CI: <verde ou vermelho> · Branch: `<nome>`

## Implementação

<3 a 6 tópicos: o que mudou, por camada ou por arquivo.>

## Testes

Resumo: <x> de <y> passaram, <f> falharam, <p> pulados.

| ID | Resultado | Teste | Método de teste |
|---|---|---|---|
| T01 | PASSOU | Deve rejeitar login com senha incorreta | `SignInTests.Wrong_password_is_refused` |
| T02 | FALHOU | ... | ... |
| T03 | PULADO | ... | — |

- **Falhas:** <ID: o motivo e o que foi tentado>, ou "nenhuma".
- **Pulados:** <ID: o motivo>, ou "nenhum".
- **Correções feitas depois de uma falha:** <ID: o que estava errado e o que mudou>, ou "nenhuma".
- **Testes alterados em relação à issue:** <ID: o que mudou e por quê>, ou "nenhum".
- **Testes adicionados na execução:** <ID: o motivo>, ou "nenhum".
- **Sem Red:** <IDs>, ou "nenhum".

## Regressões

Suíte completa: <total> testes, <resultado>. Testes antigos alterados: <quais e por quê>, ou "nenhum".

## Arquitetura

Classes, interfaces, camadas, padrões ou pacotes novos: <cada um com o porquê>, ou "nenhum: seguiu o padrão de <funcionalidade>".

## Observações

Problemas fora do escopo, que não foram alterados: <lista>, ou "nenhum".

Próximo passo: `/revisar-issue <n>`, numa conversa nova.
```

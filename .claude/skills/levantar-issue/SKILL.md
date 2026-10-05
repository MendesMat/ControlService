---
name: levantar-issue
description: Levanta uma issue do Control Service - analisa a documentação e o código, define escopo, dependências, riscos, a lista numerada de testes e os critérios de aceite, e grava tudo no corpo da issue no GitHub depois que o dono aprova o rascunho. Use quando o dono pedir /levantar-issue, "levante a issue", ou quiser planejar uma issue antes de qualquer código.
argument-hint: "<número da issue, ou a descrição de uma issue nova>"
---

# /levantar-issue

Primeiro dos três comandos (`AGENTS.md`, "Workflow: three commands"). O resultado é uma issue que outra pessoa consegue executar sem perguntar nada: **o que será feito, por que será feito e como saberemos que terminou**.

Issue: `$ARGUMENTS`

- Sem argumento: a menor issue aberta do milestone atual.
- Texto em vez de número: é a descrição de uma issue nova, criada no passo 8.

## Passos

1. **Leia a issue** e os comentários: `gh issue view <n> --comments`.
2. **Leia a documentação que ela toca:** o documento da funcionalidade em `docs/product/features/`, `docs/product/conventions.md`, `docs/api/conventions.md`, as decisões de arquitetura citadas e `docs/product/open-questions.md`. As decisões de escopo do `AGENTS.md` valem mais que ADRs e issues antigas.
3. **Leia o código relacionado:** os arquivos que a issue vai tocar e a funcionalidade parecida mais próxima já pronta, que é o padrão a seguir.
4. **Levante:**
   - o requisito ou o problema, em uma frase;
   - o menor escopo que o resolve;
   - as dependências (outras issues, decisões, documentos);
   - os riscos (o que pode surpreender na execução);
   - as lacunas: regra sem resposta em `docs/`, caso de erro sem mensagem literal, contradição entre documentos ou entre documento e código, ID de regra duplicado.
5. **Aplique a proporcionalidade** (`AGENTS.md`). Tudo o que for abstração, camada, padrão, pacote, migration ou mudança de contrato entra no rascunho como decisão explicada nos 6 pontos. Melhoria que a issue não precisa vai para "Recomendações futuras".
6. **Monte a lista de testes** (regras abaixo).
7. **Apresente ao dono, em português, e espere:**
   - o que você leu;
   - cada lacuna e cada decisão, nos 6 pontos, com a sua recomendação;
   - o rascunho completo da issue, no modelo abaixo.
8. **Depois da aprovação, grave:** `gh issue edit <n> --body-file <arquivo>` (ou `gh issue create --title ... --body-file <arquivo> --milestone ...` para uma issue nova). Confirme com o link e pare.

## Lista de testes

- **Identificação estável:** `T01`, `T02`, ... dentro da issue; `#<n>-T01` fora dela. Depois de aprovada, a lista não é renumerada.
- **Um comportamento por teste**, descrito em português e começando por "Deve": *Deve rejeitar login com senha incorreta*.
- **Ordem:** do mais simples ao mais complexo, de dentro para fora (domínio, depois a operação pelo endpoint).
- **Resultado esperado exato:** o valor, o status, o `code` e a mensagem literal. Uma descrição vaga vira adivinhação na execução.
- **Uma camada por comportamento**, a mais barata que prova a regra:

  | Camada | O que entra |
  |---|---|
  | Domínio | Regra de um Value Object ou de um agregado: validação, transição de estado, cálculo |
  | API (integração, PostgreSQL real) | Cada operação: sucesso, erros de validação, um caminho permitido e um negado para o nível mínimo, conflito de versão quando houver, recusas de negócio, unicidade no banco |
  | Aplicação (handler com fakes) | Só quando a lógica não é alcançável de forma razoável por HTTP: passagem do tempo, falha de uma dependência |

- **Não repita** o mesmo comportamento em duas camadas.
- **Peças sem teste próprio** (registro de serviços, configuração, migration): liste cada uma com o ID do teste que a cobre.

## Modelo do corpo da issue

O título fica em inglês, curto e no imperativo, porque dá origem ao título do pull request. O corpo é em português.

```markdown
## Contexto

<2 a 4 frases: o requisito ou o problema, e por que agora.>

## Escopo

- <item a implementar, com o ID da regra: USR-06>
- <pacote, migration ou mudança de contrato que esta issue autoriza, se houver>

## Fora do escopo

- <item> — <onde fica: issue #N ou "recomendação futura">

## Dependências

- <issue, decisão ou documento de que esta issue depende>

## Riscos

- <o que pode surpreender> — <como perceber>

## Decisões do levantamento

- <decisão tomada pelo dono> — <motivo, em uma frase>

## Testes

| ID | Teste | Camada | Regra | Resultado esperado |
|---|---|---|---|---|
| T01 | Deve rejeitar login com senha incorreta | API | AUTH-08 | 401 `invalid_credentials`, "Login ou senha incorretos." |
| T02 | ... | | | |

Sem teste próprio: <peça> — coberta por <ID>.

## Critérios de aceite

- [ ] Os testes T01 a Tnn passam.
- [ ] `check.ps1` termina sem avisos, com todos os testes do projeto verdes.
- [ ] <critério observável, específico desta issue>
- [ ] `docs/` atualizado no mesmo pull request: <arquivos>.

## Recomendações futuras

- <melhoria que não entra nesta issue, e por quê>
```

## Regras

- Não escreva código nem testes neste comando.
- Não grave a issue antes de o dono aprovar o rascunho.
- Não invente regra de negócio nem mensagem: pergunte. Uma mensagem nova é aprovada pelo dono com o texto exato e entra no documento da funcionalidade, com um ID de regra novo, no pull request da execução.
- Não amplie o escopo sem justificativa.
- O que o `AGENTS.md` lista em "Ask the owner first" precisa aparecer em **Escopo**: é a autorização que o `/executar-issue` vai usar para não parar no meio.

# ADR-004: Motor Interno de Templates Dinâmicos para Geração de Documentos

**Data:** 15/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O Control Service ERP precisa gerar documentos operacionais em formato PDF para múltiplos fluxos de negócio: **contratos de prestação de serviço**, **certificados de garantia** e **ordens de serviço**. Esses documentos apresentam três características que tornam a solução técnica não-trivial:

1. **Personalização por CNPJ e Natureza de Serviço:** Um contrato de dedetização emitido pelo CNPJ A possui cláusulas e formatação distintas de um contrato de higienização emitido pelo CNPJ B. Cada combinação CNPJ × Natureza de Serviço pode exigir um template próprio.

2. **Alta frequência de mudanças textuais por razões não-técnicas:** As cláusulas contratuais estão sujeitas a ajustes jurídicos recorrentes (atualizações de legislação, decisões de renegociação com clientes, correções de redação). Essas mudanças são de responsabilidade do setor jurídico/gerência da empresa — não do desenvolvedor. Exigir um deploy de código para cada ajuste textual é operacionalmente inviável e representa um acoplamento indesejado entre o ciclo de negócio e o ciclo de engenharia.

3. **Dados dinâmicos injetados em tempo de execução:** Os documentos contêm variáveis (nome do cliente, endereço, data do serviço, prazo de garantia, produtos utilizados) que devem ser substituídas por valores reais no momento da emissão.

O problema central a resolver é: **como permitir que a gestão da empresa modifique o conteúdo dos documentos de forma autônoma, sem intervenção técnica, ao mesmo tempo em que o sistema injeta dados dinâmicos corretamente no documento final?**

---

## Decisão

**Adotarei um motor interno de templates com parse e renderização server-side**, baseado em marcações textuais com delimitadores duplos colchetes (`[[NOME_VARIAVEL]]`).

### Funcionamento do Motor

Os templates de documentos serão armazenados como registros de texto longo no banco de dados, associados à combinação `CNPJ + Natureza de Serviço`. A interface administrativa permitirá que o gestor edite diretamente o corpo do template, inserindo variáveis predefinidas (como `[[NOME_CLIENTE]]`, `[[DATA_SERVICO]]`, `[[PRAZO_GARANTIA]]`, `[[PRODUTOS_UTILIZADOS]]`) em qualquer posição do texto.

No momento da emissão de um documento, o servidor:

1. **Recupera o template** correspondente ao CNPJ e natureza de serviço do pedido.
2. **Resolve as variáveis:** Itera sobre as marcações reconhecidas e as substitui pelos valores do objeto de domínio correspondente (cliente, serviço, ordem de serviço).
3. **Renderiza o PDF:** O resultado textual é convertido em PDF via biblioteca server-side (ex: QuestPDF, DinkToPdf ou equivalente), com suporte a fontes nativas e formatação configurável.

### Catálogo Fechado de Variáveis

O conjunto de variáveis disponíveis (`[[...]]`) é definido e mantido pelo desenvolvedor no código. A gestão da empresa pode **usar** qualquer variável do catálogo nos templates, mas não pode **criar** novas variáveis arbitrárias. Essa restrição é intencional: evita que o template se torne um motor de scripts (code injection) e mantém o parse determinístico e seguro.

---

## Separação de Responsabilidades (Clean Architecture)

A implementação do motor de templates respeita a **Regra da Dependência**:

- O domínio expõe um contrato abstrato (`IDocumentRenderer` ou equivalente) que define a operação de renderização a partir de um template e de um objeto de dados.
- A implementação concreta do parser e do gerador de PDF reside na camada de infraestrutura, injetada via inversão de dependência.
- Os casos de uso (ex.: `EmitirCertificadoDeGarantia`) não conhecem a tecnologia de PDF nem o mecanismo de parsing — apenas solicitam a renderização via interface.

Essa separação garante que a troca futura da biblioteca de PDF (ex.: de DinkToPdf para QuestPDF) não afete nenhuma regra de negócio, respeitando o **Princípio Aberto-Fechado (OCP)**.

---

## Alternativas Consideradas e Motivos de Descarte

### SaaS de Geração de Documentos (ex.: DocuSign, HelloSign, Templify)
Descartado. Os custos de OPEX recorrentes de um SaaS de documentos são injustificáveis para um volume relativamente baixo de emissões de uma empresa de serviços local. Além disso, a dependência de um serviço externo para uma operação síncrona crítica (emissão do contrato antes do início do serviço) introduz um ponto único de falha externo que comprometeria a disponibilidade operacional.

### Hardcode de Layouts em HTML/CSS no Repositório de Código
Descartado. Qualquer ajuste textual — incluindo a correção de uma vírgula em uma cláusula contratual — exigiria um pull request, revisão, aprovação e deploy. Isso cria uma dependência direta do ciclo de engenharia para demandas operacionais de negócio, violando o **Princípio da Responsabilidade Única (SRP)**: o código não deve mudar por razões jurídicas.

### Motor de Templates Externo (ex.: Handlebars, Razor Pages, Liquid)
Avaliado com cuidado. Esses motores são poderosos e maduros, mas introduzem uma capacidade expressiva (loops, condicionais, acesso a objetos aninhados) que vai além do necessário e abre vetores de risco (template injection, lógica de negócio vazar para o template). O motor interno com catálogo fechado de variáveis oferece exatamente o nível de expressividade necessário — nem mais, nem menos — com controle total sobre o parsing.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Agilidade corporativa:** O setor jurídico/gerência atualiza contratos e certificados autonomamente, sem abertura de ticket para o desenvolvedor.
- **Deploy desacoplado do conteúdo:** Mudanças textuais nunca geram builds ou deploys — o conteúdo vive no banco de dados, não no código.
- **Personalização por CNPJ:** Cada combinação CNPJ × Natureza de Serviço pode ter um template completamente distinto, satisfazendo o atributo de Configurabilidade exigido.

### Desvantagens e Riscos
- **Complexidade estrutural do parse:** O interpretador de templates precisa ser robusto contra marcações malformadas (variáveis não fechadas, aninhamento incorreto), com tratamento de erro claro e defensivo. Falhas de parse em produção não podem resultar em documentos corrompidos entregues ao cliente.
- **Limitações de formatação avançada:** Layouts com tabelas complexas, imagens dinâmicas ou paginação condicional são difíceis de expressar apenas com variáveis de texto plano. Nesses casos, o template precisará de suporte a HTML estrutural — aumentando a área de risco do parse.
- **Risco de variável indefinida:** Se o gestor inserir uma variável que não existe no catálogo (ex.: `[[VARIAVEL_INEXISTENTE]]`), o sistema deve detectar e alertar no momento do salvamento do template (validação proativa), não no momento da emissão (falha em produção).

---

## Conformidade e Governança

- A interface de edição de templates exibirá o **catálogo completo de variáveis disponíveis** com descrição, prevenindo o uso de variáveis inválidas.
- O salvamento de um template acionará uma **validação de variáveis**: qualquer marcação `[[...]]` não reconhecida pelo catálogo será rejeitada com mensagem explicativa ao usuário.
- Testes unitários cobrirão o motor de parse para cenários de: substituição simples, múltiplas ocorrências da mesma variável, template sem variáveis, e variável presente no template mas com valor nulo no objeto de dados (regra: exibir string vazia, nunca a marcação bruta).

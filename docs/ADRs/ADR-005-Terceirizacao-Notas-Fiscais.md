# ADR-005: Terceirização da Emissão de Notas Fiscais para SaaS Especializado

**Data:** 15/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O ciclo de faturamento do Control Service ERP exige, como etapa final, a emissão de Notas Fiscais de Serviço Eletrônicas (NFS-e) e, em alguns casos, Notas Fiscais de Produto (NF-e). Esse requisito apresenta uma complexidade técnica que vai muito além do escopo de um ERP interno:

**A integração direta com os webservices de prefeituras brasileiras é estruturalmente caótica:**
- Cada município possui seu próprio sistema de emissão de NFS-e, com diferentes protocolos (SOAP, REST, layouts proprietários), certificações e particularidades de autenticação.
- Não existe padronização nacional completa: o padrão ABRASF é adotado por muitas prefeituras, mas com variações de implementação que exigem adaptadores individuais por município.
- Os webservices municipais são notoriamente instáveis e sujeitos a mudanças sem aviso prévio, exigindo manutenção reativa e constante de quem os integra diretamente.
- Além disso, as regras de alíquota variam por tipo de serviço (CNAE), por município e por regime tributário do CNPJ emissor — e podem mudar via legislação sem periodicidade previsível.

O Control Service opera com múltiplos CNPJs (ver ADR-002), cada um com seu próprio certificado digital A1, alíquotas configuradas e municípios de atuação potencialmente distintos. Construir e manter um emissor fiscal interno seria assumir a responsabilidade de um produto separado, com sua própria superfície de complexidade técnica, legal e regulatória — um escopo absolutamente inviável para um time de desenvolvimento de um único engenheiro.

---

## Decisão

**O ERP não emitirá notas fiscais diretamente.** A responsabilidade pela comunicação com a SEFAZ e com os webservices municipais será integralmente delegada a um **SaaS terceiro especializado em emissão fiscal** (ex.: eNotas, Focus NFe, PlugNotas ou equivalente).

O ERP assumirá o papel de **orquestrador do faturamento**: ao final do ciclo comercial de um serviço ("Pronto para Faturar"), o módulo financeiro coletará os dados necessários — dados do cliente, dados do serviço, alíquotas pré-configuradas no perfil do CNPJ emissor e certificado digital associado — e os enviará via API REST para o SaaS fiscal.

O SaaS assumirá a responsabilidade por:
1. Validar os dados fiscais conforme as regras do município e do regime tributário.
2. Assinar digitalmente o documento com o certificado A1 do CNPJ emissor.
3. Transmitir a nota para o webservice municipal ou para a SEFAZ.
4. Retornar o link do PDF e do XML da nota autorizada para o ERP.

O ERP armazenará o link/XML de retorno associado ao pedido faturado, permitindo consulta e reenvio ao cliente.

### Integração via Anti-Corruption Layer (ACL)

A integração com o SaaS fiscal será encapsulada em um **adaptador de infraestrutura** que implementa uma interface de domínio abstrata (`INotaFiscalGateway` ou equivalente). O domínio e os casos de uso do módulo Financeiro **nunca conhecerão** o SaaS específico utilizado — apenas a abstração.

Essa decisão é diretamente inspirada no **Dependency Inversion Principle (DIP)**: o módulo Financeiro (política de alto nível) não depende do SaaS (detalhe de baixo nível). Ambos dependem da abstração `INotaFiscalGateway`. A troca de um SaaS por outro (ex.: de eNotas para PlugNotas) se limitará à implementação do adaptador, sem afetar nenhuma regra de negócio.

---

## Alternativas Consideradas e Motivos de Descarte

### Desenvolvimento Interno de Emissor Fiscal
Descartado categoricamente. As razões são técnicas, operacionais e estratégicas:
- **Escopo inviável:** Integrar diretamente com os webservices das prefeituras dos municípios de atuação (e de novos municípios que a empresa venha a atender) exigiria um produto paralelo de emissão fiscal — com sua própria equipe, ciclo de manutenção e área de risco legal.
- **Alta velocidade de deterioração:** Cada mudança de webservice municipal ou de legislação fiscal implicaria manutenção urgente e não-planejada, interrompendo o desenvolvimento das funcionalidades do ERP.
- **Risco de bloqueio operacional:** Uma falha no emissor interno durante o período de faturamento (ex.: dias de fechamento mensal) paralisaria o fluxo de caixa da empresa — um risco inaceitável.

### Uso de Biblioteca Open-Source de Emissão Fiscal
Avaliado e descartado. Bibliotecas open-source para emissão fiscal no ecossistema .NET existem (ex.: NFe.io SDK), mas transferem para o desenvolvedor a responsabilidade de manter o certificado digital, lidar com erros SOAP, tratar rejeições de SEFAZ e acompanhar mudanças de schema dos XMLs fiscais. A complexidade é comparável à integração direta, sem o suporte especializado de um SaaS.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Time-to-market preservado:** Elimina semanas (ou meses) de desenvolvimento e certificação de um emissor fiscal, redirecionando o esforço para as funcionalidades que diferenciam o ERP.
- **Manutenção zero no domínio fiscal:** Mudanças de legislação, atualizações de webservices municipais e novos municípios de atuação são absorvidas pelo SaaS, sem impacto no código do ERP.
- **Confiabilidade do faturamento:** SaaS especializados possuem SLAs contratuais, monitoramento e suporte dedicado para garantir a disponibilidade do serviço em períodos críticos.

### Desvantagens e Riscos
- **Vendor Lock-in:** A empresa passa a depender de um fornecedor externo para uma operação crítica do fluxo de caixa. Se o SaaS encerrar operações, alterar preços abruptamente ou degradar sua disponibilidade, o faturamento da empresa é diretamente afetado.
  - **Mitigação:** O Anti-Corruption Layer (ACL) garante que a troca de SaaS seja uma operação de troca de adaptador, não uma refatoração arquitetural. O custo de migração é deliberadamente mantido baixo.
- **Custo de OPEX recorrente:** O SaaS possui cobrança mensal ou por volume de notas emitidas, adicionando um custo fixo à operação.
  - **Mitigação:** O custo de OPEX do SaaS é ordens de magnitude inferior ao custo de desenvolvimento, manutenção e risco operacional de um emissor interno.
- **Conascência Dinâmica com serviço externo:** A disponibilidade do faturamento do ERP passa a ter dependência temporal de um sistema externo. Uma falha do SaaS fiscal em horário de pico de faturamento bloqueia a emissão de notas.
  - **Mitigação:** O ERP implementará um fluxo de retry com backoff exponencial para chamadas à API do SaaS. Além disso, o status de emissão da nota será assíncrono no fluxo de interface — o usuário não ficará bloqueado aguardando o retorno do SaaS; será notificado quando a nota for autorizada ou rejeitada.

---

## Conformidade e Governança

- O adaptador de integração com o SaaS fiscal (`INotaFiscalGateway`) será testado via **mocks e contratos de API** (sem chamadas reais ao SaaS em ambiente de testes), garantindo que a lógica de orquestração do módulo Financeiro seja testável de forma isolada.
- As credenciais de API do SaaS (chaves de autenticação) serão gerenciadas via variáveis de ambiente seguras, nunca hardcoded no repositório.
- O ERP registrará em log auditável cada tentativa de emissão: payload enviado, resposta recebida (sucesso ou erro), timestamp e identificador do pedido associado — garantindo rastreabilidade completa do ciclo fiscal.
- A seleção do SaaS fiscal específico será revisada anualmente com base em disponibilidade, custo e cobertura de municípios de atuação.

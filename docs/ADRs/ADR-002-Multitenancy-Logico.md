# ADR-002: Adoção de Multitenancy Lógico por Perfil CNPJ

**Data:** 15/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O Control Service ERP opera sob uma realidade organizacional singular: a empresa funciona como uma entidade integrada de gestão única, mas juridicamente se manifesta através de múltiplos CNPJs. Essa separação jurídica não é estrutural nem operacional — ela existe por razões estritamente fiscais e regulatórias:

1. **Alíquotas tributárias distintas:** Serviços de dedetização e higienização de reservatórios possuem classificações tributárias e regimes fiscais diferentes, exigindo CNPJs separados para a emissão correta de notas fiscais.
2. **Normas operacionais do INEA:** Os serviços regulamentados (dedetização e higienização) seguem normas operacionais específicas do Instituto Estadual do Ambiente, com requisitos de relatório (RAAE) e certificação que devem ser rastreados por entidade jurídica.
3. **Certificados digitais A1 distintos:** Cada CNPJ possui seu próprio certificado para assinatura digital de documentos fiscais.

Dado isso, o sistema precisa garantir **isolamento rigoroso** entre os dados e documentos de cada CNPJ, ao mesmo tempo em que provê uma **visão consolidada** da operação para a gestão da empresa. Esse é o problema clássico de Multitenancy — onde o "tenant" não é um cliente externo, mas um perfil jurídico interno da mesma empresa.

---

## Decisão

**Adotarei o Multitenancy Lógico em banco de dados único**, implementado através de uma chave discriminadora `tenant_id` (referenciando o perfil CNPJ) em todas as entidades que exijam segregação fiscal, operacional ou documental.

Essa chave será o mecanismo de isolamento central do sistema. Toda entidade que pertença a um contexto CNPJ específico — contratos, certificados de garantia, configurações de alíquota, ordens de serviço regulatórias e relatórios RAAE — carregará essa chave como parte de sua identidade de persistência.

A abordagem é fundamentada nos seguintes princípios:

- **Conformidade com o Monólito Modular (ADR-001):** Um banco de dados por tenant fragmentaria o único artefato implantável do sistema, introduzindo o gerenciamento de múltiplos pipelines de migração — uma sobrecarga inaceitável para um desenvolvedor solo.
- **Princípio da Responsabilidade Única (SRP) aplicado ao schema:** O isolamento de tenant não é uma responsabilidade da camada de aplicação dispersa em queries individuais. Ele é uma política centralizada, enforçada de forma declarativa no ORM.
- **Regra da Dependência (Clean Architecture):** A lógica de domínio não deve conhecer o mecanismo de isolamento de tenant. O filtro de `tenant_id` é um detalhe de persistência, não uma regra de negócio. Ele vive na camada de infraestrutura.

---

## Alternativas Consideradas e Motivos de Descarte

### Isolamento Físico (Database per Tenant)
Descartado. Embora ofereça o mais alto nível de isolamento (sem risco de vazamento de dados entre tenants via queries incorretas), os custos operacionais são proibitivos neste contexto:
- **Múltiplos pipelines de migração:** A cada alteração de schema, seria necessário aplicar e validar a migração em cada banco separado — uma operação propensa a erros e impossível de manter com consistência por um único desenvolvedor.
- **Custo de infraestrutura crescente:** Cada banco de dados adiciona overhead de conexão, backup, monitoramento e recursos de servidor.
- **Relatórios consolidados inviabilizados:** A visão gerencial unificada (ex.: balanço de comissões total da empresa, relatório de vendas agregado) exigiria queries federadas entre bancos distintos, elevando drasticamente a complexidade de leitura analítica.

### Schema por Tenant (Schema-per-Tenant)
Descartado. Uma alternativa intermediária ao Database-per-Tenant, mas que compartilha os mesmos problemas de gerenciamento de múltiplos pipelines de migração de schema dentro do mesmo banco. No PostgreSQL, schemas separados exigiriam configuração de `search_path` por sessão, introduzindo complexidade no gerenciamento de conexões do pool do EF Core.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Custo de infraestrutura mínimo:** Um único banco de dados para administrar, fazer backup, monitorar e migrar.
- **Relatórios consolidados nativos:** Consultas analíticas que agregam dados de todos os CNPJs (ex.: faturamento total da empresa, balanço de comissões global) são nativas em SQL — sem necessidade de federação ou ETL.
- **Pipeline de migração único:** `dotnet ef migrations` aplica mudanças de schema uma única vez, com rastreabilidade central.

### Desvantagens e Riscos
- **Risco de vazamento lateral de dados (Data Leakage):** Este é o risco arquitetural mais crítico da abordagem. Uma query sem o filtro `tenant_id` retornaria dados de todos os CNPJs misturados, violando o isolamento. A ausência de fronteira física entre tenants significa que o sistema depende da corretude do código para manter o isolamento.
- **Carga mental elevada no desenvolvimento:** Todo desenvolvedor que escreva queries ou acesse o ORM precisa estar ciente da obrigação de filtrar por tenant. Em uma equipe maior, isso seria um risco recorrente.

---

## Conformidade e Governança

O risco de vazamento lateral será mitigado por uma estratégia de defesa em camadas:

1. **Global Query Filters (EF Core):** Filtros de consulta globais configurados no `DbContext` garantem que **toda** query gerada pelo ORM inclua automaticamente o predicado `WHERE tenant_id = @currentTenantId`. Essa é a primeira linha de defesa — o filtro é aplicado de forma declarativa e centralizada, invisível para os casos de uso.

2. **Resolução de Tenant por Contexto de Autenticação:** O `tenant_id` ativo será resolvido a partir do token de autenticação do usuário autenticado (claim de CNPJ), injetado via um serviço `ITenantContext` no contexto do request HTTP. Isso garante que a identidade do tenant nunca seja passada manualmente por parâmetro nas chamadas de caso de uso — reduzindo o vetor de erro humano.

3. **Testes de Isolamento:** Testes de integração explícitos verificarão que operações executadas sob o contexto do Tenant A nunca retornam dados pertencentes ao Tenant B, mesmo em cenários de schema compartilhado.

4. **Revisões de Pull Request:** Qualquer desabilitação explícita do Global Query Filter (via `.IgnoreQueryFilters()`) no código de aplicação deverá ser justificada em comentário e revisada obrigatoriamente — esse padrão é reservado exclusivamente para operações administrativas de nível de sistema (ex.: geração de relatório consolidado multi-CNPJ).

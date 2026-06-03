# ADR-006: Adoção do PostgreSQL como Banco de Dados Relacional

**Data:** 18/05/2025
**Autor:** Matheus Mendes
**Status:** Aceito

---

## Contexto

O Control Service ERP exige um banco de dados relacional capaz de sustentar simultaneamente três classes de carga distintas:

1. **Transações OLTP de alta integridade:** Faturamento, emissão de Ordens de Serviço, lançamento de contas a receber/pagar e registro de itinerários são operações que exigem garantias ACID completas. Uma nota fiscal emitida deve estar atômica e consistentemente associada ao pedido que a gerou — sem estados parciais.

2. **Isolamento lógico de múltiplos CNPJs (Multitenancy):** Conforme definido na ADR-002, o isolamento entre perfis CNPJ é implementado via `tenant_id`. Isso exige suporte robusto a índices compostos (incluindo o `tenant_id`) e, idealmente, suporte a **índices parciais** para otimizar queries de um tenant específico sem varrer toda a tabela.

3. **Leituras analíticas para relatórios regulatórios e gerenciais:** A geração do RAAE mensal, o balanço de comissões e os relatórios de vendas envolvem agregações sobre conjuntos de dados de todo o período, potencialmente cruzando múltiplos módulos (Comercial, Operacional, Financeiro).

A stack de desenvolvimento é **.NET com Entity Framework Core 10** como ORM. A escolha do banco de dados deve considerar: custo de licenciamento, suporte de primeira classe no EF Core, capacidade técnica para os padrões de acesso descritos, e custo operacional de administração para um time de um único desenvolvedor.

Dois candidatos foram avaliados formalmente: **PostgreSQL** e **SQL Server**.

---

## Decisão

**Adotarei o PostgreSQL** como banco de dados relacional do Control Service ERP.

### Fundamentos da Decisão

**1. Custo Zero de Licenciamento (Open-Source):**
O PostgreSQL é distribuído sob a licença PostgreSQL (similar à MIT/BSD), sem custo de licenciamento. O SQL Server licenciado (Standard ou Enterprise) possui custo de OPEX recorrente incompatível com o orçamento de um sistema interno de gestão. Esse fator, isoladamente, é eliminatório para o SQL Server licenciado.

**2. Suporte de Primeira Classe no EF Core 10 via Npgsql:**
O provedor `Npgsql.EntityFrameworkCore.PostgreSQL` é mantido ativamente, alinhado ao ciclo de releases do EF Core e amplamente utilizado pela comunidade .NET. A API do EF Core abstrai as diferenças de dialeto SQL, tornando a experiência de desenvolvimento com PostgreSQL equivalente à do SQL Server para as operações do dia a dia (LINQ, migrations, transactions). O risco técnico de adoção é baixo.

**3. Funcionalidades Avançadas Relevantes ao Domínio:**
O PostgreSQL oferece, sem custo adicional, funcionalidades que serão diretamente úteis no sistema:

- **Índices Parciais:** Permitem criar índices sobre um subconjunto de linhas (ex.: `WHERE tenant_id = X`), otimizando queries de tenant específico sem o overhead de um índice sobre toda a tabela — diretamente aplicável ao padrão de multitenancy lógico.
- **Tipos JSON Nativos (JSONB):** Permitem armazenar e consultar estruturas semi-estruturadas (ex.: payload de retorno da API de notas fiscais, logs de sincronização do app móvel) de forma nativa, sem necessidade de tabelas auxiliares ou serialização manual em texto.
- **Full-Text Search nativo:** Pode ser utilizado futuramente para pesquisa textual em histórico de clientes ou contratos, sem dependência de um serviço externo.
- **Advisory Locks:** Mecanismo nativo para serialização de operações críticas (ex.: garantir que dois processos não gerem o mesmo número de Ordem de Serviço simultaneamente), útil em cenários futuros de background jobs concorrentes.

**4. Posicionamento como "Detalhe" de Infraestrutura (Clean Architecture):**
Conforme o princípio de que **o banco de dados é um detalhe** da arquitetura, a camada de domínio e os casos de uso do ERP são completamente agnósticos ao banco de dados utilizado. A inversão de dependência, via interfaces de repositório implementadas na camada de infraestrutura, garante que a troca do banco de dados (se necessária no futuro) seja uma operação restrita à camada de infraestrutura. Essa decisão não cria acoplamento de negócio com o PostgreSQL.

---

## Alternativas Consideradas e Motivos de Descarte

### SQL Server (Licenciado — Standard/Enterprise)
Descartado. O custo de OPEX recorrente de licenciamento é injustificável para um sistema interno sem previsão de receita própria. A funcionalidade técnica oferecida não compensa o custo em comparação ao PostgreSQL open-source.

### SQL Server Express (Gratuito)
Descartado por limitações estruturais incompatíveis com o crescimento esperado do sistema:
- **Limite de 10 GB por banco de dados:** O armazenamento de arquivos de O.S., fotos de campo sincronizadas e histórico de relatórios RAAE pode facilmente exceder esse limite ao longo dos anos.
- **Ausência do SQL Server Agent:** Impossibilita o agendamento nativo de rotinas de manutenção (ex.: purge de logs, geração proativa de relatórios), que serão necessárias no futuro.
- **Limitação de CPU e RAM:** O Express é restrito a 1 socket/4 cores e 1 GB de RAM em buffer pool — insuficiente para picos de carga durante geração de relatórios RAAE e sincronizações simultâneas do app móvel.

### SQLite (para o servidor central)
Descartado para uso no backend central. O SQLite é excelente como banco embarcado para o app móvel offline (ver ADR-003), mas inadequado para o servidor central por:
- **Limitação de concorrência de escrita:** O SQLite serializa todas as escritas com um lock global. Com múltiplos usuários no escritório + sincronizações simultâneas do app móvel, o throughput de escrita seria inaceitável.
- **Falta de controle de acesso granular:** Sem sistema de usuários nativo, o isolamento de permissões por perfil de acesso precisaria ser implementado inteiramente na aplicação.

---

## Trade-offs, Riscos e Impactos

### Vantagens
- **Custo de licenciamento zero:** Elimina OPEX recorrente associado ao banco de dados.
- **Maturidade e confiabilidade comprovadas:** O PostgreSQL é amplamente utilizado em sistemas de missão crítica há décadas, com suporte transacional completo (ACID, MVCC), replicação nativa e comunidade ativa.
- **Funcionalidades avançadas sem custo adicional:** Índices parciais, JSONB e advisory locks entregam valor concreto para os padrões de acesso do ERP sem necessidade de licenças extras.
- **Portabilidade de infra:** O PostgreSQL roda em qualquer ambiente — on-premise, Docker, cloud gerenciado (AWS RDS, Azure Database for PostgreSQL, Supabase) — sem custo de migração de licença.

### Desvantagens e Riscos
- **Menor familiaridade da ferramenta de administração visual:** O pgAdmin é menos polido e intuitivo que o SQL Server Management Studio (SSMS), com o qual o desenvolvedor possui maior familiaridade histórica.
  - **Mitigação:** Uso do **DBeaver** (gratuito, multiplataforma, suporte nativo a PostgreSQL) ou do próprio SSMS com driver ODBC para operações de administração pontual. O EF Core Migrations cobre a grande maioria das operações de schema do dia a dia, reduzindo a dependência de ferramentas de administração visual.
- **Diferenças de dialeto SQL em queries nativas:** Casos onde é necessário escrever SQL nativo (ex.: relatórios analíticos complexos) podem apresentar diferenças de sintaxe em relação ao T-SQL do SQL Server (ex.: operador de concatenação, funções de data, CTEs recursivos).
  - **Mitigação:** A preferência é pelo uso de LINQ via EF Core para 100% das queries de domínio. Queries nativas serão documentadas e revisadas como SQL padrão PostgreSQL.

---

## Conformidade e Governança

- **Gerenciamento de Schema via EF Core Migrations:** Todas as alterações de estrutura de banco de dados serão realizadas exclusivamente através de `dotnet ef migrations add` e `dotnet ef database update`. Nenhuma alteração de schema será realizada manualmente em produção. As migrations são rastreadas em controle de versão (Git) junto ao código, garantindo rastreabilidade e reversibilidade.
- **Migrations em CI/CD:** O pipeline de CI/CD executará as migrations automaticamente no ambiente de staging antes de cada deploy, validando que o schema está consistente com o código antes da promoção para produção.
- **Backups automatizados:** A política de backup do PostgreSQL será configurada com retenção mínima de 30 dias (full daily + WAL archiving para point-in-time recovery), garantindo o atributo de Auditabilidade e proteção contra perda de dados.
- **Restrições de acesso ao banco:** O usuário de aplicação (`controlservice_app`) terá permissões restritas a DML (SELECT, INSERT, UPDATE, DELETE) nas tabelas de negócio. Operações DDL serão executadas exclusivamente pelo usuário de migrations em contexto controlado.

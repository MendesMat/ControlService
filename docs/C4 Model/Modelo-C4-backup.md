## 5. Comunicação da Arquitetura - Modelo C4
O modelo C4 (escrito em formato adaptado de fluxos) orienta a minha visão estrutural do sistema de forma coerente.

### 5.1 Nível 1: Diagrama de Contexto
Demonstra a interação primordial entre as Personas da nossa empresa, o meu ERP e sistemas externos.

```mermaid
C4Context
    title Nível 1 - Diagrama de Contexto: Control Service ERP

    Person(admin, "Backoffice / Admin", "Realiza gestão operacional, agenda roteiros e monitora faturamento.")
    Person(operador, "Operador de Campo", "Recebe rotas, atende no cliente offline e sincroniza baixa.")
    
    System(erp, "Meu Control Service ERP", "Centraliza clientes, contratos, finanças e regras INEA da nossa empresa.")
    System_Ext(inea, "Sistema Ambiental INEA", "Portal externo que consumirá o nosso RAAE gerado.")
    System_Ext(nf_api, "API de Notas Fiscais SaaS", "Ex: Focus NFe. Motor externo para consolidar NFS-e em prefeituras.")

    Rel(admin, erp, "Gerencia processos e solicita notas (HTTPS)")
    Rel(operador, erp, "Sincroniza Roteiros e Baixas (HTTPS)")
    Rel(erp, inea, "Gera formato válido para RAAE")
    Rel(erp, nf_api, "Transmite ordens com alíquotas fixadas para emissão", "HTTPS/REST")
```

### 5.2 Nível 2: Diagrama de Contêiner
Explícita os meus limites técnicos implantáveis (quanta lógico).

```mermaid
C4Container
    title Nível 2 - Diagrama de Contêiner

    Person(admin, "Backoffice / Admin", "Usuário gerencial na base")
    Person(operador, "Operador de Campo", "Atua isolado no endereço do cliente")

    Container(spa, "SPA Admin Web", "React/TS", "Minha aplicação visual principal para o escritório.")
    Container(mobile, "App Mobile Field", "Flutter/SQLite", "Meu app offline-first que guarda as rotas do dia na memória interna.")
    
    Container(api, "Backend Monolítico API", ".NET (C#)", "Meu núcleo de regras de negócio segregadas modularmente.")
    ContainerDb(db, "Unified Database", "RDBMS", "Repositório central particionado via tenant_id")

    System_Ext(nf_api, "Motor Faturamento SaaS")

    Rel(admin, spa, "Acessa no navegador")
    Rel(operador, mobile, "Interage offline e sincroniza depois")
    
    Rel(spa, api, "Lê/Grava Dados Administrativos", "REST/JSON")
    Rel(mobile, api, "Sincroniza Lotes de Execuções e Insumos", "REST/JSON")
    Rel(api, db, "Faz leitura e escrita global", "Entity Framework Core")
    Rel(api, nf_api, "Delega geração complexa (NFS-e)")
```

### 5.3 Nível 3: Diagrama de Componentes (Foco no Backend API)
Foco exclusivo para demonstrar como modelei o padrão de *Domain Partitioning* justificado.

```mermaid
C4Component
    title Nível 3 - Diagrama de Componentes: API Backend

    Container_Boundary(backend_boundary, "Módulos de Domínio (Processo Único)") {
        Component(c_auth, "1. Gerenciamento", "Configuração Tenant, Alíquotas NF, Templates", "Autoriza JWT e recupera o tenant_id da requisição.")
        Component(c_comercial, "2. Comercial", "Gestão de Clientes e Roteiros", "Agrupa propostas e gera fluxos de serviços pendentes.")
        Component(c_operacional, "3. Operacional", "Ordens de Serviço e Offline Sync", "Sincroniza os endpoints móveis e deduz o uso de químicos.")
        Component(c_financeiro, "4. Financeiro", "Faturamento, Comissões, Gatilho NF", "Recebe finalização de O.S e dispara os requests de NF-e.")
        Component(c_relatorio, "5. Relatórios", "Conformidade e RAAE", "Gera os contratos dinâmicos, consolida log do INEA.")
    }

    ContainerDb(db, "Meu Banco de Dados")
    System_Ext(nf_api, "Motor SaaS Externo")

    Rel(c_auth, c_comercial, "Injeta claims de perfil e tenant")
    Rel(c_comercial, c_operacional, "Determina roteiros para sincronização")
    Rel(c_operacional, c_financeiro, "Autoriza fechamento (Pronto Faturar)")
    Rel(c_operacional, c_relatorio, "Compila dados químicos aplicados")
    Rel(c_financeiro, nf_api, "Delega a burocracia governamental", "HTTPS")
    
    Rel(c_auth, db, "CRUD (EF Core)")
    Rel(c_comercial, db, "CRUD (EF Core)")
    Rel(c_operacional, db, "CRUD (EF Core)")
    Rel(c_financeiro, db, "CRUD (EF Core)")
    Rel(c_relatorio, db, "Leitura Analítica (Dapper)")
```

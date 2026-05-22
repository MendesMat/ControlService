workspace "Control Service ERP" "Arquitetura do sistema de gestao integrado para prestadores de servicos especializados." {

    !identifiers hierarchical

    model {
        // =============================================
        // PERSONAS
        // =============================================
        admin = person "Escritório / Administrador" "Realiza gestao operacional, agenda roteiros e monitora faturamento."
        operador = person "Operador de Campo" "Recebe rotas, atende no cliente de forma offline e sincroniza a baixa dos servicos."

        // =============================================
        // SISTEMAS EXTERNOS
        // =============================================
        nf_api = softwareSystem "API de Notas Fiscais SaaS" "Ex: Focus NFe. Motor externo responsavel por consolidar NFS-e junto as prefeituras." "External System"

        // =============================================
        // NOSSO SISTEMA PRINCIPAL
        // =============================================
        erp = softwareSystem "Control Service ERP" "Centraliza clientes, contratos, financas e regras INEA. Opera sob multiplos CNPJs com isolamento logico via tenant_id." {

            spa = container "SPA Web Administrativo" "Aplicacao web principal para o time de escritorio. Acesso a todos os modulos gerenciais." "React 18 (LTS) / TypeScript" {
                tags "Web Browser"
            }

            mobile = container "App Móvel de Campo" "App offline-first para operadores de campo. Armazena rotas do dia localmente e sincroniza execucoes ao reconectar." "React Native / Expo" {
                tags "Mobile App"
            }

            mobile_db = container "Banco de Dados Local (Móvel)" "Armazena o roteiro do dia e as baixas de servico coletadas offline, antes da sincronizacao com o backend." "SQLite" {
                tags "Database"
            }

            db = container "Banco de Dados Unificado" "Repositorio central particionado por tenant_id. Isola dados fiscais e operacionais por perfil CNPJ." "PostgreSQL" {
                tags "Database"
            }

            api = container "Backend Monolitico API" "Nucleo de regras de negocio segregadas modularmente por dominio. Unico processo implantavel." ".NET 10 (C#)" {

                c_gerenciamento = component "Modulo de Gerenciamento" "Gerencia configuracoes de tenant, perfis CNPJ, aliquotas de NF e templates de documentos. Autentica via JWT e injeta tenant_id nas requisicoes." "ASP.NET Core 10 / EF Core 10"

                c_comercial = component "Modulo Comercial" "Gerencia clientes, contratos e roteiros de servico. Agrupa propostas e gera fluxos de servicos pendentes." "ASP.NET Core 10 / EF Core 10"

                c_operacional = component "Modulo Operacional" "Processa ordens de servico e a sincronizacao offline do aplicativo móvel. Registra uso de insumos quimicos." "ASP.NET Core 10 / EF Core 10"

                c_financeiro = component "Modulo Financeiro" "Gerencia faturamento, comissoes e dispara a emissao de NF via API terceira apos fechamento de O.S." "ASP.NET Core 10 / EF Core 10"

                c_relatorios = component "Modulo de Relatorios" "Consolida dados analiticos para o relatorio RAAE do INEA, balanco de comissoes e relatorio de vendas. Delega a geracao de PDF ao motor de templates." "ASP.NET Core 10 / Dapper"

                c_templates = component "Motor de Templates" "Adaptador de infraestrutura que le templates do banco de dados, substitui variaveis dinamicas e renderiza o PDF final. Implementa interface do dominio (IDocumentRenderer)." "QuestPDF / Parser interno"
            }
        }

        // =============================================
        // RELACIONAMENTOS - NIVEL 1 (Contexto)
        // Declarados no nivel de sistema para que o systemContext view funcione corretamente.
        // Relacionamentos implícitos se propagam para os níveis inferiores automaticamente.
        // =============================================
        admin -> erp "Acessa a interface de gestao" "HTTPS"
        operador -> erp "Consulta rotas e registra execucoes" "HTTPS (sincronização)"
        erp -> nf_api "Delega emissao de NFS-e com aliquotas do perfil CNPJ" "HTTPS / REST"

        // =============================================
        // RELACIONAMENTOS - NIVEL 2 (Containers)
        // =============================================
        erp.spa -> erp.api "Consome APIs administrativas e financeiras" "REST / JSON"
        erp.mobile -> erp.api "Sincroniza dados da O.S. e insumos" "REST / JSON"
        erp.mobile -> erp.mobile_db "Persiste roteiro e baixas offline" "SQLite"
        erp.api -> erp.db "Leitura e escrita de todos os modulos" "Entity Framework Core / Dapper"

        // =============================================
        // RELACIONAMENTOS - NIVEL 3 (Componentes internos da API)
        // =============================================
        // Conexoes dos Clientes (Containers) para os Componentes especificos da API (Nivel 3)
        erp.spa -> erp.api.c_gerenciamento "Realiza login, gerencia tenants e templates" "HTTPS / REST / JSON"
        erp.spa -> erp.api.c_comercial "Gerencia clientes, contratos e roteiros diarios" "HTTPS / REST / JSON"
        erp.spa -> erp.api.c_financeiro "Monitora fluxo de caixa e dispara faturamento" "HTTPS / REST / JSON"
        erp.spa -> erp.api.c_relatorios "Busca relatorios gerenciais e RAAE" "HTTPS / REST / JSON"

        erp.mobile -> erp.api.c_operacional "Busca roteiro offline e envia baixas de servicos" "HTTPS / REST / JSON"

        // Comunicacao entre Componentes da API (Acoplamento fraco e regras de negocio)
        erp.api.c_gerenciamento -> erp.api.c_comercial "Injeta claims de perfil e tenant_id"
        erp.api.c_operacional -> erp.api.c_comercial "Consulta agendamentos e envia relatos de baixas para consolidacao"
        erp.api.c_financeiro -> erp.api.c_comercial "Consulta pedidos e contratos consolidados para faturamento manual"
        erp.api.c_financeiro -> nf_api "Delega a burocracia de emissao fiscal" "HTTPS"
        erp.api.c_relatorios -> erp.api.c_comercial "Consulta O.S. consolidadas e comissoes para geracao do RAAE e relatorios gerenciais"
        erp.api.c_relatorios -> erp.api.c_templates "Solicita renderizacao de contratos e certificados em PDF"

        erp.api.c_gerenciamento -> erp.db "CRUD de configuracoes, templates de documentos e perfis CNPJ" "EF Core"
        erp.api.c_comercial -> erp.db "CRUD de clientes e contratos" "EF Core"
        erp.api.c_operacional -> erp.db "CRUD de ordens de servico e insumos" "EF Core"
        erp.api.c_financeiro -> erp.db "CRUD de faturamento e comissoes" "EF Core"
        erp.api.c_relatorios -> erp.db "Leitura analitica para relatorios" "Dapper"
    }

    views {
        // =============================================
        // NIVEL 1 - Diagrama de Contexto
        // =============================================
        systemContext erp "Contexto" "Diagrama de Contexto de Sistema" {
            include *
            autolayout lr
        }

        // =============================================
        // NIVEL 2 - Diagrama de Containers
        // =============================================
        container erp "Conteineres" "Diagrama de Contêineres" {
            include *
            autolayout lr
        }

        // =============================================
        // NIVEL 3 - Diagrama de Componentes (API)
        // =============================================
        component erp.api "Componentes" "Diagrama de Componentes" {
            include *
            autolayout lr
        }

        // =============================================
        // ESTILOS
        // =============================================
        styles {
            element "Element" {
                color #ffffff
                stroke #2d6a9f
                strokeWidth 3
                shape RoundedBox
            }
            element "Person" {
                shape Person
                background #08427b
                color #ffffff
            }
            element "Software System" {
                background #1168bd
                color #ffffff
            }
            element "External System" {
                background #999999
                color #ffffff
            }
            element "Container" {
                background #438dd5
                color #ffffff
            }
            element "Component" {
                background #85bbf0
                color #000000
            }
            element "Database" {
                shape Cylinder
                background #438dd5
                color #ffffff
            }
            element "Web Browser" {
                shape WebBrowser
            }
            element "Mobile App" {
                shape MobileDevicePortrait
            }
            relationship "Relationship" {
                thickness 2
            }
        }
    }
}

# ADR 02: Adoção de Multitenancy Lógico por CNPJ

## Status:
Aceito

## Contexto:
A empresa atua como uma organização única, mas opera através de múltiplos perfis de CNPJs. Essa separação existe exclusivamente por conta de alíquotas tributárias distintas e porque os serviços de dedetização e higienização de reservatórios exigem um tratamento especial perante as normas operacionais do INEA. Preciso isolar essas emissões e registros documentais sem fragmentar a gestão de toda a empresa.

## Decisão:
Optei por implementar o multitenancy lógico em um único banco de dados unificado, adicionando uma chave `tenant_id` (identificando o CNPJ da filial) em toda entidade que exija segregação fiscal ou operacional.

## Alternativas consideradas:
Isolamento Físico (Database per Tenant): Descartei. Exigiria que eu gerenciasse múltiplos pipelines de migração de banco de dados, o que aumentaria os custos e superaria minha capacidade operacional diária como único desenvolvedor.

## Trade-offs, Riscos e Impactos:
- Impacto positivo: Excelente redução de custos de infraestrutura e simplificação brutal para extrair relatórios consolidados da operação da empresa como um todo.
- Impacto negativo: Assumo um risco constante de vazamento lateral de dados se eu esquecer a cláusula de filtro em uma query. Isso eleva minha carga mental e meu esforço na hora de codificar.

## Compliance:
Configurei de forma mandatória o uso de Filtros de Consulta Globais (Global Query Filters) na camada do meu ORM.

# ADR 01: Adoção do Estilo Monolítico Modular (Modular Monolith)

- **Status:** Aceito
- **Contexto:** O projeto abarca os módulos Financeiro, Comercial, Relatórios e Operacional do meu ERP, que devem conversar perfeitamente entre si. Como sou o único desenvolvedor atuando na construção e manutenção, preciso de uma arquitetura que maximize minha produtividade.
- **Decisão:** Tomei a decisão de que a arquitetura do Backend será um Monólito Modular, utilizando *Particionamento por Domínio*. Todos os módulos rodarão num processo único e conversarão via *Conascência Estática* Síncrona, agrupados por domínio (e não por função técnica como na Arquitetura em Camadas clássica).
- **Alternativas consideradas:** 
  - *Microsserviços*: Descartei violentamente essa opção devido à complexidade introduzida (transações compensatórias para comunicação inter-banco, deploy segmentado, latência e observabilidade distribuída). É uma abordagem injustificável e inatingível para mim, atuando sozinho no escopo da nossa empresa.
- **Trade-offs, Riscos e Impactos:** Ganho simplicidade, facilidade de implantação e garantias transacionais locais (ACID). O *trade-off* deste design é a impossibilidade de eu dar elasticidade aos módulos separadamente (se o financeiro cair sob alta carga, o comercial cai também).
- **Compliance:** Utilizarei ferramentas como *ArchUnit/NetArchTest* no meu pipeline de CI/CD para impedir que os módulos burlem as regras de dependência (garantindo o baixo acoplamento entre os meus domínios).

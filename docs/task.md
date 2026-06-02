# Plano de Ação: Projeto de Modelagem DDD

Esta é a nossa TODO list para preencher todas as lacunas identificadas no relatório de análise de requisitos. Vamos atualizar as caixas de seleção (`[ ]` -> `[/]` -> `[x]`) conforme progredimos.

### Fase 1: Exploração do Domínio e Design Estratégico (Documentação)
- [x] **1.1. Especialistas de Domínio:** Redigir texto identificando quem seriam os especialistas (ex: Operadores de Campo, Gerente Comercial) e como ocorreu a "exploração" do domínio com eles.
- [x] **1.2. Linguagem Ubíqua & Conceitos:** Criar um glossário formal mapeando os termos de negócio (RAAE, Ordem de Serviço, Baixa, Natureza de Serviço, etc.) e as regras do domínio.
- [x] **1.3. Context Map:** Desenhar ou descrever formalmente o mapa de contexto identificando padrões de relação (Partnership, Conformist, ACL) entre os Bounded Contexts.
- [x] **1.4. Core Domain:** Documentar explicitamente qual dos contextos é o *Core Domain* (ex: Operacional/Comercial) e justificar.

### Fase 2: Modelagem Tática e Comportamento (Código e Documentação)
- [x] **2.1. Ciclo de Vida do Agregado:** Escrever documentação sobre como é a criação, transição de estados e exclusão (soft delete/arquivamento) da raiz do agregado.
- [x] **2.2. Invariantes e Limites Transacionais:** Documentar a decisão por trás dos limites do agregado (ex: por que `Customer` é uma raiz, quais regras ele blinda internamente).
- [ ] **2.3. Domain Services:** Implementar (no código C#) pelo menos um Domain Service para um comportamento que envolva múltiplas entidades ou não caiba naturalmente em um Agregado/VO.
- [ ] **2.4. Specification / Policy:** Implementar (no código C#) o padrão de *Specification* ou *Policy* para blindar uma regra de negócio complexa (ex: `CustomerEligibleForServiceSpecification`).

### Fase 3: Fechamento e Entrega (Relatório Final)
- [ ] **3.1. Estruturação do Relatório:** Consolidar todas as informações das Fases 1 e 2 em um formato discursivo que atenda ao template do relatório (Contexto, Linguagem Ubíqua, Mapas, Tático, etc.).
- [ ] **3.2. Diagramas de Tradução para Código:** Gerar (ou incluir) um diagrama de classes / conceitual que mostre como a linguagem ubíqua e os agregados viraram código C#.
- [ ] **3.3. Exportar PDF Final:** Compilar tudo em um arquivo `.pdf` no padrão de nome exigido: `nome_sobrenome_dr2_modelagem_dominios_ddd.pdf`.
- [ ] **3.4. Gravar Vídeo de Apresentação:** Tarefa para o Aluno (gravar vídeo de até 5 minutos percorrendo o PDF e explicando as decisões, linguagem, invariantes e código).

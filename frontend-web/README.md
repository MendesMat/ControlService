# ControlService Web SPA — Painel Administrativo

Este é o cliente web do **ControlService ERP**, desenvolvido para centralizar toda a gestão administrativa, comercial, financeira e de relatórios da empresa prestadora de serviços especializados. Ele opera em rede estável e atende às operações internas do escritório.

---

## 🚀 Stack de Tecnologias

O front-end web é projetado sob os mais modernos conceitos de desenvolvimento de interfaces dinâmicas e de alto desempenho:

*   **Core:** React (Versão LTS mais atual)
*   **Linguagem:** TypeScript para tipagem estática e segurança em tempo de desenvolvimento
*   **Estilização:** CSS Moderno (Vanilla CSS ou TailwindCSS conforme decisão técnica de implantação) seguindo as diretrizes rígidas do [Documento de Identidade Visual](../docs/frontend-stylization.md)
*   **Roteamento:** React Router para gerenciamento de Single Page Application (SPA)
*   **Consumo de API:** Axios ou Fetch API com interceptores para gerenciar tokens, cabeçalhos de multitenancy e tratamento global de erros

---

## 🎨 Identidade Visual e Layout

A interface segue uma linha estética premium, profissional e limpa, otimizada para alta produtividade em escritório:

*   **Estrutura Base (Layout SPA):**
    *   **Navegação Lateral Fixa (Sidebar):** Menu retrátil com largura fixa de `240px`, fundo escuro (`#111827`), texto em cinza claro (`#D1D5DB`) e ícones em `#9CA3AF`.
    *   **Área de Conteúdo Principal:** Fundo claro/off-white (`#F9FAFB` / `#FFFFFF`) para máximo contraste e legibilidade.
*   **Paleta de Cores Harmônica:**
    *   **Primária:** Azul Destaque (`#2563EB`), com hover em Azul Escuro (`#1E40AF`).
    *   **Secundária/Estrutura:** Tons de cinza estruturados (`#111827`, `#374151`, `#F3F4F6`).
    *   **Estados (Feedback Rápido):** Sucesso (Verde `#16A34A`), Alerta (Amarelo `#D97706`) e Erro (Vermelho `#DC2626`).
*   **Componentização e Micro-Interações:**
    *   **Tipografia:** Utilização da fonte moderna `Inter` (com fallbacks nativos).
    *   **Bordas e Sombras:** Cards e containers com cantos arredondados (`border-radius: 8px`) e sombras sutis (`shadow-sm`).
    *   **Inputs Dinâmicos:** Efeito hover e focus com glow azul (`#2563EB`).
    *   **Tabelas Responsivas:** Linhas alternadas em cinza leve com efeito hover para facilitar a leitura de planilhas e relatórios volumosos.

---

## ⚙️ Módulos Funcionais Implementados

O painel web gerencia 4 dos 5 setores funcionais do ERP:

### 1. Setor de Gerenciamento
*   **Gestão de Perfis de CNPJ (Multitenancy):** Isolamento lógico rigoroso. Seleção do tenant ativo que injeta dinamicamente o cabeçalho apropriado para a API backend.
*   **Cadastro de Perfis e Usuários:** Controle de acesso granular baseado em papéis (Leitura, Edição, Criação/Exclusão).
*   **Templates Dinâmicos:** Motor de renderização visual para personalização de contratos e certificados por CNPJ e Natureza de Serviço (ex: substituição automática de variáveis como `[[NOME_CLIENTE]]`).
*   **Catálogo de Naturezas, Objetos, Garantias e Químicos:** Mapeamento tributário, comissões de objetos e produtos regulamentados pelo INEA.

### 2. Setor Comercial
*   **CRM e Clientes:** CRUD completo de clientes com histórico centralizado de ordens de serviço.
*   **Renovação Proativa:** Painel mensal de vencimentos de garantias baseado nos objetos de serviço configurados.
*   **Roteiro Diário de Serviços:** Painel com filtros avançados (Bairro, Operador, Objeto, Status) para acompanhamento em tempo real da agenda da empresa.

### 3. Setor Financeiro
*   **Contas a Receber:** Gestão de faturamento agrupado mensalmente para serviços com status "Pronto para Faturar".
*   **Contas a Pagar:** Lançamento de despesas operacionais e consolidação de comissões de vendedores baseadas nas metas dos objetos de serviço concluídos.

### 4. Setor de Relatórios (Conformidade Legal)
*   **Geração do RAAE (INEA):** Painel interativo para validação e emissão mensal automática do relatório RAAE em conformidade com as regras sanitárias e de controle de insumos químicos.
*   **Balanços Comerciais:** Visualização de vendas e comissões segregadas por profissional e tipo de atividade.
*   **Auditoria de Divergências:** Painel que alerta sobre propostas aprovadas sem agendamento correspondente ou faturamentos inconsistentes.

---

## 🏗️ Fluxo de Integração com o Backend

A comunicação com a API backend C# (.NET 10) respeita os seguintes padrões de governança:

1.  **Headers Multitenancy:** Cada requisição HTTP de dados de clientes ou serviços deve conter o header identificador do CNPJ ativo selecionado.
2.  **Segurança (JWT):** Autenticação e autorização por tokens armazenados de forma segura, garantindo que o usuário web tenha o perfil adequado para operações comerciais e financeiras críticas (que são restritas aos operadores de campo do aplicativo móvel).
3.  **Contratos Robustos:** Consumo de contratos consistentes validados a partir da documentação Scalar/OpenAPI exposta pela API do backend.

---

## ⚙️ Configuração e Inicialização Local (Futuro)

Quando o desenvolvimento for iniciado, o projeto usará a estrutura padrão do ecossistema React:

```bash
# Instalar dependências
npm install

# Executar servidor de desenvolvimento local
npm run dev

# Gerar build de produção otimizado
npm run build
```

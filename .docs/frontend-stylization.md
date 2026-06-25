# Documento de Identidade Visual — ERP de Prestação de Serviços

> Última revisão: Junho/2026
> Baseado em: Refactoring UI (Adam Wathan & Steve Schoger) + UX Guidelines (Steve Krug — Don't Make Me Think)

---

## 1. Princípios de Design

- **Hierarquia visual via peso + contraste + tamanho** — nunca usar apenas tamanho.
- **Espaço em branco generoso** — começar com muito espaço e reduzir; nunca o inverso.
- **Linha máxima legível** — 65–75 caracteres (≈ `max-w: 65ch`).
- **Atenção do usuário é escassa** — cada pixel deve justificar sua presença.

---

## 2. Paleta de Cores HSL

### Primária (Azul)
| Uso | Valor |
|---|---|
| Ações principais (botão, link ativo) | `hsl(221, 83%, 53%)` |
| Hover / Foco | `hsl(221, 83%, 45%)` |
| Fundo suave (tags, apoio) | `hsl(221, 83%, 95%)` |
| Fundo muito suave (muted) | `hsl(221, 83%, 96%)` |

### Escala de Cinza (Hue frio 215)
| Uso | Valor |
|---|---|
| Fundo da aplicação | `hsl(215, 18%, 96%)` |
| Superfícies (cards, header, sidebar) | `hsl(0, 0%, 100%)` |
| Texto principal | `hsl(215, 25%, 12%)` |
| Texto secundário | `hsl(215, 14%, 48%)` |
| Texto terciário / placeholder | `hsl(215, 12%, 65%)` |
| Borda padrão | `hsl(215, 15%, 88%)` |
| Borda sutil | `hsl(215, 15%, 93%)` |

### Sidebar
- Fundo: **branco** `hsl(0, 0%, 100%)` — tema claro, mesma superfície que o header.
- Separador: borda direita 1px `hsl(215, 15%, 88%)` (sem box-shadow pesada).

### Cores de Status (com fundos)
| Status | Texto | Fundo |
|---|---|---|
| Sucesso | `hsl(142, 71%, 34%)` | `hsl(142, 71%, 95%)` |
| Alerta | `hsl(38, 70%, 35%)` | `hsl(38, 92%, 93%)` |
| Erro | `hsl(348, 83%, 42%)` | `hsl(348, 83%, 96%)` |
| Info | `hsl(221, 83%, 45%)` | `hsl(221, 83%, 95%)` |

---

## 3. Tipografia

- **Família**: `Inter` (Google Fonts, pesos 400/500/600/700) → fallback `Segoe UI`, `sans-serif`.

### Escala Tipográfica
| Token | Tamanho | Peso | Line-height | Uso |
|---|---|---|---|---|
| `h1` | **26px** (1.625rem) | 700 | 1.2 | Nome da página |
| `h2` | **20px** (1.25rem) | 600 | 1.3 | Título de seção / card |
| `h3` | **17px** (1.0625rem) | 600 | 1.4 | Subtítulo de card |
| `body` | **16px** (1rem) | 400 | 1.6 | Texto de corpo |
| `text-sm` | **14px** | 400 | 1.5 | Metadados, captions |
| `text-xs` | **12px** | 500 | 1.5 | Labels de formulário |
| `text-2xs` | **11px** | 600 | 1.4 | Tags, chips, "eyebrow" |

> **Regra:** Line-height é **inversamente proporcional** ao tamanho — títulos grandes, menos espaço entre linhas.

### Sidebar (especificidade)
- Módulos pai: **15px**, peso 500.
- Submenus: **15px**, peso 400.

---

## 4. Layout e Espaçamento

### Escala de Espaçamento (Não-Linear)
| Tipo | Valores |
|---|---|
| Gap interno (ícone + texto, botões) | 8px, 10px, 12px, 16px, 20px |
| Padding de card/seção | 32px, 40px, 48px, 64px |
| Padding de página | 40px (`p-10`) ou 48px (`p-12`) |

> **Regra:** Margem externa sempre maior que padding interno.

### Estrutura de Layout
| Elemento | Valor |
|---|---|
| Sidebar | **250px** (fixa) |
| Header | **64px** (h-16) |
| Conteúdo | Flex restante, `overflow-auto` |
| `max-w` padrão de página | `max-w-6xl` (72rem) |

---

## 5. Profundidade e Sombras

| Token | Valor CSS | Uso |
|---|---|---|
| `shadow-soft` | `0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04)` | Botões, inputs com foco |
| `shadow-card` | `0 1px 2px rgba(0,0,0,0.04), 0 0 0 1px rgba(0,0,0,0.04)` | Cards, containers |
| `shadow-composed` | `0 4px 6px -1px rgba(0,0,0,0.06), 0 2px 4px -2px rgba(0,0,0,0.04)` | Modais, dropdowns |
| `shadow-sidebar` | `1px 0 0 0 hsl(215,15%,88%)` | Borda direita da sidebar |

---

## 6. Componentes Padrão

### Botões
```html
<!-- Primário -->
<button class="btn-primary">Ação Principal</button>

<!-- Secundário -->
<button class="btn-secondary">Ação Secundária</button>
```

### Badges de Status
```html
<span class="badge badge-success">Concluído</span>
<span class="badge badge-alert">Pendente</span>
<span class="badge badge-error">Cancelado</span>
<span class="badge badge-info">Em andamento</span>
```

### Chip de Contexto
```html
<span class="chip-success">CNPJ: Matriz</span>
```

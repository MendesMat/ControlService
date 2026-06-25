import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";

// ── Layout (Application Shell) ────────────────────────────────────────────────
import Sidebar from "./components/layout/Sidebar";
import Header  from "./components/layout/Header";

// ── Componente compartilhado de placeholder ───────────────────────────────────
import PlaceholderPage from "./components/shared/PlaceholderPage";


// ── Módulo: Gerenciamento ──────────────────────────────────────────────────────
import NaturezasPage from "./modules/gerenciamento/pages/NaturezasPage";

/**
 * Application Shell.
 * Responsabilidade: composição do layout global e mapeamento de URLs.
 * Não contém lógica de negócio (Clean Architecture — Shell Rule).
 */
function App() {
  return (
    <Router>
      <div className="flex h-screen bg-surface-app text-content-primary font-sans overflow-hidden">
        <Sidebar />

        <div className="flex-1 flex flex-col min-w-0">
          <Header />

          <main className="flex-1 overflow-auto">
            <Routes>
              {/* Redireciona raiz para o módulo inicial (itinerário) */}
              <Route path="/" element={<Navigate to="/comercial/itinerario" replace />} />

              {/* ── Comercial ──────────────────────────────────────────── */}
              <Route path="/comercial"            element={<Navigate to="/comercial/itinerario" replace />} />
              <Route path="/comercial/itinerario" element={<PlaceholderPage title="Itinerário Diário" />} />
              <Route path="/comercial/clientes"   element={<PlaceholderPage title="Lista de Clientes" />} />
              <Route path="/comercial/renovacoes" element={<PlaceholderPage title="Renovações" />} />

              {/* ── Financeiro ─────────────────────────────────────────── */}
              <Route path="/financeiro/receber"   element={<PlaceholderPage title="Contas a Receber" />} />
              <Route path="/financeiro/pagar"     element={<PlaceholderPage title="Contas a Pagar" />} />

              {/* ── Relatórios ─────────────────────────────────────────── */}
              <Route path="/relatorios/raae"      element={<PlaceholderPage title="Relatórios RAAE" />} />
              <Route path="/relatorios/vendas"    element={<PlaceholderPage title="Relatório de Vendas" />} />

              {/* ── Gerenciamento ──────────────────────────────────────── */}
              <Route path="/gerenciamento/usuarios"   element={<PlaceholderPage title="Usuários" />} />
              <Route path="/gerenciamento/permissoes" element={<PlaceholderPage title="Permissões" />} />
              <Route path="/gerenciamento/cnpj"       element={<PlaceholderPage title="Perfis de CNPJ" />} />
              <Route path="/gerenciamento/quimicos"   element={<PlaceholderPage title="Produtos Químicos" />} />
              <Route path="/gerenciamento/naturezas"  element={<NaturezasPage />} />

              {/* Fallback */}
              <Route path="*" element={<Navigate to="/comercial/itinerario" replace />} />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  );
}

export default App;

import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import {
  TrendingUp, Wallet, BarChart2, Settings,
  ChevronDown, ChevronLeft, ChevronRight,
  CalendarDays, Users, RefreshCw,
  Receipt, CreditCard, FileText,
  Building2, HardHat, FlaskConical,
  Layers, FileEdit, Shield, LineChart,
} from "lucide-react";

interface SubMenuItem {
  label: string;
  to: string;
  icon: React.ReactNode;
}

interface MenuModule {
  key: string;
  label: string;
  icon: React.ReactNode;
  items: SubMenuItem[];
}

const menuModules: MenuModule[] = [
  {
    key: "comercial",
    label: "Comercial",
    icon: <TrendingUp size={22} />,
    items: [
      { label: "Itinerário Diário", to: "/comercial/itinerario", icon: <CalendarDays size={18} /> },
      { label: "Lista de Clientes", to: "/comercial/clientes",   icon: <Users size={18} /> },
      { label: "Renovações",        to: "/comercial/renovacoes", icon: <RefreshCw size={18} /> },
    ],
  },
  {
    key: "financeiro",
    label: "Financeiro",
    icon: <Wallet size={22} />,
    items: [
      { label: "Contas a Receber", to: "/financeiro/receber", icon: <Receipt size={18} /> },
      { label: "Contas a Pagar",   to: "/financeiro/pagar",   icon: <CreditCard size={18} /> },
    ],
  },
  {
    key: "relatorios",
    label: "Relatórios",
    icon: <BarChart2 size={22} />,
    items: [
      { label: "Relatórios RAAE",    to: "/relatorios/raae",   icon: <FileText size={18} /> },
      { label: "Relatório de Vendas",to: "/relatorios/vendas", icon: <LineChart size={18} /> },
    ],
  },
  {
    key: "gerenciamento",
    label: "Gerenciamento",
    icon: <Settings size={22} />,
    items: [
      { label: "Usuários",             to: "/gerenciamento/usuarios",   icon: <Users size={18} /> },
      { label: "Permissões",           to: "/gerenciamento/permissoes", icon: <Shield size={18} /> },
      { label: "Perfis CNPJ",          to: "/gerenciamento/cnpj",       icon: <Building2 size={18} /> },
      { label: "Produtos Químicos",    to: "/gerenciamento/quimicos",   icon: <FlaskConical size={18} /> },
      { label: "Naturezas de Serviço", to: "/gerenciamento/naturezas",  icon: <Layers size={18} /> },
    ],
  },
];

function findActiveModuleKey(pathname: string): string {
  const found = menuModules.find((mod) =>
    mod.items.some((item) => pathname.startsWith(item.to))
  );
  return found?.key ?? "comercial";
}

function isSubItemActive(pathname: string, itemPath: string): boolean {
  return pathname === itemPath || pathname.startsWith(itemPath + "/");
}

export default function Sidebar() {
  const location = useLocation();
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [openKey, setOpenKey] = useState<string>(
    () => findActiveModuleKey(location.pathname)
  );

  const toggleModule = (key: string) =>
    setOpenKey((prev) => (prev === key ? "" : key));

  const expandedWidth = 300;
  const collapsedWidth = 68;

  return (
    <aside
      style={{ width: isCollapsed ? collapsedWidth : expandedWidth }}
      className="bg-surface-sidebar shadow-sidebar flex flex-col shrink-0 h-screen overflow-hidden transition-all duration-200 ease-in-out"
    >
      {/* ── Cabeçalho: logo + botão de colapso ── */}
      <div className="h-16 flex items-center justify-between px-5 border-b border-border-subtle shrink-0">
        {!isCollapsed && (
          <span className="font-bold text-xl text-content-primary tracking-tight whitespace-nowrap">
            ERP System
          </span>
        )}
        <button
          aria-label={isCollapsed ? "Expandir menu" : "Recolher menu"}
          onClick={() => setIsCollapsed((prev) => !prev)}
          className={`text-content-secondary hover:text-content-primary hover:bg-surface-app
                      rounded-md p-1.5 transition-colors duration-150 shrink-0
                      ${isCollapsed ? "mx-auto" : "ml-auto"}`}
        >
          {isCollapsed ? <ChevronRight size={20} /> : <ChevronLeft size={20} />}
        </button>
      </div>

      {/* ── Navegação ── */}
      <nav className="flex-1 overflow-y-auto overflow-x-hidden px-2 py-4">
        {menuModules.map((mod) => {
          const isOpen = !isCollapsed && openKey === mod.key;
          const hasActiveChild = mod.items.some((item) =>
            location.pathname.startsWith(item.to)
          );

          return (
            <div key={mod.key} className="mb-0.5">
              <button
                aria-expanded={isOpen}
                title={isCollapsed ? mod.label : undefined}
                onClick={() => {
                  if (isCollapsed) setIsCollapsed(false);
                  toggleModule(mod.key);
                }}
                className={`sidebar-module-btn ${hasActiveChild ? "active" : ""} ${isCollapsed ? "justify-center" : ""}`}
              >
                <span className={`shrink-0 ${hasActiveChild ? "text-primary" : "text-content-secondary"}`}>
                  {mod.icon}
                </span>
                {!isCollapsed && (
                  <>
                    <span className="flex-1 text-left truncate">{mod.label}</span>
                    <ChevronDown
                      size={16}
                      className={`shrink-0 text-content-secondary transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
                    />
                  </>
                )}
              </button>

              {/* Submenus — só visíveis quando expandido */}
              {!isCollapsed && (
                <div className={`overflow-hidden transition-all duration-200 ease-out ${isOpen ? "max-h-96 opacity-100" : "max-h-0 opacity-0"}`}>
                  <div className="ml-3 pl-3 border-l border-border-subtle mt-0.5 mb-1 flex flex-col gap-0.5">
                    {mod.items.map((item) => (
                      <NavLink
                        key={item.to}
                        to={item.to}
                        className={`sidebar-sub-link ${isSubItemActive(location.pathname, item.to) ? "active-link" : ""}`}
                      >
                        <span className={`shrink-0 ${isSubItemActive(location.pathname, item.to) ? "text-primary" : "text-content-secondary"}`}>
                          {item.icon}
                        </span>
                        {item.label}
                      </NavLink>
                    ))}
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </nav>
    </aside>
  );
}

import { useState } from "react";

export interface TabItem {
  id: string;
  label: string;
  content: React.ReactNode;
}

interface TabsProps {
  tabs: TabItem[];
  defaultTabId?: string;
}

/**
 * Componente de Tabs reutilizável.
 * Mantém o estado ativo internamente e renderiza o conteúdo da aba correspondente.
 */
export default function Tabs({ tabs, defaultTabId }: TabsProps) {
  const [activeTab, setActiveTab] = useState(defaultTabId ?? tabs[0]?.id);

  if (!tabs || tabs.length === 0) return null;

  const activeContent = tabs.find((t) => t.id === activeTab)?.content;

  return (
    <div className="flex flex-col w-full">
      {/* ── Navegação das Abas ── */}
      <div className="flex border-b border-border mb-6">
        {tabs.map((tab) => {
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              aria-selected={isActive}
              role="tab"
              className={`px-6 py-3 font-medium text-base transition-colors duration-150 border-b-2 -mb-[1px]
                ${
                  isActive
                    ? "border-primary text-primary"
                    : "border-transparent text-content-secondary hover:text-content-primary hover:border-border"
                }
              `}
            >
              {tab.label}
            </button>
          );
        })}
      </div>

      {/* ── Conteúdo da Aba Ativa ── */}
      <div role="tabpanel" className="flex-1">
        {activeContent}
      </div>
    </div>
  );
}

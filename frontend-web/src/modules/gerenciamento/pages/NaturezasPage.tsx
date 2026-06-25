import { useState } from "react";
import { Plus, Search } from "lucide-react";
import Tabs from "../../../components/shared/Tabs";
import type { TabItem } from "../../../components/shared/Tabs";
import type { NaturezaServico, ObjetoServico } from "../../../types/gerenciamento";

// ── Mocks Temporários ────────────────────────────────────────────────────────
const MOCK_NATUREZAS: NaturezaServico[] = [
  {
    id: "nat-1",
    nome: "Limpeza Urbana e Conservação",
    codigoTributacao: "11.01",
    codigoTributacaoMunicipal: "1101",
  },
  {
    id: "nat-2",
    nome: "Manutenção Predial (Elétrica/Hidráulica)",
    codigoTributacao: "07.10",
    codigoTributacaoMunicipal: "0710",
  },
  {
    id: "nat-3",
    nome: "Consultoria Ambiental",
    codigoTributacao: "17.05",
    codigoTributacaoMunicipal: "1705",
  },
];

const MOCK_OBJETOS: ObjetoServico[] = [
  { id: "obj-1", nome: "Varrição de Vias Públicas", naturezasIds: ["nat-1"] },
  { id: "obj-2", nome: "Poda de Árvores", naturezasIds: ["nat-1", "nat-3"] },
  { id: "obj-3", nome: "Troca de Lâmpadas (Postes)", naturezasIds: ["nat-2"] },
];

// ── Componentes de Aba ────────────────────────────────────────────────────────

function NaturezasTab() {
  return (
    <div className="flex flex-col gap-6">
      {/* Ações (Busca + Novo) */}
      <div className="flex items-center justify-between">
        <div className="relative">
          <Search
            size={18}
            className="absolute left-3.5 top-1/2 -translate-y-1/2 text-content-secondary"
          />
          <input
            type="text"
            placeholder="Buscar natureza..."
            className="pl-11 pr-4 py-2.5 bg-surface-app border border-border rounded-lg text-base
                       focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/40
                       transition-all w-80 placeholder:text-content-secondary"
          />
        </div>
        <button className="btn-primary">
          <Plus size={20} />
          Nova Natureza
        </button>
      </div>

      {/* Tabela de Naturezas */}
      <div className="bg-surface border border-border rounded-xl overflow-hidden shadow-card">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-surface-app border-b border-border">
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider">
                Nome da Natureza
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider">
                Cód. Tributação
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider">
                Cód. Tributação Municipal
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider text-right">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {MOCK_NATUREZAS.map((nat) => (
              <tr
                key={nat.id}
                className="hover:bg-surface-app transition-colors"
              >
                <td className="px-6 py-4 font-medium text-content-primary">
                  {nat.nome}
                </td>
                <td className="px-6 py-4 text-content-secondary">
                  {nat.codigoTributacao}
                </td>
                <td className="px-6 py-4 text-content-secondary">
                  {nat.codigoTributacaoMunicipal}
                </td>
                <td className="px-6 py-4 text-right">
                  <button className="text-primary hover:text-primary-hover font-medium transition-colors">
                    Editar
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function ObjetosTab() {
  // Helper para buscar nomes das naturezas
  const getNaturezaNames = (ids: string[]) => {
    return ids
      .map((id) => MOCK_NATUREZAS.find((n) => n.id === id)?.nome)
      .filter(Boolean);
  };

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <div className="relative">
          <Search
            size={18}
            className="absolute left-3.5 top-1/2 -translate-y-1/2 text-content-secondary"
          />
          <input
            type="text"
            placeholder="Buscar objeto..."
            className="pl-11 pr-4 py-2.5 bg-surface-app border border-border rounded-lg text-base
                       focus:outline-none focus:ring-2 focus:ring-primary/30 focus:border-primary/40
                       transition-all w-80 placeholder:text-content-secondary"
          />
        </div>
        <button className="btn-primary">
          <Plus size={20} />
          Novo Objeto
        </button>
      </div>

      <div className="bg-surface border border-border rounded-xl overflow-hidden shadow-card">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-surface-app border-b border-border">
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider">
                Nome do Objeto
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider">
                Naturezas Atreladas
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-content-secondary uppercase tracking-wider text-right">
                Ações
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {MOCK_OBJETOS.map((obj) => {
              const naturezas = getNaturezaNames(obj.naturezasIds);
              return (
                <tr
                  key={obj.id}
                  className="hover:bg-surface-app transition-colors"
                >
                  <td className="px-6 py-4 font-medium text-content-primary">
                    {obj.nome}
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex flex-wrap gap-2">
                      {naturezas.map((natName, idx) => (
                        <span
                          key={idx}
                          className="inline-flex items-center px-2.5 py-1 rounded-md text-xs font-medium bg-primary-muted text-primary border border-primary/20"
                        >
                          {natName}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button className="text-primary hover:text-primary-hover font-medium transition-colors">
                      Editar
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}

// ── Página Principal ──────────────────────────────────────────────────────────

export default function NaturezasPage() {
  const tabs: TabItem[] = [
    { id: "naturezas", label: "Naturezas de Serviço", content: <NaturezasTab /> },
    { id: "objetos", label: "Objetos de Serviço", content: <ObjetosTab /> },
  ];

  return (
    <div className="p-10 max-w-7xl mx-auto flex flex-col min-h-0">
      <div className="mb-8">
        <h1>Naturezas e Objetos de Serviço</h1>
        <p className="text-content-secondary mt-1">
          Gerencie as categorias de tributação e os serviços específicos atrelados
          a elas.
        </p>
      </div>

      <div className="flex-1">
        <Tabs tabs={tabs} defaultTabId="naturezas" />
      </div>
    </div>
  );
}

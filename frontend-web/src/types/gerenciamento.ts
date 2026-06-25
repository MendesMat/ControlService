import type { AuditFields } from "./shared";

/**
 * Representa uma Natureza de Serviço.
 * Uma categoria primária que agrupa vários Objetos de Serviço.
 */
export interface NaturezaServico extends AuditFields {
  id: string;
  nome: string;
  codigoTributacao: string;
  codigoTributacaoMunicipal: string;
}

/**
 * Representa um Objeto de Serviço.
 * Um serviço específico que pode estar atrelado a uma ou mais Naturezas.
 */
export interface ObjetoServico extends AuditFields {
  id: string;
  nome: string;
  // Na resposta da API, provavelmente virá a lista de IDs ou os objetos parciais das naturezas
  naturezasIds: string[];
}

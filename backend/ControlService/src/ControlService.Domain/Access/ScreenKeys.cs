namespace ControlService.Domain.Access;

/// <summary>Stable screen keys owned by the back-end (ADR-0021). Mirrors docs/product/screen-catalog.json.</summary>
public static class ScreenKeys
{
    public const string GerenciamentoUsuarios = "gerenciamento/usuarios";
    public const string GerenciamentoPermissoes = "gerenciamento/permissoes";
    public const string GerenciamentoPerfisCnpj = "gerenciamento/perfis-cnpj";
    public const string GerenciamentoNaturezasDeServico = "gerenciamento/naturezas-de-servico";
    public const string GerenciamentoObjetosDeServico = "gerenciamento/objetos-de-servico";
    public const string GerenciamentoProdutos = "gerenciamento/produtos";
    public const string GerenciamentoGarantias = "gerenciamento/garantias";
    public const string GerenciamentoFormasDePagamento = "gerenciamento/formas-de-pagamento";
    public const string GerenciamentoVeiculos = "gerenciamento/veiculos";

    public const string ComercialClientes = "comercial/clientes";
    public const string ComercialRoteiroDiario = "comercial/roteiro-diario";
    public const string ComercialRoteiroMensal = "comercial/roteiro-mensal";
    public const string ComercialAcompanhamento = "comercial/acompanhamento";
    public const string ComercialRenovacoes = "comercial/renovacoes";

    public const string FinanceiroContasAReceber = "financeiro/contas-a-receber";
    public const string FinanceiroContasAPagar = "financeiro/contas-a-pagar";

    public const string RelatoriosRelatorioDeVendas = "relatorios/relatorio-de-vendas";
    public const string RelatoriosRaae = "relatorios/raae";
    public const string RelatoriosIncongruencias = "relatorios/incongruencias";
    public const string RelatoriosCustoXFaturamento = "relatorios/custo-x-faturamento";

    public static readonly IReadOnlyCollection<string> All =
    [
        GerenciamentoUsuarios,
        GerenciamentoPermissoes,
        GerenciamentoPerfisCnpj,
        GerenciamentoNaturezasDeServico,
        GerenciamentoObjetosDeServico,
        GerenciamentoProdutos,
        GerenciamentoGarantias,
        GerenciamentoFormasDePagamento,
        GerenciamentoVeiculos,
        ComercialClientes,
        ComercialRoteiroDiario,
        ComercialRoteiroMensal,
        ComercialAcompanhamento,
        ComercialRenovacoes,
        FinanceiroContasAReceber,
        FinanceiroContasAPagar,
        RelatoriosRelatorioDeVendas,
        RelatoriosRaae,
        RelatoriosIncongruencias,
        RelatoriosCustoXFaturamento,
    ];
}

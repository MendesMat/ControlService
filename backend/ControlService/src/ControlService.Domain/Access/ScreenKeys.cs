namespace ControlService.Domain.Access;

/// <summary>Stable screen keys owned by the back-end (ADR-0021). Mirrors docs/product/screen-catalog.json.</summary>
public static class ScreenKeys
{
    public const string Users = "gerenciamento/usuarios";
    public const string PermissionProfiles = "gerenciamento/permissoes";
    public const string CnpjProfiles = "gerenciamento/perfis-cnpj";
    public const string ServiceNatures = "gerenciamento/naturezas-de-servico";
    public const string ServiceObjects = "gerenciamento/objetos-de-servico";
    public const string Products = "gerenciamento/produtos";
    public const string Warranties = "gerenciamento/garantias";
    public const string PaymentMethods = "gerenciamento/formas-de-pagamento";
    public const string Vehicles = "gerenciamento/veiculos";

    public const string Customers = "comercial/clientes";
    public const string DailyRoute = "comercial/roteiro-diario";
    public const string MonthlyRoute = "comercial/roteiro-mensal";
    public const string FollowUp = "comercial/acompanhamento";
    public const string Renewals = "comercial/renovacoes";

    public const string AccountsReceivable = "financeiro/contas-a-receber";
    public const string AccountsPayable = "financeiro/contas-a-pagar";

    public const string SalesReport = "relatorios/relatorio-de-vendas";
    public const string RaaeReport = "relatorios/raae";
    public const string InconsistencyReport = "relatorios/incongruencias";
    public const string CostVersusRevenueReport = "relatorios/custo-x-faturamento";

    public static readonly IReadOnlyCollection<string> All =
    [
        Users,
        PermissionProfiles,
        CnpjProfiles,
        ServiceNatures,
        ServiceObjects,
        Products,
        Warranties,
        PaymentMethods,
        Vehicles,
        Customers,
        DailyRoute,
        MonthlyRoute,
        FollowUp,
        Renewals,
        AccountsReceivable,
        AccountsPayable,
        SalesReport,
        RaaeReport,
        InconsistencyReport,
        CostVersusRevenueReport,
    ];
}

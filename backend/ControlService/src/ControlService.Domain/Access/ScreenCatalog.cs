namespace ControlService.Domain.Access;

/// <summary>The areas and screens as the menu shows them, in menu order. Mirrors docs/product/screen-catalog.json;
/// the keys come from <see cref="ScreenKeys"/>, so a key never changes with a name.</summary>
public static class ScreenCatalog
{
    public static readonly IReadOnlyList<ScreenArea> Areas =
    [
        new("gerenciamento", "Gerenciamento",
        [
            new(ScreenKeys.Users, "Usuários"),
            new(ScreenKeys.PermissionProfiles, "Permissões"),
            new(ScreenKeys.CnpjProfiles, "Perfis CNPJ"),
            new(ScreenKeys.ServiceNatures, "Naturezas de Serviço"),
            new(ScreenKeys.ServiceObjects, "Objetos de Serviço"),
            new(ScreenKeys.Products, "Produtos"),
            new(ScreenKeys.Warranties, "Garantias"),
            new(ScreenKeys.PaymentMethods, "Formas de Pagamento"),
            new(ScreenKeys.Vehicles, "Veículos"),
        ]),
        new("comercial", "Comercial",
        [
            new(ScreenKeys.Customers, "Clientes"),
            new(ScreenKeys.DailyRoute, "Roteiro Diário"),
            new(ScreenKeys.MonthlyRoute, "Roteiro Mensal"),
            new(ScreenKeys.FollowUp, "Acompanhamento"),
            new(ScreenKeys.Renewals, "Renovações"),
        ]),
        new("financeiro", "Financeiro",
        [
            new(ScreenKeys.AccountsReceivable, "Contas a Receber"),
            new(ScreenKeys.AccountsPayable, "Contas a Pagar"),
        ]),
        new("relatorios", "Relatórios",
        [
            new(ScreenKeys.SalesReport, "Relatório de Vendas"),
            new(ScreenKeys.RaaeReport, "RAAE"),
            new(ScreenKeys.InconsistencyReport, "Incongruências"),
            new(ScreenKeys.CostVersusRevenueReport, "Custo x Faturamento"),
        ]),
    ];
}

public sealed record ScreenArea(string Id, string Label, IReadOnlyList<CatalogScreen> Screens);

public sealed record CatalogScreen(string Key, string Label);

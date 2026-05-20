using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Financial.Infrastructure.Data;

public class FinancialDbContextFactory : IDesignTimeDbContextFactory<FinancialDbContext>
{
    public FinancialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinancialDbContext>();

        // Configurado para PostgreSQL com tabela de histórico de migrations customizada
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ControlService;Username=postgres;Password=postgres",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_Financial", "public")
        );

        return new FinancialDbContext(optionsBuilder.Options);
    }
}

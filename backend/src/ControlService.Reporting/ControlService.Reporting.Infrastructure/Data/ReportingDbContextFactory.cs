using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Reporting.Infrastructure.Data;

public class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
    public ReportingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();

        // Configurado para PostgreSQL com tabela de histórico de migrations customizada
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ControlService;Username=postgres;Password=postgres",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_Reporting", "public")
        );

        return new ReportingDbContext(optionsBuilder.Options);
    }
}

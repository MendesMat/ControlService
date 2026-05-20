using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Operational.Infrastructure.Data;

public class OperationalDbContextFactory : IDesignTimeDbContextFactory<OperationalDbContext>
{
    public OperationalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OperationalDbContext>();

        // Configurado para PostgreSQL com tabela de histórico de migrations customizada
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ControlService;Username=postgres;Password=postgres",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_Operational", "public")
        );

        return new OperationalDbContext(optionsBuilder.Options);
    }
}

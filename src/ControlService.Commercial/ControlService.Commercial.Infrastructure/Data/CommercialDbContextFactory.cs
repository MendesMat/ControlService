using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Commercial.Infrastructure.Data;

public class CommercialDbContextFactory : IDesignTimeDbContextFactory<CommercialDbContext>
{
    public CommercialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommercialDbContext>();

        // Configurado para PostgreSQL com tabela de histórico de migrations customizada
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ControlService;Username=postgres;Password=postgres",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_Commercial", "public")
        );

        return new CommercialDbContext(optionsBuilder.Options);
    }
}

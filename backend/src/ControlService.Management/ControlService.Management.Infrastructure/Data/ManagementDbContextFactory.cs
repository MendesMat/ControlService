using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Management.Infrastructure.Data;

public class ManagementDbContextFactory : IDesignTimeDbContextFactory<ManagementDbContext>
{
    public ManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ManagementDbContext>();

        // Configurado para PostgreSQL com tabela de histórico de migrations customizada
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=ControlService;Username=postgres;Password=postgres",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_Management", "public")
        );

        return new ManagementDbContext(optionsBuilder.Options);
    }
}

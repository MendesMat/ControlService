using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ControlService.Infrastructure.Data;

public class ControlServiceDbContextFactory : IDesignTimeDbContextFactory<ControlServiceDbContext>
{
    public ControlServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ControlServiceDbContext>();

        // Esta factory é usada apenas em tempo de design (migrações)
        // O valor real da connection string não importa para gerar o código da migration,
        // mas o provedor (SqlServer) deve ser o mesmo.
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ControlService;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ControlServiceDbContext(optionsBuilder.Options);
    }
}

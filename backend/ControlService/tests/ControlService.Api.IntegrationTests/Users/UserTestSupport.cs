using ControlService.Domain.Users;
using ControlService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Users;

/// <summary>What the users endpoint tests repeat.</summary>
internal static class UserTestSupport
{
    public const string UsersPath = "/api/v1/users";

    public const string SuggestionPath = "/api/v1/users/login-suggestion";

    public const string TemporaryPassword = "temporaria-1";

    /// <summary>A complete, valid request body with fictitious data. `marker` makes the login and the names
    /// unique, since the database is shared by the whole assembly. Tests change it with indexers.</summary>
    public static Dictionary<string, object?> NewUserBody(string marker) => new()
    {
        ["login"] = $"ana.{marker}",
        ["email"] = $"ana.{marker}@example.com",
        ["fullName"] = $"Ana Paula Souza {marker}",
        ["displayName"] = $"Ana {marker}",
        ["cpf"] = "529.982.247-25",
        ["rg"] = "12.345.678-9",
        ["birthDate"] = "1990-04-17",
        ["bloodType"] = "O+",
        ["phone"] = "(21) 98765-4321",
        ["address"] = new Dictionary<string, object?>
        {
            ["cep"] = "20040-020",
            ["street"] = "Rua da Assembleia",
            ["number"] = "10",
            ["complement"] = "sala 501",
            ["district"] = "Centro",
            ["city"] = "Rio de Janeiro",
        },
        ["emergencyContact"] = new Dictionary<string, object?>
        {
            ["name"] = "Marcos Souza",
            ["relationship"] = "irmão",
            ["phone"] = "(21) 99876-5432",
        },
        ["profileIds"] = Array.Empty<Guid>(),
        ["temporaryPassword"] = TemporaryPassword,
    };

    /// <summary>Reads the user straight from the database, outside the API.</summary>
    public static async Task<User> FindUserAsync(IServiceProvider services, Guid id)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Users.AsNoTracking().SingleAsync(user => user.Id == id);
    }
}

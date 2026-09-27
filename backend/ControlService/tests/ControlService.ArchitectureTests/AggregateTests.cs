using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;

namespace ControlService.ArchitectureTests;

public sealed class AggregateTests
{
    [Fact]
    public void Domain_aggregates_have_no_public_setters() // issue #5 done-when, ADR-0006
    {
        Type[] aggregates = [typeof(User), typeof(PermissionProfile)];

        foreach (var aggregate in aggregates)
        {
            foreach (var property in aggregate.GetProperties())
            {
                property.GetSetMethod(nonPublic: false)
                    .ShouldBeNull($"{aggregate.Name}.{property.Name} exposes a public setter.");
            }
        }
    }
}

using ControlService.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace ControlService.Api.IntegrationTests.Auth;

public sealed class CredentialStoreTests(AuthApiFactory factory) : IClassFixture<AuthApiFactory>
{
    private const string WrongPassword = "senha-errada-123";

    [Fact]
    public async Task Credential_store_locks_the_login_on_the_fifth_consecutive_failure() // AUTH-08
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICredentialStore>();

        for (var attempt = 1; attempt <= 4; attempt++)
        {
            var failure = await store.CheckPasswordAsync(user.Id, WrongPassword, TestContext.Current.CancellationToken);
            failure.Outcome.ShouldBe(CredentialCheckOutcome.Failed);
        }

        var fifth = await store.CheckPasswordAsync(user.Id, WrongPassword, TestContext.Current.CancellationToken);

        fifth.Outcome.ShouldBe(CredentialCheckOutcome.LockedOut);
        fifth.LockoutEnd.ShouldBe(factory.Clock.GetUtcNow().AddMinutes(15));
    }

    [Fact]
    public async Task Lockout_ends_after_the_configured_minutes() // AUTH-08
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICredentialStore>();
        await FailAsync(store, user.Id, 5);
        (await store.CheckPasswordAsync(user.Id, user.Password, TestContext.Current.CancellationToken))
            .Outcome.ShouldBe(CredentialCheckOutcome.LockedOut);

        factory.Clock.Advance(TimeSpan.FromMinutes(15));

        (await store.CheckPasswordAsync(user.Id, user.Password, TestContext.Current.CancellationToken))
            .Outcome.ShouldBe(CredentialCheckOutcome.Succeeded);
    }

    [Fact]
    public async Task A_successful_check_resets_the_failure_count() // AUTH-08
    {
        var user = await AuthTestSupport.CreateUserAsync(factory.Services);
        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<ICredentialStore>();
        await FailAsync(store, user.Id, 4);
        (await store.CheckPasswordAsync(user.Id, user.Password, TestContext.Current.CancellationToken))
            .Outcome.ShouldBe(CredentialCheckOutcome.Succeeded);

        var outcomes = await FailAsync(store, user.Id, 4);

        outcomes.ShouldAllBe(outcome => outcome == CredentialCheckOutcome.Failed);
    }

    private static async Task<List<CredentialCheckOutcome>> FailAsync(ICredentialStore store, Guid userId, int attempts)
    {
        var outcomes = new List<CredentialCheckOutcome>();
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            outcomes.Add((await store.CheckPasswordAsync(userId, WrongPassword, TestContext.Current.CancellationToken)).Outcome);
        }

        return outcomes;
    }
}

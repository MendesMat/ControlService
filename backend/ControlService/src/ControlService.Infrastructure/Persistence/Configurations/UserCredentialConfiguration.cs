using ControlService.Domain.Users;
using ControlService.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlService.Infrastructure.Persistence.Configurations;

public sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("user_credentials");
        builder.Property(credential => credential.MustChangePassword).IsRequired();
        builder.HasIndex(credential => credential.NormalizedUserName).HasDatabaseName("ix_user_credentials_normalized_user_name").IsUnique();

        // The credential belongs to a user and never outlives it (AUTH-19).
        builder.HasOne<User>().WithOne().HasForeignKey<UserCredential>(credential => credential.Id).OnDelete(DeleteBehavior.Restrict);

        // Identity's satellite tables are required by its EF store but unused here; only their
        // leftover default names (asp_net_users, EmailIndex) are replaced.
        builder.HasIndex(credential => credential.NormalizedEmail).HasDatabaseName("ix_user_credentials_normalized_email");
        builder.HasMany<IdentityUserClaim<Guid>>().WithOne().HasForeignKey(claim => claim.UserId)
            .HasConstraintName("fk_user_credential_claims_user_credentials_user_id");
        builder.HasMany<IdentityUserLogin<Guid>>().WithOne().HasForeignKey(login => login.UserId)
            .HasConstraintName("fk_user_credential_logins_user_credentials_user_id");
        builder.HasMany<IdentityUserToken<Guid>>().WithOne().HasForeignKey(token => token.UserId)
            .HasConstraintName("fk_user_credential_tokens_user_credentials_user_id");
    }
}

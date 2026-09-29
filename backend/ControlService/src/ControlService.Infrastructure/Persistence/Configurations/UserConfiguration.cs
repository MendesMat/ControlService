using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.IsSystem).IsRequired();

        builder.Property(user => user.Login)
            .HasConversion(login => login.Value, value => Login.Create(value).Value)
            .HasColumnName("login")
            .IsRequired();
        builder.HasIndex(user => user.Login).IsUnique().HasDatabaseName("ix_users_login");

        builder.Property(user => user.Email)
            .HasConversion(email => email.Value, value => EmailAddress.Create(value).Value)
            .HasColumnName("email")
            .IsRequired();

        builder.Property(user => user.DisplayName).IsRequired();
        builder.Property(user => user.NormalizedDisplayName).HasColumnName("display_name_normalized").IsRequired();
        builder.HasIndex(user => user.NormalizedDisplayName).IsUnique().HasDatabaseName("ix_users_display_name_normalized");

        builder.Property(user => user.FullName).IsRequired();

        builder.Property(user => user.Status).HasConversion<string>().IsRequired();

        builder.Property(user => user.Version).IsRowVersion();

        builder.HasOne<User>().WithMany().HasForeignKey(user => user.DeactivatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(user => user.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(user => user.UpdatedBy).OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(user => user.ProfileIds);

        builder.OwnsMany<ProfileAssignment>("_profileAssignments", assignment =>
        {
            assignment.ToTable("user_permission_profiles");
            assignment.WithOwner().HasForeignKey("UserId");
            assignment.Property(a => a.ProfileId).HasColumnName("profile_id");
            assignment.HasKey("UserId", nameof(ProfileAssignment.ProfileId));
            assignment.HasOne<PermissionProfile>().WithMany().HasForeignKey(a => a.ProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Navigation("_profileAssignments").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

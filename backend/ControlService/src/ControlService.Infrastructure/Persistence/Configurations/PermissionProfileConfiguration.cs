using ControlService.Domain.Access;
using ControlService.Domain.PermissionProfiles;
using ControlService.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlService.Infrastructure.Persistence.Configurations;

public sealed class PermissionProfileConfiguration : IEntityTypeConfiguration<PermissionProfile>
{
    public void Configure(EntityTypeBuilder<PermissionProfile> builder)
    {
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.Property(profile => profile.IsSystem).IsRequired();

        builder.Property(profile => profile.Name).IsRequired();
        builder.Property(profile => profile.NormalizedName).IsRequired();
        builder.HasIndex(profile => profile.NormalizedName).IsUnique().HasDatabaseName("ix_permission_profiles_name_normalized");

        builder.Property(profile => profile.Description).IsRequired();

        builder.Property(profile => profile.Version).IsRowVersion();

        builder.HasOne<User>().WithMany().HasForeignKey(profile => profile.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(profile => profile.UpdatedBy).OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(profile => profile.Levels);

        builder.OwnsMany<ScreenLevel>("_levels", level =>
        {
            level.ToTable("permission_profile_levels");
            level.WithOwner().HasForeignKey("PermissionProfileId");
            level.Property(l => l.Screen)
                .HasConversion(screen => screen.Value, value => ScreenKey.Create(value).Value)
                .HasColumnName("screen_key");
            level.Property(l => l.Level).HasConversion<short>().HasColumnName("level");
            level.HasKey("PermissionProfileId", nameof(ScreenLevel.Screen));
        });
    }
}

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

        // A blank optional value is NULL in the database; the API answers it as "" (CNV-03). The converters are
        // not called for NULL, so a missing CPF, phone, CEP or blood type never reaches them.
        builder.ComplexProperty(user => user.PersonalData, personal =>
        {
            personal.Property(data => data.Cpf)
                .HasConversion(cpf => cpf!.Value, value => Cpf.Create(value).Value).HasColumnName("cpf");
            personal.Property(data => data.Rg).HasColumnName("rg").IsRequired();
            personal.Property(data => data.BirthDate).HasColumnName("birth_date");
            personal.Property(data => data.BloodType)
                .HasConversion(bloodType => bloodType!.Value, value => BloodType.Create(value).Value).HasColumnName("blood_type");
            personal.Property(data => data.Phone)
                .HasConversion(phone => phone!.Value, value => PhoneNumber.Create(value).Value).HasColumnName("phone");

            personal.ComplexProperty(data => data.Address, address =>
            {
                address.Property(part => part.Cep)
                    .HasConversion(cep => cep!.Value, value => Cep.Create(value).Value).HasColumnName("address_cep");
                address.Property(part => part.Street).HasColumnName("address_street").IsRequired();
                address.Property(part => part.Number).HasColumnName("address_number").IsRequired();
                address.Property(part => part.Complement).HasColumnName("address_complement").IsRequired();
                address.Property(part => part.District).HasColumnName("address_district").IsRequired();
                address.Property(part => part.City).HasColumnName("address_city").IsRequired();
            });

            personal.ComplexProperty(data => data.EmergencyContact, contact =>
            {
                contact.Property(part => part.Name).HasColumnName("emergency_contact_name").IsRequired();
                contact.Property(part => part.Relationship).HasColumnName("emergency_contact_relationship").IsRequired();
                contact.Property(part => part.Phone)
                    .HasConversion(phone => phone!.Value, value => PhoneNumber.Create(value).Value)
                    .HasColumnName("emergency_contact_phone");
            });
        });

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

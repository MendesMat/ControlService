using ControlService.Domain.Commercial.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControlService.Infrastructure.Data.Configurations.Commercial;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.LegalName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.TradeName)
            .HasMaxLength(200);

        builder.Property(c => c.Activity)
            .HasMaxLength(150);

        builder.Property(c => c.OperationalNote)
            .HasMaxLength(500);

        builder.Property(c => c.FinancialNote)
            .HasMaxLength(500);

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        // OwnsOne for Document (ValueObject)
        builder.OwnsOne(c => c.Document, doc =>
        {
            doc.Property(d => d.Value)
               .HasColumnName("Document")
               .HasMaxLength(14);

            doc.Property(d => d.Type)
               .HasColumnName("DocumentType");
        });

        // OwnsOne for MunicipalInscription
        builder.OwnsOne(c => c.MunicipalInscription, inc =>
        {
            inc.Property(i => i.Value)
               .HasColumnName("MunicipalInscription")
               .HasMaxLength(20);

            inc.Property(i => i.Type)
               .HasColumnName("MunicipalInscriptionType");
        });

        // OwnsOne for StateInscription
        builder.OwnsOne(c => c.StateInscription, inc =>
        {
            inc.Property(i => i.Value)
               .HasColumnName("StateInscription")
               .HasMaxLength(20);

            inc.Property(i => i.Type)
               .HasColumnName("StateInscriptionType");
        });

        // OwnsOne for Address
        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(10).IsRequired();
            address.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(200).IsRequired();
            address.Property(a => a.Number).HasColumnName("Address_Number").HasMaxLength(50);
            address.Property(a => a.Complement).HasColumnName("Address_Complement").HasMaxLength(100);
            address.Property(a => a.Neighborhood).HasColumnName("Address_Neighborhood").HasMaxLength(100).IsRequired();
            address.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(2).IsRequired();
        });

        // Collections Mapping
        builder.OwnsMany(c => c.Phones, p =>
        {
            p.ToTable("CustomerPhones");
            p.Property(phone => phone.Value).HasMaxLength(20).IsRequired();
            p.Property(phone => phone.Type).IsRequired();
            p.WithOwner().HasForeignKey("CustomerId");
            p.HasKey("Id"); // Shadow property
        });

        builder.OwnsMany(c => c.Emails, e =>
        {
            e.ToTable("CustomerEmails");
            e.Property(email => email.Value).HasMaxLength(150).IsRequired();
            e.Property(email => email.Type).IsRequired();
            e.WithOwner().HasForeignKey("CustomerId");
            e.HasKey("Id"); // Shadow property
        });

        builder.Property(c => c.ContactPersons)
            .HasColumnName("ContactPersons");
    }
}

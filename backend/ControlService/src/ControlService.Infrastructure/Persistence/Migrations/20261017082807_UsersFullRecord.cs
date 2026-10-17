using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlService.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UsersFullRecord : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The status "Pending" belonged to the plan with activation links (decision 21): everyone is active now.
        migrationBuilder.Sql("UPDATE users SET status = 'Active' WHERE status = 'Pending';");

        migrationBuilder.DropColumn(
            name: "activated_at",
            table: "users");

        migrationBuilder.AddColumn<string>(
            name: "address_cep",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "address_city",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "address_complement",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "address_district",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "address_number",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "address_street",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateOnly>(
            name: "birth_date",
            table: "users",
            type: "date",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "blood_type",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "cpf",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "emergency_contact_name",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "emergency_contact_phone",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "emergency_contact_relationship",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "phone",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "rg",
            table: "users",
            type: "text",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "address_cep",
            table: "users");

        migrationBuilder.DropColumn(
            name: "address_city",
            table: "users");

        migrationBuilder.DropColumn(
            name: "address_complement",
            table: "users");

        migrationBuilder.DropColumn(
            name: "address_district",
            table: "users");

        migrationBuilder.DropColumn(
            name: "address_number",
            table: "users");

        migrationBuilder.DropColumn(
            name: "address_street",
            table: "users");

        migrationBuilder.DropColumn(
            name: "birth_date",
            table: "users");

        migrationBuilder.DropColumn(
            name: "blood_type",
            table: "users");

        migrationBuilder.DropColumn(
            name: "cpf",
            table: "users");

        migrationBuilder.DropColumn(
            name: "emergency_contact_name",
            table: "users");

        migrationBuilder.DropColumn(
            name: "emergency_contact_phone",
            table: "users");

        migrationBuilder.DropColumn(
            name: "emergency_contact_relationship",
            table: "users");

        migrationBuilder.DropColumn(
            name: "phone",
            table: "users");

        migrationBuilder.DropColumn(
            name: "rg",
            table: "users");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "activated_at",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);
    }
}

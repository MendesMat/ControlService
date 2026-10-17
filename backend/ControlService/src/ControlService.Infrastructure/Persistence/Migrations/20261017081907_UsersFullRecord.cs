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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "activated_at",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);
    }
}

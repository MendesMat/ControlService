using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlService.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                is_system = table.Column<bool>(type: "boolean", nullable: false),
                login = table.Column<string>(type: "text", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                display_name = table.Column<string>(type: "text", nullable: false),
                normalized_display_name = table.Column<string>(type: "text", nullable: false),
                full_name = table.Column<string>(type: "text", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                activated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deactivated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                deactivated_by = table.Column<Guid>(type: "uuid", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
                table.ForeignKey(
                    name: "fk_users_users_created_by",
                    column: x => x.created_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_users_users_deactivated_by",
                    column: x => x.deactivated_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_users_users_updated_by",
                    column: x => x.updated_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "permission_profiles",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                is_system = table.Column<bool>(type: "boolean", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                normalized_name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission_profiles", x => x.id);
                table.ForeignKey(
                    name: "fk_permission_profiles_users_created_by",
                    column: x => x.created_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_permission_profiles_users_updated_by",
                    column: x => x.updated_by,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "permission_profile_levels",
            columns: table => new
            {
                screen_key = table.Column<string>(type: "text", nullable: false),
                permission_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                level = table.Column<short>(type: "smallint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission_profile_levels", x => new { x.permission_profile_id, x.screen_key });
                table.ForeignKey(
                    name: "fk_permission_profile_levels_permission_profiles_permission_pr",
                    column: x => x.permission_profile_id,
                    principalTable: "permission_profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_permission_profiles",
            columns: table => new
            {
                profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_permission_profiles", x => new { x.user_id, x.profile_id });
                table.ForeignKey(
                    name: "fk_user_permission_profiles_permission_profiles_profile_id",
                    column: x => x.profile_id,
                    principalTable: "permission_profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_permission_profiles_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_permission_profiles_created_by",
            table: "permission_profiles",
            column: "created_by");

        migrationBuilder.CreateIndex(
            name: "ix_permission_profiles_name_normalized",
            table: "permission_profiles",
            column: "normalized_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_permission_profiles_updated_by",
            table: "permission_profiles",
            column: "updated_by");

        migrationBuilder.CreateIndex(
            name: "ix_user_permission_profiles_profile_id",
            table: "user_permission_profiles",
            column: "profile_id");

        migrationBuilder.CreateIndex(
            name: "ix_users_created_by",
            table: "users",
            column: "created_by");

        migrationBuilder.CreateIndex(
            name: "ix_users_deactivated_by",
            table: "users",
            column: "deactivated_by");

        migrationBuilder.CreateIndex(
            name: "ix_users_display_name_normalized",
            table: "users",
            column: "normalized_display_name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_login",
            table: "users",
            column: "login",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_updated_by",
            table: "users",
            column: "updated_by");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "permission_profile_levels");

        migrationBuilder.DropTable(
            name: "user_permission_profiles");

        migrationBuilder.DropTable(
            name: "permission_profiles");

        migrationBuilder.DropTable(
            name: "users");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task_flow.Data.Migrations
{
    /// <inheritdoc />
    public partial class GlobalWorkspaceNameUniquenessAndWorkspacePaginationPrep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspace_UserId_Name",
                table: "Workspaces");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_Name",
                table: "Workspaces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspaces",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspace_Name",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspaces");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_UserId_Name",
                table: "Workspaces",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }
    }
}

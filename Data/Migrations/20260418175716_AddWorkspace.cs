using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace task_flow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Task",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Workspace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workspace_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Task_WorkspaceId",
                table: "Task",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_UserId",
                table: "Workspace",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Workspace_WorkspaceId",
                table: "Task",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Workspace_WorkspaceId",
                table: "Task");

            migrationBuilder.DropTable(
                name: "Workspace");

            migrationBuilder.DropIndex(
                name: "IX_Task_WorkspaceId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Task");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task_flow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Workspace_WorkspaceId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Workspace_AspNetUsers_UserId",
                table: "Workspace");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workspace",
                table: "Workspace");

            migrationBuilder.RenameTable(
                name: "Workspace",
                newName: "Workspaces");

            migrationBuilder.RenameIndex(
                name: "IX_Workspace_UserId",
                table: "Workspaces",
                newName: "IX_Workspaces_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workspaces",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workspaces",
                table: "Workspaces",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Workspaces_WorkspaceId",
                table: "Task",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_AspNetUsers_UserId",
                table: "Workspaces",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Workspaces_WorkspaceId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_AspNetUsers_UserId",
                table: "Workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workspaces",
                table: "Workspaces");

            migrationBuilder.RenameTable(
                name: "Workspaces",
                newName: "Workspace");

            migrationBuilder.RenameIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspace",
                newName: "IX_Workspace_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Workspace",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workspace",
                table: "Workspace",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Workspace_WorkspaceId",
                table: "Task",
                column: "WorkspaceId",
                principalTable: "Workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workspace_AspNetUsers_UserId",
                table: "Workspace",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

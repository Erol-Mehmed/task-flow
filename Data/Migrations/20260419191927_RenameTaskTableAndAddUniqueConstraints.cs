using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task_flow.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameTaskTableAndAddUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_AspNetUsers_UserId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Workspaces_WorkspaceId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Task",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_WorkspaceId",
                table: "Task");

            migrationBuilder.RenameTable(
                name: "Task",
                newName: "Tasks");

            migrationBuilder.RenameIndex(
                name: "IX_Task_UserId",
                table: "Tasks",
                newName: "IX_Tasks_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Workspace_UserId_Name",
                table: "Workspaces",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItem_WorkspaceId_Title",
                table: "Tasks",
                columns: new[] { "WorkspaceId", "Title" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Workspace_UserId_Name",
                table: "Workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tasks",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_TaskItem_WorkspaceId_Title",
                table: "Tasks");

            migrationBuilder.RenameTable(
                name: "Tasks",
                newName: "Task");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_UserId",
                table: "Task",
                newName: "IX_Task_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Task",
                table: "Task",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_UserId",
                table: "Workspaces",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_WorkspaceId",
                table: "Task",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_AspNetUsers_UserId",
                table: "Task",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Workspaces_WorkspaceId",
                table: "Task",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id");
        }
    }
}

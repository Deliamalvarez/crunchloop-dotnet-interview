using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoItemWithListRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TodoListId",
                table: "TodoItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItem_TodoListId",
                table: "TodoItem",
                column: "TodoListId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItem_TodoList_TodoListId",
                table: "TodoItem",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItem_TodoList_TodoListId",
                table: "TodoItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoItem_TodoListId",
                table: "TodoItem");

            migrationBuilder.DropColumn(
                name: "TodoListId",
                table: "TodoItem");
        }
    }
}

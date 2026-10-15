using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardPlay.Repository.Migrations
{
    /// <inheritdoc />
    public partial class PrecoUnitarioNaMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoUnitario",
                table: "MovimentacoesCartao",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoUnitario",
                table: "MovimentacoesCartao");
        }
    }
}

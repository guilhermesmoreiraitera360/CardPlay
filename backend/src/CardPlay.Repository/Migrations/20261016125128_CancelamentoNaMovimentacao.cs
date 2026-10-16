using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardPlay.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CancelamentoNaMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompraId",
                table: "MovimentacoesCartao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCancelamento",
                table: "MovimentacoesCartao",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompraId",
                table: "MovimentacoesCartao");

            migrationBuilder.DropColumn(
                name: "DataCancelamento",
                table: "MovimentacoesCartao");
        }
    }
}

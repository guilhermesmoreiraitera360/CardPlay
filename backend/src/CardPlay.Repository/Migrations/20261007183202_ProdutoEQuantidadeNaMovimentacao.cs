using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardPlay.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoEQuantidadeNaMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeProduto",
                table: "MovimentacoesCartao",
                type: "TEXT",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProdutoId",
                table: "MovimentacoesCartao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "MovimentacoesCartao",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeProduto",
                table: "MovimentacoesCartao");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "MovimentacoesCartao");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "MovimentacoesCartao");
        }
    }
}

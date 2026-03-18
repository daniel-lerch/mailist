using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class PermittedSenders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendersQuery",
                table: "DistributionLists",
                type: "longtext",
                nullable: false,
                defaultValue: "null")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendersQuery",
                table: "DistributionLists");
        }
    }
}

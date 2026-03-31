using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mailist.Migrations
{
    /// <inheritdoc />
    public partial class SpamFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_DistributionLists_Alias",
                table: "DistributionLists");

            migrationBuilder.AddColumn<int>(
                name: "SpamCategory",
                table: "InboxEmails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpamJustification",
                table: "InboxEmails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionLists_Alias",
                table: "DistributionLists",
                column: "Alias",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DistributionLists_Alias",
                table: "DistributionLists");

            migrationBuilder.DropColumn(
                name: "SpamCategory",
                table: "InboxEmails");

            migrationBuilder.DropColumn(
                name: "SpamJustification",
                table: "InboxEmails");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DistributionLists_Alias",
                table: "DistributionLists",
                column: "Alias");
        }
    }
}

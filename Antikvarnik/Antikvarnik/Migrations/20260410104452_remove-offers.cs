using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Antikvarnik.Migrations
{
    /// <inheritdoc />
    public partial class removeoffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfferMessages_AspNetUsers_SenderId",
                table: "OfferMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferMessages_Offers_OfferId",
                table: "OfferMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_AspNetUsers_UserId",
                table: "Offers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offers",
                table: "Offers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferMessages",
                table: "OfferMessages");

            migrationBuilder.RenameTable(
                name: "Offers",
                newName: "Offer");

            migrationBuilder.RenameTable(
                name: "OfferMessages",
                newName: "OfferMessage");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_UserId",
                table: "Offer",
                newName: "IX_Offer_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferMessages_SenderId",
                table: "OfferMessage",
                newName: "IX_OfferMessage_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferMessages_OfferId",
                table: "OfferMessage",
                newName: "IX_OfferMessage_OfferId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Offer",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offer",
                table: "Offer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferMessage",
                table: "OfferMessage",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ItemMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SentOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemMessages_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMessages_ItemId",
                table: "ItemMessages",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMessages_SenderId",
                table: "ItemMessages",
                column: "SenderId");

            // IMPORTANT: use Restrict for at least one of the user-related FKs to avoid multiple cascade paths.
            migrationBuilder.AddForeignKey(
                name: "FK_Offer_AspNetUsers_UserId",
                table: "Offer",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferMessage_AspNetUsers_SenderId",
                table: "OfferMessage",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // keep Offer -> OfferMessage cascade (deleting an Offer removes its messages)
            migrationBuilder.AddForeignKey(
                name: "FK_OfferMessage_Offer_OfferId",
                table: "OfferMessage",
                column: "OfferId",
                principalTable: "Offer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offer_AspNetUsers_UserId",
                table: "Offer");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferMessage_AspNetUsers_SenderId",
                table: "OfferMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_OfferMessage_Offer_OfferId",
                table: "OfferMessage");

            migrationBuilder.DropTable(
                name: "ItemMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfferMessage",
                table: "OfferMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offer",
                table: "Offer");

            migrationBuilder.RenameTable(
                name: "OfferMessage",
                newName: "OfferMessages");

            migrationBuilder.RenameTable(
                name: "Offer",
                newName: "Offers");

            migrationBuilder.RenameIndex(
                name: "IX_OfferMessage_SenderId",
                table: "OfferMessages",
                newName: "IX_OfferMessages_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_OfferMessage_OfferId",
                table: "OfferMessages",
                newName: "IX_OfferMessages_OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_Offer_UserId",
                table: "Offers",
                newName: "IX_Offers_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Offers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfferMessages",
                table: "OfferMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offers",
                table: "Offers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OfferMessages_AspNetUsers_SenderId",
                table: "OfferMessages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfferMessages_Offers_OfferId",
                table: "OfferMessages",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_AspNetUsers_UserId",
                table: "Offers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

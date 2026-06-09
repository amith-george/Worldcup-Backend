using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorldcupApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPollIdToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PollId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_PollId",
                table: "Teams",
                column: "PollId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Polls_PollId",
                table: "Teams",
                column: "PollId",
                principalTable: "Polls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Polls_PollId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_PollId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "Teams");
        }
    }
}

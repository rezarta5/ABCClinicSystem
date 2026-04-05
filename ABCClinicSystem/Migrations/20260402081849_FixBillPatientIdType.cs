using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCClinicSystem.Migrations
{
    /// <inheritdoc />
    public partial class FixBillPatientIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_PatientId1",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_PatientId1",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Bills");

            migrationBuilder.AlterColumn<string>(
                name: "PatientId",
                table: "Bills",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_PatientId",
                table: "Bills",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_PatientId",
                table: "Bills",
                column: "PatientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_PatientId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_PatientId",
                table: "Bills");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "Bills",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "PatientId1",
                table: "Bills",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_PatientId1",
                table: "Bills",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_PatientId1",
                table: "Bills",
                column: "PatientId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

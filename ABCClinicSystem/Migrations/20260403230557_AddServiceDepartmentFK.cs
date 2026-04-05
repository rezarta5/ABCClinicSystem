using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCClinicSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDepartmentFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceDepartmentId",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceDepartmentId",
                table: "Services",
                column: "ServiceDepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ServiceDepartments_ServiceDepartmentId",
                table: "Services",
                column: "ServiceDepartmentId",
                principalTable: "ServiceDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ServiceDepartments_ServiceDepartmentId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_ServiceDepartmentId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceDepartmentId",
                table: "Services");
        }
    }
}

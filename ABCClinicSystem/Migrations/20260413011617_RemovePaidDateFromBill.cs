using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABCClinicSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaidDateFromBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "Bills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "Bills",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

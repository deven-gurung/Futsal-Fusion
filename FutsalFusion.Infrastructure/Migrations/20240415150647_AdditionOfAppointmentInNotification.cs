using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutsalFusion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdditionOfAppointmentInNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Notifications");
        }
    }
}

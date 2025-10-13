using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBooking.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingStatusDurationDateSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "Bookings",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "BookingTime",
                table: "Bookings",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Bookings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Date",
                table: "Bookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Doctor_Date_Time",
                table: "Bookings",
                columns: new[] { "DoctorId", "BookingDate", "BookingTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Date",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Doctor_Date_Time",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BookingTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bookings");
        }
    }
}

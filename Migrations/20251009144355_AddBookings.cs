using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicBooking.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.DropForeignKey(
                name: "FK_PatientDoctors_Doctors_DoctorId1",
                table: "PatientDoctors");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientDoctors_Patients_PatientId1",
                table: "PatientDoctors");

            migrationBuilder.DropIndex(
                name: "IX_PatientDoctors_DoctorId1",
                table: "PatientDoctors");

            migrationBuilder.DropIndex(
                name: "IX_PatientDoctors_PatientId1",
                table: "PatientDoctors");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "PatientDoctors");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "PatientDoctors");*/

            /*migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PatientDoctors",
                type: "rowversion",
                rowVersion: true,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);*/

            // 1. Elimina la colonna RowVersion esistente
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PatientDoctors");

            // 2. Ricrea la colonna con il tipo corretto
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PatientDoctors",
                type: "rowversion",
                rowVersion: true,
                nullable: true);


            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    SpecializationId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "SpecializationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctors_PatientId",
                table: "PatientDoctors",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DateTime",
                table: "Bookings",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Doctor_DateTime",
                table: "Bookings",
                columns: new[] { "DoctorId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PatientId",
                table: "Bookings",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SpecializationId",
                table: "Bookings",
                column: "SpecializationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_PatientDoctors_PatientId",
                table: "PatientDoctors");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PatientDoctors",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "PatientDoctors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PatientId1",
                table: "PatientDoctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctors_DoctorId1",
                table: "PatientDoctors",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDoctors_PatientId1",
                table: "PatientDoctors",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientDoctors_Doctors_DoctorId1",
                table: "PatientDoctors",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientDoctors_Patients_PatientId1",
                table: "PatientDoctors",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "PatientId");
        }
    }
}

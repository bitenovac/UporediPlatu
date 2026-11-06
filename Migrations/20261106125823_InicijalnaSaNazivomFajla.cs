using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UporediPlate.Migrations
{
    /// <inheritdoc />
    public partial class InicijalnaSaNazivomFajla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RfzoUvozi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jmbg = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Lbo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivSpreme = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivSpecijalnosti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivSubSpecijalnosti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivZvanja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatumZaposlenja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NazivZanimanja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivRV = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivRukovodioc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivZZ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivRM = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Koef1 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DodKzaRuk = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DodKzaSiNzv = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Dodatni036 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UkrsG = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UkrsM = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NapRadnik = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NapRz = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcRVr = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NazivOJ = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SifraZu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NazivZU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PovecanjeId = table.Column<int>(type: "int", nullable: true),
                    DatumKapitacija = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DatumPrestankaRO = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NaSpecijalizaciji = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PeriodUvoza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumUvozaPodataka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NazivFajla = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfzoUvozi", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RfzoUvozi");
        }
    }
}

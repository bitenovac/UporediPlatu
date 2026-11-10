using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UporediPlate.Migrations
{
    /// <inheritdoc />
    public partial class DodajTabeluZuUvozi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZuUvozi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filijala = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SifraZu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NazivZu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mesec = table.Column<int>(type: "int", nullable: false),
                    Jmbg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RadnoMesto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OsnovniKoef = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UkupniKoef = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SatiUkupno = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrekovremeniSati = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DezurstvoSati = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PoPozivuSati = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PrekovremeniNeto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DezurstvoNeto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PoPozivuNeto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PripravnostSati = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PripravnostNeto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UkupanNeto = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Bruto1 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Bruto2 = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PeriodUvoza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumUvozaPodataka = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NazivFajla = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZuUvozi", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZuUvozi");
        }
    }
}

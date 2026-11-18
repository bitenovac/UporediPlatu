using System.Data;
using System.Globalization;
using System.Text;
using ExcelDataReader;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace UporediPlate.Services;

public class UvozZuServis
{
    private readonly AppDbContext _context;
    private readonly ILogger<UvozZuServis> _logger;

    public UvozZuServis(AppDbContext context, ILogger<UvozZuServis> logger)
    {
        _context = context;
        _logger = logger;
    }

    public DataTable DobijIstorijuUvoza()
    {
        var dtIstorija = new DataTable();
        dtIstorija.Columns.Add("NazivFajla", typeof(string));
        dtIstorija.Columns.Add("PeriodUvoza", typeof(DateTime));
        dtIstorija.Columns.Add("DatumUvozaPodataka", typeof(DateTime));
        dtIstorija.Columns.Add("BrojRedova", typeof(int));

        try
        {
            var grupe = _context.ZuUvozi
                .GroupBy(x => new { x.PeriodUvoza, x.DatumUvozaPodataka })
                .Select(g => new
                {
                    SifraZu = g.Max(x => x.SifraZu) ?? "Непознато",
                    Nivo = g.Max(x => x.Nivo) ?? "Непознато",
                    g.Key.PeriodUvoza,
                    g.Key.DatumUvozaPodataka,
                    BrojRedova = g.Count()
                })
                .OrderByDescending(x => x.DatumUvozaPodataka)
                .ToList();

            foreach (var g in grupe)
            {
                var formatiraniNaziv = $"{g.SifraZu}_{g.Nivo}.xlsx";
                dtIstorija.Rows.Add(formatiraniNaziv, g.PeriodUvoza, g.DatumUvozaPodataka, g.BrojRedova);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом читања историје ЗУ из сервиса.");
        }

        return dtIstorija;
    }

    public async Task<(bool Uspeh, string Poruka)> ObradiZuExcel(IFormFile excelFajl, DateTime period)
    {
        if (excelFajl.Length == 0)
        {
            return (false, "Фајл није изабран или је празан.");
        }

        var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
        if (ekstenzija != ".xlsx" && ekstenzija != ".xls")
        {
            return (false, "Неисправан формат. Дозвољени су само .xlsx и .xls фајлови.");
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var trenutnoVremeUvoza = DateTime.Now;

        try
        {
            using var stream = new MemoryStream();
            await excelFajl.CopyToAsync(stream);
            stream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            if (result.Tables.Count == 0)
            {
                return (false, "Excel не садржи табеле.");
            }

            var izvornaTabela = result.Tables[0];

            var indeksRedaSaKolonama = -1;
            for (var i = 0; i < izvornaTabela.Rows.Count; i++)
            {
                var prvaCelija = izvornaTabela.Rows[i][0]?.ToString()?.ToLower() ?? "";
                if (prvaCelija.Contains("filijala"))
                {
                    indeksRedaSaKolonama = i;
                    break;
                }
            }

            if (indeksRedaSaKolonama == -1)
            {
                return (false, "Није пронађен важећи ред са колонама (Недостаје реч Филијала у првој колони).");
            }

            var dtZaBazu = new DataTable();
            dtZaBazu.Columns.Add("Filijala", typeof(string));
            dtZaBazu.Columns.Add("SifraZu", typeof(string));
            dtZaBazu.Columns.Add("NazivZu", typeof(string));
            dtZaBazu.Columns.Add("Nivo", typeof(string));
            dtZaBazu.Columns.Add("Mesec", typeof(int));
            dtZaBazu.Columns.Add("Jmbg", typeof(string));
            dtZaBazu.Columns.Add("Ime", typeof(string));
            dtZaBazu.Columns.Add("Prezime", typeof(string));
            dtZaBazu.Columns.Add("RadnoMesto", typeof(string));
            dtZaBazu.Columns.Add("OsnovniKoef", typeof(decimal));
            dtZaBazu.Columns.Add("UkupniKoef", typeof(decimal));
            dtZaBazu.Columns.Add("SatiUkupno", typeof(decimal));
            dtZaBazu.Columns.Add("PrekovremeniSati", typeof(decimal));
            dtZaBazu.Columns.Add("DezurstvoSati", typeof(decimal));
            dtZaBazu.Columns.Add("PoPozivuSati", typeof(decimal));
            dtZaBazu.Columns.Add("PrekovremeniNeto", typeof(decimal));
            dtZaBazu.Columns.Add("DezurstvoNeto", typeof(decimal));
            dtZaBazu.Columns.Add("PoPozivuNeto", typeof(decimal));
            dtZaBazu.Columns.Add("PripravnostSati", typeof(decimal));
            dtZaBazu.Columns.Add("PripravnostNeto", typeof(decimal));
            dtZaBazu.Columns.Add("UkupanNeto", typeof(decimal));
            dtZaBazu.Columns.Add("Bruto1", typeof(decimal));
            dtZaBazu.Columns.Add("Bruto2", typeof(decimal));
            dtZaBazu.Columns.Add("PeriodUvoza", typeof(DateTime));
            dtZaBazu.Columns.Add("DatumUvozaPodataka", typeof(DateTime));
            dtZaBazu.Columns.Add("NazivFajla", typeof(string));

            var pocetniRedZaPodatke = indeksRedaSaKolonama + 2;

            for (var i = pocetniRedZaPodatke; i < izvornaTabela.Rows.Count; i++)
            {
                var red = izvornaTabela.Rows[i];

                var siroviJmbg = "";
                var jmbgObj = red[5];
                if (jmbgObj != DBNull.Value)
                {
                    if (jmbgObj is double dJmbg)
                    {
                        siroviJmbg = Math.Truncate(dJmbg).ToString(CultureInfo.InvariantCulture);
                    }
                    else if (decimal.TryParse(jmbgObj.ToString()?.Replace(",", "."), NumberStyles.Any,
                                 CultureInfo.InvariantCulture, out var jmbgDec))
                    {
                        siroviJmbg = Math.Truncate(jmbgDec).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        siroviJmbg = jmbgObj.ToString()?.Trim() ?? "";
                    }
                }

                if (siroviJmbg.Length == 12)
                {
                    siroviJmbg = "0" + siroviJmbg;
                }

                if (string.IsNullOrEmpty(siroviJmbg) || siroviJmbg.Length < 13 ||
                    siroviJmbg.Contains("jmbg", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                dtZaBazu.Rows.Add(
                    red[0]?.ToString()?.Trim() ?? "",
                    red[1]?.ToString()?.Trim() ?? "",
                    red[2]?.ToString()?.Trim() ?? "",
                    red[3]?.ToString()?.Trim() ?? "",
                    DekodirajInt(red[4]),
                    siroviJmbg,
                    red[6]?.ToString()?.Trim() ?? "",
                    red[7]?.ToString()?.Trim() ?? "",
                    red[8]?.ToString()?.Trim() ?? "",
                    DekodirajDecimal(red[9]),
                    DekodirajDecimal(red[10]),
                    DekodirajDecimal(red[11]),
                    DekodirajDecimal(red[12]),
                    DekodirajDecimal(red[13]),
                    DekodirajDecimal(red[14]),
                    DekodirajDecimal(red[15]),
                    DekodirajDecimal(red[16]),
                    DekodirajDecimal(red[17]),
                    DekodirajDecimal(red[18]),
                    DekodirajDecimal(red[19]),
                    DekodirajDecimal(red[20]),
                    DekodirajDecimal(red[21]),
                    DekodirajDecimal(red[22]),
                    period,
                    trenutnoVremeUvoza,
                    excelFajl.FileName
                );
            }

            if (dtZaBazu.Rows.Count <= 0)
            {
                return (false, "Нема валидних редова за увоз.");
            }

            var stringKonekcije = _context.Database.GetDbConnection().ConnectionString;
            await using (var konekcija = new SqlConnection(stringKonekcije))
            {
                await konekcija.OpenAsync();
                using (var bulkCopy = new SqlBulkCopy(konekcija))
                {
                    bulkCopy.DestinationTableName = "ZuUvozi";
                    foreach (DataColumn col in dtZaBazu.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }

                    await bulkCopy.WriteToServerAsync(dtZaBazu);
                }
            }

            return (true, $"Подаци из ЗУ су успешно увезени. Укупно записа: {dtZaBazu.Rows.Count}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка при ЗУ увозу унутар сервиса.");
            return (false, $"Грешка: {ex.Message}");
        }
    }

    private static decimal DekodirajDecimal(object? obj) => ExcelVrednosti.DekodirajDecimal(obj) ?? 0;

    private static int DekodirajInt(object? obj) => Convert.ToInt32(Math.Truncate(DekodirajDecimal(obj)));

    public async Task<(bool Uspeh, string Poruka)> ObrisiUvoz(string vremeKljuc)
    {
        try
        {
            if (!VremenskiKljuc.PokusajParsiranje(vremeKljuc, out var pocetakSekunde))
            {
                return (false, "Неисправан временски кључ.");
            }

            var zaBrisanje = _context.ZuUvozi
                .IzUvoza(pocetakSekunde)
                .ToList();
            if (zaBrisanje.Count == 0)
            {
                return (false, "Подаци нису пронађени.");
            }

            _context.ZuUvozi.RemoveRange(zaBrisanje);
            await _context.SaveChangesAsync();
            return (true, "Увоз из ЗУ је успешно обрисан.");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом брисања ЗУ увоза.");
            return (false, $"Грешка при брисању: {ex.Message}");
        }
    }
}
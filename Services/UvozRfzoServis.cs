using System.Data;
using System.Globalization;
using System.Text;
using ExcelDataReader;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace UporediPlate.Services;

public class UvozRfzoServis
{
    private readonly AppDbContext _context;
    private readonly ILogger<UvozRfzoServis> _logger;

    public UvozRfzoServis(AppDbContext context, ILogger<UvozRfzoServis> logger)
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
            var grupe = _context.RfzoUvozi
                .GroupBy(x => new { x.NazivFajla, x.PeriodUvoza, x.DatumUvozaPodataka })
                .Select(g => new
                {
                    NazivFajla = g.Key.NazivFajla ?? "Непознат назив",
                    g.Key.PeriodUvoza,
                    g.Key.DatumUvozaPodataka,
                    BrojRedova = g.Count()
                })
                .OrderByDescending(x => x.DatumUvozaPodataka)
                .ToList();

            foreach (var g in grupe)
            {
                dtIstorija.Rows.Add(g.NazivFajla, g.PeriodUvoza, g.DatumUvozaPodataka, g.BrojRedova);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом читања историје из сервиса.");
        }

        return dtIstorija;
    }

    public async Task<(bool Uspeh, string Poruka)> ObradiRfzoExcel(IFormFile excelFajl, DateTime period)
    {
        if (excelFajl.Length == 0)
        {
            return (false, "Фајл није изабран или је празан.");
        }

        var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
        if (ekstenzija != ".xlsx" && ekstenzija != ".xls")
        {
            return (false, "Неисправан формат фајла. Дозвољени су само .xlsx и .xls фајлови.");
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var trenutnoVremeUvoza = DateTime.Now;

        try
        {
            using var stream = new MemoryStream();
            await excelFajl.CopyToAsync(stream);
            stream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            if (result.Tables.Count == 0)
            {
                return (false, "Изабрани Excel фајл не садржи табеле.");
            }

            var izvornaTabela = result.Tables[0];

            var dtZaBazu = new DataTable();
            dtZaBazu.Columns.Add("Jmbg", typeof(string));
            dtZaBazu.Columns.Add("Lbo", typeof(string));
            dtZaBazu.Columns.Add("Prezime", typeof(string));
            dtZaBazu.Columns.Add("Ime", typeof(string));
            dtZaBazu.Columns.Add("NazivSpreme", typeof(string));
            dtZaBazu.Columns.Add("NazivSpecijalnosti", typeof(string));
            dtZaBazu.Columns.Add("NazivSubSpecijalnosti", typeof(string));
            dtZaBazu.Columns.Add("NazivZvanja", typeof(string));
            dtZaBazu.Columns.Add("DatumZaposlenja", typeof(DateTime)).AllowDBNull = true;
            dtZaBazu.Columns.Add("NazivZanimanja", typeof(string));
            dtZaBazu.Columns.Add("NazivRV", typeof(string));
            dtZaBazu.Columns.Add("NazivRukovodioc", typeof(string));
            dtZaBazu.Columns.Add("NazivZZ", typeof(string));
            dtZaBazu.Columns.Add("NazivRM", typeof(string));
            dtZaBazu.Columns.Add("Koef1", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("DodKzaRuk", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("DodKzaSiNzv", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("Dodatni036", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("UkrsG", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("UkrsM", typeof(decimal)).AllowDBNull = true;
            dtZaBazu.Columns.Add("NapRadnik", typeof(string));
            dtZaBazu.Columns.Add("NapRz", typeof(string));
            dtZaBazu.Columns.Add("ProcRVr", typeof(int)).AllowDBNull = true;
            dtZaBazu.Columns.Add("NazivOJ", typeof(string));
            dtZaBazu.Columns.Add("SifraZu", typeof(string));
            dtZaBazu.Columns.Add("NazivZU", typeof(string));
            dtZaBazu.Columns.Add("PovecanjeId", typeof(int)).AllowDBNull = true;
            dtZaBazu.Columns.Add("DatumKapitacija", typeof(DateTime)).AllowDBNull = true;
            dtZaBazu.Columns.Add("DatumPrestankaRO", typeof(DateTime)).AllowDBNull = true;
            dtZaBazu.Columns.Add("NaSpecijalizaciji", typeof(int)).AllowDBNull = true;
            dtZaBazu.Columns.Add("PeriodUvoza", typeof(DateTime));
            dtZaBazu.Columns.Add("DatumUvozaPodataka", typeof(DateTime));
            dtZaBazu.Columns.Add("NazivFajla", typeof(string));

            foreach (DataRow red in izvornaTabela.Rows)
            {
                if (izvornaTabela.Columns.Count < 4)
                {
                    continue;
                }

                var siroviJmbg = "";
                var jmbgObj = red[0];
                if (jmbgObj != DBNull.Value)
                {
                    var jmbgDec = ExcelVrednosti.DekodirajDecimal(jmbgObj);
                    if (jmbgDec.HasValue)
                    {
                        siroviJmbg = Math.Truncate(jmbgDec.Value).ToString(CultureInfo.InvariantCulture);
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

                if (string.IsNullOrEmpty(siroviJmbg) || siroviJmbg.ToLower().Contains("jmbg") ||
                    siroviJmbg.Length < 13)
                {
                    continue;
                }

                var siroviLbo = "";
                var lboObj = red[1];
                if ( lboObj != DBNull.Value)
                {
                    var lboDec = ExcelVrednosti.DekodirajDecimal(lboObj);
                    if (lboDec.HasValue)
                    {
                        siroviLbo = Math.Truncate(lboDec.Value).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        siroviLbo = lboObj.ToString()?.Trim() ?? "";
                    }
                }

                dtZaBazu.Rows.Add(
                    siroviJmbg,
                    siroviLbo,
                    ProcitajKolonu(2, "string"),
                    ProcitajKolonu(3, "string"),
                    ProcitajKolonu(4, "string"),
                    ProcitajKolonu(5, "string"),
                    ProcitajKolonu(6, "string"),
                    ProcitajKolonu(7, "string"),
                    ProcitajKolonu(8, "datum"),
                    ProcitajKolonu(9, "string"),
                    ProcitajKolonu(10, "string"),
                    ProcitajKolonu(11, "string"),
                    ProcitajKolonu(12, "string"),
                    ProcitajKolonu(13, "string"),
                    ProcitajKolonu(14, "decimal"),
                    ProcitajKolonu(15, "decimal"),
                    ProcitajKolonu(16, "decimal"),
                    ProcitajKolonu(17, "decimal"),
                    ProcitajKolonu(18, "decimal"),
                    ProcitajKolonu(19, "decimal"),
                    ProcitajKolonu(20, "string"),
                    ProcitajKolonu(21, "string"),
                    ProcitajKolonu(22, "int"),
                    ProcitajKolonu(23, "string"),
                    ProcitajKolonu(24, "string"),
                    ProcitajKolonu(25, "string"),
                    ProcitajKolonu(26, "int"),
                    ProcitajKolonu(27, "datum"),
                    ProcitajKolonu(28, "datum"),
                    ProcitajKolonu(29, "int"),
                    period,
                    trenutnoVremeUvoza,
                    excelFajl.FileName
                );
                continue;

                object ProcitajKolonu(int indeks, string tip)
                {
                    if (izvornaTabela.Columns.Count <= indeks)
                    {
                        return DBNull.Value;
                    }

                    var val = red[indeks];
                    if (val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString()))
                    {
                        return DBNull.Value;
                    }

                    if (tip == "decimal")
                    {
                        var decRez = ExcelVrednosti.DekodirajDecimal(val);
                        return decRez.HasValue ? decRez.Value : DBNull.Value;
                    }

                    if (tip == "int")
                    {
                        var decVal = ExcelVrednosti.DekodirajDecimal(val);
                        return decVal.HasValue ? (object)Convert.ToInt32(Math.Truncate(decVal.Value)) : 0;
                    }

                    if (tip == "datum")
                    {
                        var datRez = ExcelVrednosti.KonvertujDatum(val);
                        return datRez.HasValue ? datRez.Value : DBNull.Value;
                    }

                    return val.ToString()?.Trim() ?? "";
                }
            }

            if (dtZaBazu.Rows.Count <= 0)
            {
                return (false, "Није пронађен ниједан валидан ред за увоз у фајлу.");
            }

            var stringKonekcije = _context.Database.GetDbConnection().ConnectionString;
            await using (var konekcija = new SqlConnection(stringKonekcije))
            {
                await konekcija.OpenAsync();
                using (var bulkCopy = new SqlBulkCopy(konekcija))
                {
                    bulkCopy.DestinationTableName = "RfzoUvozi";
                    foreach (DataColumn col in dtZaBazu.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }

                    await bulkCopy.WriteToServerAsync(dtZaBazu);
                }
            }

            return (true, $"Подаци су успешно увезени. Укупно записа: {dtZaBazu.Rows.Count}");

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом увоза унутар сервиса.");
            return (false, $"Грешка приликом увоза података: {ex.Message}");
        }
    }

    public async Task<(bool Uspeh, string Poruka)> ObrisiUvoz(string vremeKljuc)
    {
        try
        {
            if (!VremenskiKljuc.PokusajParsiranje(vremeKljuc, out var pocetakSekunde))
            {
                return (false, "Неисправан временски кључ.");
            }

            var zaBrisanje = _context.RfzoUvozi
                .IzUvoza(pocetakSekunde)
                .ToList();
            if (zaBrisanje.Any())
            {
                _context.RfzoUvozi.RemoveRange(zaBrisanje);
                await _context.SaveChangesAsync();
                return (true, "Увоз је успешно обрисан.");
            }

            return (false, "Подаци за наведени период нису пронађени.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом брисања унутар сервиса.");
            return (false, $"Грешка приликом брисања: {ex.Message}");
        }
    }
}
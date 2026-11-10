using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using UporediPlate.Models;
using UporediPlate;

namespace UporediPlate.Services
{
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
            DataTable dtIstorija = new DataTable();
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
                        PeriodUvoza = g.Key.PeriodUvoza,
                        DatumUvozaPodataka = g.Key.DatumUvozaPodataka,
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
            if (excelFajl == null || excelFajl.Length == 0)
                return (false, "Фајл није изабран или је празан.");

            var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
            if (ekstenzija != ".xlsx" && ekstenzija != ".xls")
                return (false, "Неисправан формат фајла. Дозвољени су само .xlsx и .xls фајлови.");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DateTime trenutnoVremeUvoza = DateTime.Now;

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFajl.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        if (result.Tables.Count == 0)
                            return (false, "Изабрани Excel фајл не садржи табеле.");

                        DataTable izvornaTabela = result.Tables[0];

                        DataTable dtZaBazu = new DataTable();
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
                            if (izvornaTabela.Columns.Count < 4) continue;

                            string siroviJmbg = "";
                            var jmbgObj = red[0]; // Исправљено са 'red' на 'red[0]'
                            if (jmbgObj != null && jmbgObj != DBNull.Value)
                            {
                                if (decimal.TryParse(jmbgObj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal jmbgDec))
                                    siroviJmbg = Math.Truncate(jmbgDec).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                else
                                    siroviJmbg = jmbgObj.ToString()?.Trim() ?? "";
                            }

                            if (siroviJmbg.Length == 12) siroviJmbg = "0" + siroviJmbg;
                            if (string.IsNullOrEmpty(siroviJmbg) || siroviJmbg.ToLower().Contains("jmbg") || siroviJmbg.Length < 13) continue;

                            string siroviLbo = "";
                            var lboObj = red[1]; // Исправљено са 'red' на 'red[1]'
                            if (lboObj != null && lboObj != DBNull.Value)
                            {
                                if (decimal.TryParse(lboObj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal lboDec))
                                    siroviLbo = Math.Truncate(lboDec).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                else
                                    siroviLbo = lboObj.ToString()?.Trim() ?? "";
                            }

                            // Локална функција за читање исправљена да правилно референцира тренутни 'red' и 'izvornaTabela'
                            object ProcitajKolonu(int indeks, string tip)
                            {
                                if (izvornaTabela.Columns.Count <= indeks) return DBNull.Value;
                                var val = red[indeks];
                                if (val == null || val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString())) return DBNull.Value;

                                if (tip == "decimal")
                                {
                                    if (decimal.TryParse(val.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal decRez))
                                        return decRez;
                                    return DBNull.Value;
                                }
                                if (tip == "int")
                                {
                                    if (decimal.TryParse(val.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal decVal))
                                        return Convert.ToInt32(Math.Truncate(decVal));
                                    return 0;
                                }
                                if (tip == "datum")
                                {
                                    var datRez = KonvertujExcelDatum(val);
                                    return datRez.HasValue ? (object)datRez.Value : DBNull.Value;
                                }

                                return val.ToString()?.Trim() ?? "";
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
                        }
                        if (dtZaBazu.Rows.Count > 0)
                        {
                            string stringKonekcije = _context.Database.GetDbConnection().ConnectionString;
                            using (SqlConnection konekcija = new SqlConnection(stringKonekcije))
                            {
                                await konekcija.OpenAsync();
                                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(konekcija))
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
                        return (false, "Није пронађен ниједан валидан ред за увоз у фајлу.");
                    }
                }
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
                if (string.IsNullOrEmpty(vremeKljuc) || vremeKljuc.Length < 14)
                    return (false, "Неисправан временски кључ.");
                int godina = int.Parse(vremeKljuc.Substring(0, 4));
                int mesec = int.Parse(vremeKljuc.Substring(4, 2));
                int dan = int.Parse(vremeKljuc.Substring(6, 2));
                int sat = int.Parse(vremeKljuc.Substring(8, 2));
                int minut = int.Parse(vremeKljuc.Substring(10, 2));
                int sekund = int.Parse(vremeKljuc.Substring(12, 2));
                DateTime pocetakSekunde = new DateTime(godina, mesec, dan, sat, minut, sekund);
                DateTime krajSekunde = pocetakSekunde.AddSeconds(1);
                var zaBrisanje = _context.RfzoUvozi
                .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
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
        private DateTime? KonvertujExcelDatum(object obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.ToString())) return null;
            string strVal = obj.ToString().Trim();
            if (strVal.Contains(".") || strVal.Contains("-"))
            {
                if (DateTime.TryParse(strVal, out DateTime regularanDatum)) return regularanDatum;
                if (DateTime.TryParseExact(strVal, new[] { "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yyyy.", "yyyy-MM-dd" },
                System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime specDatum))
                {
                    return specDatum;
                }
            }
            if (double.TryParse(strVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double brojDana))
            {
                try
                {
                    if (brojDana > 1 && brojDana < 2958465) return DateTime.FromOADate(brojDana);
                }
                catch { return null; }
            }
            return null;
        }
    }
}
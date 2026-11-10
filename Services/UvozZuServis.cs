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
            DataTable dtIstorija = new DataTable();
            dtIstorija.Columns.Add("NazivFajla", typeof(string));
            dtIstorija.Columns.Add("PeriodUvoza", typeof(DateTime));
            dtIstorija.Columns.Add("DatumUvozaPodataka", typeof(DateTime));
            dtIstorija.Columns.Add("BrojRedova", typeof(int));

            try
            {
                // Динамички генеришемо назив у формату Шифра ЗУ_Ниво.xlsx из података у табели
                var grupe = _context.ZuUvozi
                    .GroupBy(x => new { x.PeriodUvoza, x.DatumUvozaPodataka })
                    .Select(g => new
                    {
                        // Читамо вредности ШифраЗУ и Ниво из групе и спајамо их у тражени назив
                        SifraZu = g.Max(x => x.SifraZu) ?? "Непознато",
                        Nivo = g.Max(x => x.Nivo) ?? "Непознато",
                        PeriodUvoza = g.Key.PeriodUvoza,
                        DatumUvozaPodataka = g.Key.DatumUvozaPodataka,
                        BrojRedova = g.Count()
                    })
                    .OrderByDescending(x => x.DatumUvozaPodataka)
                    .ToList();

                foreach (var g in grupe)
                {
                    // Формирамо коначни приказани назив фајла
                    string formatiraniNaziv = $"{g.SifraZu}_{g.Nivo}.xlsx";
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
            if (excelFajl == null || excelFajl.Length == 0)
                return (false, "Фајл није изабран или је празан.");

            var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
            if (ekstenzija != ".xlsx" && ekstenzija != ".xls")
                return (false, "Неисправан формат. Дозвољени су само .xlsx и .xls фајлови.");

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
                        var result = reader.AsDataSet();
                        if (result.Tables.Count == 0)
                            return (false, "Excel не садржи табеле.");

                        DataTable izvornaTabela = result.Tables[0];

                        int indeksRedaSaKolonama = -1;
                        for (int i = 0; i < izvornaTabela.Rows.Count; i++)
                        {
                            string prvaCelija = izvornaTabela.Rows[i][0]?.ToString()?.ToLower() ?? "";
                            if (prvaCelija.Contains("filijala"))
                            {
                                indeksRedaSaKolonama = i;
                                break;
                            }
                        }

                        if (indeksRedaSaKolonama == -1)
                            return (false, "Није пронађен важећи ред са колонама (Недостаје реч Филијала у првој колони).");

                        DataTable dtZaBazu = new DataTable();
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

                        int pocetniRedZaPodatke = indeksRedaSaKolonama + 2;

                        for (int i = pocetniRedZaPodatke; i < izvornaTabela.Rows.Count; i++)
                        {
                            DataRow red = izvornaTabela.Rows[i];

                            string siroviJmbg = "";
                            var jmbgObj = red[5];
                            if (jmbgObj != null && jmbgObj != DBNull.Value)
                            {
                                if (decimal.TryParse(jmbgObj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal jmbgDec))
                                    siroviJmbg = Math.Truncate(jmbgDec).ToString(System.Globalization.CultureInfo.InvariantCulture);
                                else
                                    siroviJmbg = jmbgObj.ToString()?.Trim() ?? "";
                            }

                            if (siroviJmbg.Length == 12) siroviJmbg = "0" + siroviJmbg;
                            if (string.IsNullOrEmpty(siroviJmbg) || siroviJmbg.Length < 13 || siroviJmbg.ToLower().Contains("jmbg")) continue;

                            decimal DekodirajDecimal(object obj)
                            {
                                if (obj == null || obj == DBNull.Value || string.IsNullOrWhiteSpace(obj.ToString())) return 0;
                                return decimal.TryParse(obj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rez) ? rez : 0;
                            }

                            int DekodirajInt(object obj)
                            {
                                if (obj == null || obj == DBNull.Value || string.IsNullOrWhiteSpace(obj.ToString())) return 0;
                                return decimal.TryParse(obj.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal decVal) ? Convert.ToInt32(Math.Truncate(decVal)) : 0;
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

                        if (dtZaBazu.Rows.Count > 0)
                        {
                            string stringKonekcije = _context.Database.GetDbConnection().ConnectionString;
                            using (SqlConnection konekcija = new SqlConnection(stringKonekcije))
                            {
                                await konekcija.OpenAsync();
                                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(konekcija))
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
                        return (false, "Нема валидних редова за увоз.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка при ЗУ увозу унутар сервиса.");
                return (false, $"Грешка: {ex.Message}");
            }
        }

        // ДОДАТО: Логика за сигурно брисање ЗУ увоза
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

                var zaBrisanje = _context.ZuUvozi
                    .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
                    .ToList();

                if (zaBrisanje.Any())
                {
                    _context.ZuUvozi.RemoveRange(zaBrisanje);
                    await _context.SaveChangesAsync();
                    return (true, "Увоз из ЗУ је успешно обрисан.");
                }
                return (false, "Подаци нису пронађени.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом брисања ЗУ увоза.");
                return (false, $"Грешка при брисању: {ex.Message}");
            }
        }
    }
}
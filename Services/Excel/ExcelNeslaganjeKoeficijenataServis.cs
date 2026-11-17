using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services.Excel
{
    public class ExcelNeslaganjeKoeficijenataServis
    {
        private readonly ExcelPomocnik _pomocnik;

        public ExcelNeslaganjeKoeficijenataServis(ExcelPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public byte[] Generisi(List<IzvestajNeslaganjeKoeficijenataViewModel> podaci)
        {
            var sortirano = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .ToList();

            using var workbook = new XLWorkbook();
            var grupePoZu = sortirano.GroupBy(x => x.SifraZuNormalizovana);

            foreach (var grupa in grupePoZu)
            {
                var prvi = grupa.First();
                string nazivLista = _pomocnik.OcistiNazivLista(
                    string.IsNullOrWhiteSpace(prvi.SifraZuOriginal) ? grupa.Key : prvi.SifraZuOriginal);

                var ws = workbook.Worksheets.Add(nazivLista);
                int brojKolona = 9;

                _pomocnik.PrimeniZaglavljeLista(
                    ws,
                    "Неслагање у основним и укупним коефицијентима (РФЗО и ЗУ)",
                    $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                    brojKolona: brojKolona,
                    landscape: true);

                var zuRange = ws.Range(2, 1, 2, brojKolona);
                zuRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");
                zuRange.Style.Font.FontColor = XLColor.White;
                zuRange.Style.Font.Bold = true;

                int statusRow = 3;
                var statusRange = ws.Range(statusRow, 1, statusRow, brojKolona);
                statusRange.Merge();
                statusRange.Value = "Утврђена је разлика у основном или укупном коефицијенту између РФЗО и ЗУ евиденције";
                statusRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
                statusRange.Style.Font.FontColor = XLColor.FromHtml("#212529");
                statusRange.Style.Font.Bold = true;
                statusRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                statusRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(statusRow).Height = 22;

                int headerRow = 4;
                string[] kolone =
                {
                    "Период",
                    "ЈМБГ",
                    "Име",
                    "Презиме",
                    "Назив радног места (РФЗО)",
                    "Осн. коэф. (РФЗО)",
                    "Осн. коэф. (ЗУ)",
                    "Укуп. коэф. (РФЗО)",
                    "Укуп. коэф. (ЗУ)"
                };
                _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

                int trenutniRed = headerRow + 1;
                foreach (var item in grupa)
                {
                    ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                    ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var jmbgCell = ws.Cell(trenutniRed, 2);
                    jmbgCell.SetValue(item.Jmbg);
                    jmbgCell.Style.NumberFormat.Format = "@";
                    jmbgCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 3).Value = item.Ime;
                    ws.Cell(trenutniRed, 4).Value = item.Prezime;

                    var rmCell = ws.Cell(trenutniRed, 5);
                    rmCell.Value = item.NazivRM;
                    rmCell.Style.Alignment.WrapText = true;

                    ws.Cell(trenutniRed, 6).Value = item.OsnovniKoefRfzo;
                    ws.Cell(trenutniRed, 6).Style.NumberFormat.Format = "#,##0.0000";
                    ws.Cell(trenutniRed, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 7).Value = item.OsnovniKoefZu;
                    ws.Cell(trenutniRed, 7).Style.NumberFormat.Format = "#,##0.0000";
                    ws.Cell(trenutniRed, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 8).Value = item.UkupniKoefRfzo;
                    ws.Cell(trenutniRed, 8).Style.NumberFormat.Format = "#,##0.0000";
                    ws.Cell(trenutniRed, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 9).Value = item.UkupniKoefZu;
                    ws.Cell(trenutniRed, 9).Style.NumberFormat.Format = "#,##0.0000";
                    ws.Cell(trenutniRed, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona: brojKolona);
                    ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    trenutniRed++;
                }

                _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona: brojKolona, 5);
                ws.Column(5).Width = 38;
            }

            return _pomocnik.KonvertujUBajtove(workbook);
        }
    }
}

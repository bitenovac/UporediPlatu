using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services.Excel
{
    public class ExcelPrekovremeniSatiServis
    {
        private readonly ExcelPomocnik _pomocnik;

        public ExcelPrekovremeniSatiServis(ExcelPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public byte[] Generisi(List<IzvestajPrekovremeniSatiViewModel> podaci)
        {
            var sortirano = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenByDescending(x => x.UkupnoDodatnihSati)
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
                int brojKolona = 10;

                _pomocnik.PrimeniZaglavljeLista(
                    ws,
                    "Преглед прековременог рада, дежурства, рада по позиву и приправности (ЗУ)",
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
                statusRange.Value = "Приказ запослених из ЗУ евиденције код којих је збир додатних сати већи од нуле (сортирано опадајуће)";
                statusRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
                statusRange.Style.Font.FontColor = XLColor.FromHtml("#212529");
                statusRange.Style.Font.Bold = true;
                statusRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                statusRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(statusRow).Height = 22;

                int headerRow = 4;
                string[] kolone =
                {
                    "ЈМБГ",
                    "Име",
                    "Презиме",
                    "Радно место",
                    "Прековремени сати",
                    "Дежурство сати",
                    "По позиву сати",
                    "Приправност сати",
                    "Укупно сати",
                    "Радно време"
                };
                _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

                int trenutniRed = headerRow + 1;
                foreach (var item in grupa)
                {
                    var jmbgCell = ws.Cell(trenutniRed, 1);
                    jmbgCell.SetValue(item.Jmbg);
                    jmbgCell.Style.NumberFormat.Format = "@";
                    jmbgCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 2).Value = item.Ime;
                    ws.Cell(trenutniRed, 3).Value = item.Prezime;

                    var rmCell = ws.Cell(trenutniRed, 4);
                    rmCell.Value = item.RadnoMesto;
                    rmCell.Style.Alignment.WrapText = true;

                    ws.Cell(trenutniRed, 5).Value = item.PrekovremeniSati;
                    ws.Cell(trenutniRed, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 6).Value = item.DezurstvoSati;
                    ws.Cell(trenutniRed, 6).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 7).Value = item.PoPozivuSati;
                    ws.Cell(trenutniRed, 7).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 8).Value = item.PripravnostSati;
                    ws.Cell(trenutniRed, 8).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 9).Value = item.UkupnoDodatnihSati;
                    ws.Cell(trenutniRed, 9).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    ws.Cell(trenutniRed, 9).Style.Font.Bold = true;

                    ws.Cell(trenutniRed, 10).Value = item.RadnoVreme;
                    ws.Cell(trenutniRed, 10).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(trenutniRed, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona: brojKolona);
                    ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    trenutniRed++;
                }

                _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona: brojKolona, 4);
                ws.Column(4).Width = 36;
            }

            return _pomocnik.KonvertujUBajtove(workbook);
        }
    }
}

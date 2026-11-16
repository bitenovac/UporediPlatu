using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services.Excel
{
    public class ExcelNepronadjeniUZuServis
    {
        private readonly ExcelPomocnik _pomocnik;

        public ExcelNepronadjeniUZuServis(ExcelPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public byte[] Generisi(List<IzvestajNepronadjeniUZuViewModel> podaci)
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
                string nazivLista = _pomocnik.OcistiNazivLista(grupa.Key);

                var ws = workbook.Worksheets.Add(nazivLista);
                int brojKolona = 4;

                // 1. Главни наслов извештаја
                _pomocnik.PrimeniZaglavljeLista(
                    ws,
                    "Радници на списку РФЗО-а који нису на списку ЗУ",
                    $"Шифра ЗУ: {prvi.SifraZuNormalizovana} | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                    brojKolona: brojKolona,
                    landscape: false);

                // Стилизовање 2. реда (подаци о ЗУ) у зелену боју као на екрану
                var zuRange = ws.Range(2, 1, 2, brojKolona);
                zuRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");
                zuRange.Style.Font.FontColor = XLColor.White;
                zuRange.Style.Font.Bold = true;

                // 3. Жути ред са разлогом неслагања
                int statusRow = 3;
                var statusRange = ws.Range(statusRow, 1, statusRow, brojKolona);
                statusRange.Merge();
                statusRange.Value = "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу";
                statusRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
                statusRange.Style.Font.FontColor = XLColor.FromHtml("#212529");
                statusRange.Style.Font.Bold = true;
                statusRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                statusRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(statusRow).Height = 22;

                // 4. Заглавље табеле (4 колоне)
                int headerRow = 4;
                string[] kolone = { "Период", "ЈМБГ", "Презиме и име", "Радно место / Занимање" };
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

                    ws.Cell(trenutniRed, 3).Value = $"{item.Prezime} {item.Ime}";
                    ws.Cell(trenutniRed, 4).Value = string.IsNullOrWhiteSpace(item.NazivRM) ? item.NazivZanimanja : item.NazivRM;

                    _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona: brojKolona);
                    trenutniRed++;
                }

                _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona: brojKolona, 4);
            }

            return _pomocnik.KonvertujUBajtove(workbook);
        }
    }
}

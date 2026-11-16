using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services.Excel
{
    public class ExcelNepronadjeniURfzoServis
    {
        private readonly ExcelPomocnik _pomocnik;

        public ExcelNepronadjeniURfzoServis(ExcelPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public byte[] Generisi(List<IzvestajNepronadjeniViewModel> podaci)
        {
            var sortirano = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.Nivo)
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

                _pomocnik.PrimeniZaglavljeLista(
                    ws,
                    "ИЗВЕШТАЈ О КОНТРОЛИ ПОДАТАКА О ДОСТАВЉЕНИМ ЗАРАДАМА",
                    $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) — {prvi.NazivZu}",
                    brojKolona: 6,
                    landscape: false);

                int headerRow = 4;
                string[] kolone = { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место", "Статус провере" };
                _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

                int trenutniRed = headerRow + 1;
                foreach (var item in grupa)
                {
                    ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                    ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 2).Value = item.Nivo;
                    ws.Cell(trenutniRed, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var jmbgCell = ws.Cell(trenutniRed, 3);
                    jmbgCell.SetValue(item.Jmbg);
                    jmbgCell.Style.NumberFormat.Format = "@";
                    jmbgCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 4).Value = $"{item.Prezime} {item.Ime}";
                    ws.Cell(trenutniRed, 5).Value = item.RadnoMesto;
                    ws.Cell(trenutniRed, 6).Value = item.RazlogNeslaganja;

                    _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona: 6);
                    trenutniRed++;
                }

                _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona: 6, 5, 6);
            }

            return _pomocnik.KonvertujUBajtove(workbook);
        }
    }
}

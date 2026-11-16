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

                _pomocnik.PrimeniZaglavljeLista(
                    ws,
                    "ИЗВЕШТАЈ: РАДНИЦИ НА СПИСКУ РФЗО-А КОЈИ НИСУ НА СПИСКУ ЗУ",
                    $"Шифра ЗУ: {prvi.SifraZuNormalizovana} — {prvi.NazivZu}",
                    brojKolona: 8,
                    landscape: true);

                int headerRow = 4;
                string[] kolone = { "Период", "ЈМБГ", "Презиме и име", "Радно место / Занимање", "Коеф. 1", "% РВ", "Престанак РО", "Статус провере" };
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

                    ws.Cell(trenutniRed, 5).Value = item.Koef1.HasValue ? item.Koef1.Value.ToString("N4") : "-";
                    ws.Cell(trenutniRed, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 6).Value = item.ProcRVr.HasValue ? item.ProcRVr.Value.ToString("N2") : "-";
                    ws.Cell(trenutniRed, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(trenutniRed, 7).Value = item.DatumPrestankaRO.HasValue ? item.DatumPrestankaRO.Value.ToString("dd.MM.yyyy") : "-";
                    ws.Cell(trenutniRed, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 8).Value = item.RazlogNeslaganja;

                    _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona: 8);
                    trenutniRed++;
                }

                _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona: 8, 4, 8);
            }

            return _pomocnik.KonvertujUBajtove(workbook);
        }
    }
}

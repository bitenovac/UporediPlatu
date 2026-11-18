using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services.Excel;

public class ExcelNepronadjeniUZuServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelNepronadjeniUZuServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

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
            var nazivLista = _pomocnik.OcistiNazivLista(grupa.Key);

            var ws = workbook.Worksheets.Add(nazivLista);
            var brojKolona = 4;

            // 1. Главни наслов извештаја
            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Радници на списку РФЗО-а који нису на списку ЗУ",
                $"Шифра ЗУ: {prvi.SifraZuNormalizovana} | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona);

            // 3. Жути ред са разлогом неслагања
            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу");

            // 4. Заглавље табеле (4 колоне)
            var headerRow = 4;
            string[] kolone = { "Период", "ЈМБГ", "Презиме и име", "Радно место / Занимање" };
            _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

            var trenutniRed = headerRow + 1;
            foreach (var item in grupa)
            {
                ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                _pomocnik.UpisiJmbg(ws.Cell(trenutniRed, 2), item.Jmbg);

                ws.Cell(trenutniRed, 3).Value = $"{item.Prezime} {item.Ime}";
                ws.Cell(trenutniRed, 4).Value =
                    string.IsNullOrWhiteSpace(item.NazivRM) ? item.NazivZanimanja : item.NazivRM;

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona, 4);
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
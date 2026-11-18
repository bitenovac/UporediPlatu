using ClosedXML.Excel;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Excel;

public class ExcelNeslaganjeKoeficijenataServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelNeslaganjeKoeficijenataServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

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
            var nazivLista = _pomocnik.OcistiNazivLista(
                IzvestajPomocnik.SifraZaPrikaz(prvi.SifraZuOriginal, grupa.Key));

            var ws = workbook.Worksheets.Add(nazivLista);
            var brojKolona = 9;

            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Неслагање у основним и укупним коефицијентима (РФЗО и ЗУ)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona,
                true);

            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "Утврђена је разлика у основном или укупном коефицијенту између РФЗО и ЗУ евиденције");

            var headerRow = 4;
            string[] kolone =
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Назив радног места (РФЗО)",
                "Осн. коеф. (РФЗО)",
                "Осн. коеф. (ЗУ)",
                "Укуп. коеф. (РФЗО)",
                "Укуп. коеф. (ЗУ)"
            };
            _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

            var trenutniRed = headerRow + 1;
            foreach (var item in grupa)
            {
                ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                _pomocnik.UpisiJmbg(ws.Cell(trenutniRed, 2), item.Jmbg);

                ws.Cell(trenutniRed, 3).Value = item.Ime;
                ws.Cell(trenutniRed, 4).Value = item.Prezime;

                var rmCell = ws.Cell(trenutniRed, 5);
                rmCell.Value = item.NazivRM;
                rmCell.Style.Alignment.WrapText = true;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 6), item.OsnovniKoefRfzo);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 7), item.OsnovniKoefZu);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 8), item.UkupniKoefRfzo);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 9), item.UkupniKoefZu);

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona, 5);
            ws.Column(5).Width = 38;
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
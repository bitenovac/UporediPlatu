using ClosedXML.Excel;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Excel;

public class ExcelPreko65GodinaServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelPreko65GodinaServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

    public byte[] Generisi(List<IzvestajPreko65GodinaViewModel> podaci)
    {
        var sortirano = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.DatumRodjenja)
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
            var brojKolona = 11;

            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Запослени старији од 65 година живота (услов за старосну пензију)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona,
                true);

            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "Напомена: Црвеним словима су означени запослени који у обрачунском месецу пуне 65 година живота.");

            var headerRow = 4;
            string[] kolone =
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Датум рођења",
                "Редован рад",
                "Прековремени сати",
                "Дежурство сати",
                "По позиву сати",
                "Приправност сати",
                "Укупно нето"
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

                ws.Cell(trenutniRed, 5).Value = item.DatumRodjenja.ToString("dd.MM.yyyy");
                ws.Cell(trenutniRed, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 6), item.RedovanRad);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 7), item.PrekovremeniSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 8), item.DezurstvoSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 9), item.PoPozivuSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 10), item.PripravnostSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 11), item.UkupanNeto);
                ws.Cell(trenutniRed, 11).Style.Font.Bold = true;

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Crveni font ako radnik puni 65 godina tačno u obračunskom mesecu
                if (item.Puni65UTomMesecu)
                {
                    var redRange = ws.Range(trenutniRed, 1, trenutniRed, brojKolona);
                    redRange.Style.Font.FontColor = XLColor.FromHtml("#DC3545");
                    redRange.Style.Font.Bold = true;
                }

                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona);
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
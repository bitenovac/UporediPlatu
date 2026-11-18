using ClosedXML.Excel;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Excel;

public class ExcelPrekovremeniSatiServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelPrekovremeniSatiServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

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
            var nazivLista = _pomocnik.OcistiNazivLista(
                IzvestajPomocnik.SifraZaPrikaz(prvi.SifraZuOriginal, grupa.Key));

            var ws = workbook.Worksheets.Add(nazivLista);
            var brojKolona = 10;

            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Преглед прековременог рада, дежурства, рада по позиву и приправности (ЗУ)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona,
                true);

            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "Приказ запослених из ЗУ евиденције код којих је збир додатних сати већи од нуле (сортирано опадајуће)");

            var headerRow = 4;
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

            var trenutniRed = headerRow + 1;
            foreach (var item in grupa)
            {
                _pomocnik.UpisiJmbg(ws.Cell(trenutniRed, 1), item.Jmbg);

                ws.Cell(trenutniRed, 2).Value = item.Ime;
                ws.Cell(trenutniRed, 3).Value = item.Prezime;

                var rmCell = ws.Cell(trenutniRed, 4);
                rmCell.Value = item.RadnoMesto;
                rmCell.Style.Alignment.WrapText = true;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 5), item.PrekovremeniSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 6), item.DezurstvoSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 7), item.PoPozivuSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 8), item.PripravnostSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 9), item.UkupnoDodatnihSati);
                ws.Cell(trenutniRed, 9).Style.Font.Bold = true;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 10), item.RadnoVreme);

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona, 4);
            ws.Column(4).Width = 36;
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
using ClosedXML.Excel;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Excel;

public class ExcelNepunoRadnoVremeServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelNepunoRadnoVremeServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

    public byte[] Generisi(List<IzvestajNepunoRadnoVremeViewModel> podaci)
    {
        var sortirano = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.ProcenatRadnogVremenaRfzo)
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
                "Контрола запослених са непуним радним временом (РФЗО vs ЗУ)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona,
                true);

            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "Напомена: Црвеним фонтом су истакнути запослени код којих су сати редовног рада већи од максимално дозвољеног фонда.");

            var headerRow = 4;
            string[] kolone =
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Радно место",
                "% РВ (РФЗО)",
                "Макс. сати (РФЗО)",
                "Редован рад (ЗУ)",
                "Прековремено (ЗУ)",
                "Дежурство (ЗУ)",
                "Укупно нето (ЗУ)"
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
                rmCell.Value = item.RadnoMesto;
                rmCell.Style.Alignment.WrapText = true;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 6), item.ProcenatRadnogVremenaRfzo);
                ws.Cell(trenutniRed, 6).Style.Font.Bold = true;

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 7), item.MaksimalnoDozvoljeniSati);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 8), item.RedovanRadZu);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 9), item.PrekovremeniSatiZu);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 10), item.DezurstvoSatiZu);

                _pomocnik.UpisiBroj(ws.Cell(trenutniRed, 11), item.UkupanNetoZu);
                ws.Cell(trenutniRed, 11).Style.Font.Bold = true;

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                ws.Row(trenutniRed).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Ceo red crvenim fontom kada postoji prekoračenje
                if (item.RedovanRadZu > item.MaksimalnoDozvoljeniSati)
                {
                    var redRange = ws.Range(trenutniRed, 1, trenutniRed, brojKolona);
                    redRange.Style.Font.FontColor = XLColor.FromHtml("#DC3545");
                    redRange.Style.Font.Bold = true;
                }

                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona, 5);
            ws.Column(5).Width = 36;
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
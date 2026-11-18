using ClosedXML.Excel;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Excel;

public class ExcelNepronadjeniURfzoServis
{
    private readonly ExcelPomocnik _pomocnik;

    public ExcelNepronadjeniURfzoServis(ExcelPomocnik pomocnik) => _pomocnik = pomocnik;

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
            var nazivLista = _pomocnik.OcistiNazivLista(
                IzvestajPomocnik.SifraZaPrikaz(prvi.SifraZuOriginal, grupa.Key));

            var ws = workbook.Worksheets.Add(nazivLista);
            var brojKolona = 5;

            // 1. Главни наслов извештаја
            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Радници који се налазе на списку ЗУ а који нису на списку РФЗО-а",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona);

            // 3. Жути ред са разлогом неслагања
            _pomocnik.PrimeniStatusnuTraku(ws, 3, brojKolona,
                "ЈМБГ није пронађен у РФЗО евиденцији за овај период");

            // 4. Заглавље табеле (5 колона без статуса провере)
            var headerRow = 4;
            string[] kolone = { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место" };
            _pomocnik.PrimeniZaglavljeTabele(ws, headerRow, kolone);

            var trenutniRed = headerRow + 1;
            foreach (var item in grupa)
            {
                ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(trenutniRed, 2).Value = item.Nivo;
                ws.Cell(trenutniRed, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                _pomocnik.UpisiJmbg(ws.Cell(trenutniRed, 3), item.Jmbg);

                ws.Cell(trenutniRed, 4).Value = $"{item.Prezime} {item.Ime}";
                ws.Cell(trenutniRed, 5).Value = item.RadnoMesto;

                _pomocnik.StilizujRedPodataka(ws, trenutniRed, brojKolona);
                trenutniRed++;
            }

            _pomocnik.PodesiSirinuKolona(ws, headerRow, trenutniRed - 1, brojKolona, 5);
        }

        return _pomocnik.KonvertujUBajtove(workbook);
    }
}
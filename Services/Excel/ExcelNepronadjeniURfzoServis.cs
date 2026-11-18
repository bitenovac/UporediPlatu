using ClosedXML.Excel;
using UporediPlate.Models;

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
                string.IsNullOrWhiteSpace(prvi.SifraZuOriginal) ? grupa.Key : prvi.SifraZuOriginal);

            var ws = workbook.Worksheets.Add(nazivLista);
            var brojKolona = 5;

            // 1. Главни наслов извештаја
            _pomocnik.PrimeniZaglavljeLista(
                ws,
                "Радници који се налазе на списку ЗУ а који нису на списку РФЗО-а",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona);

            // Стилизовање 2. реда (подаци о ЗУ) у зелену боју као на екрану
            var zuRange = ws.Range(2, 1, 2, brojKolona);
            zuRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");
            zuRange.Style.Font.FontColor = XLColor.White;
            zuRange.Style.Font.Bold = true;

            // 3. Жути ред са разлогом неслагања
            var statusRow = 3;
            var statusRange = ws.Range(statusRow, 1, statusRow, brojKolona);
            statusRange.Merge();
            statusRange.Value = "ЈМБГ није пронађен у РФЗО евиденцији за овај период";
            statusRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
            statusRange.Style.Font.FontColor = XLColor.FromHtml("#212529");
            statusRange.Style.Font.Bold = true;
            statusRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            statusRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(statusRow).Height = 22;

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

                var jmbgCell = ws.Cell(trenutniRed, 3);
                jmbgCell.SetValue(item.Jmbg);
                jmbgCell.Style.NumberFormat.Format = "@";
                jmbgCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

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
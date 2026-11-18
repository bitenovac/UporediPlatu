using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf;

public class PdfPreko65GodinaServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfPreko65GodinaServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajPreko65GodinaViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.DatumRodjenja)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal)
                ? grupa.Key
                : grupa.First().SifraZuOriginal,
            KreirajPdfZaUstanovu,
            "Izvestaj_Preko_65_Godina",
            "Izvestaji_Preko_65_Godina_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajPreko65GodinaViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Запослени старији од 65 година живота (услов за старосну пензију)",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            true);

        var zutaBoja = new Color(255, 193, 7); // #FFC107
        var tamniTekst = new Color(33, 37, 41); // #212529
        var crvenaBoja = new Color(220, 53, 69); // #DC3545

        var statusPar = section.AddParagraph();
        statusPar.Format.Shading.Color = zutaBoja;
        statusPar.Format.Font.Color = tamniTekst;
        statusPar.Format.Font.Bold = true;
        statusPar.Format.Font.Size = 8.5;
        statusPar.Format.LeftIndent = Unit.FromPoint(4);
        statusPar.Format.RightIndent = Unit.FromPoint(4);
        statusPar.Format.SpaceBefore = Unit.FromPoint(0);
        statusPar.Format.SpaceAfter = Unit.FromPoint(6);
        statusPar.Format.Borders.Top.Width = 4;
        statusPar.Format.Borders.Top.Color = zutaBoja;
        statusPar.Format.Borders.Bottom.Width = 4;
        statusPar.Format.Borders.Bottom.Color = zutaBoja;
        statusPar.AddText(
            "Напомена: Црвеним словима су означени запослени који у обрачунском месецу пуне 65 година живота.");

        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 1.7, 2.6, 2.6, 3.0, 2.2, 2.0, 2.0, 2.0, 2.0, 2.0, 2.6 },
            new[]
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Датум\nрођења",
                "Редован\nрад",
                "Прековр.\nсати",
                "Дежурство\nсати",
                "По позиву\nсати",
                "Приправ.\nсати",
                "Укупно\nнето"
            });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.VerticalAlignment = VerticalAlignment.Center;

            if (item.Puni65UTomMesecu)
            {
                row.Format.Font.Color = crvenaBoja;
                row.Format.Font.Bold = true;
            }

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph(item.Ime);
            row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
            row.Cells[4].AddParagraph(item.DatumRodjenja.ToString("dd.MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[5].AddParagraph(item.RedovanRad.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[6].AddParagraph(item.PrekovremeniSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[7].AddParagraph(item.DezurstvoSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[8].AddParagraph(item.PoPozivuSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[9].AddParagraph(item.PripravnostSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;

            var netoPar = row.Cells[10].AddParagraph(item.UkupanNeto.ToString("N2"));
            netoPar.Format.Alignment = ParagraphAlignment.Right;
            netoPar.Format.Font.Bold = true;
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
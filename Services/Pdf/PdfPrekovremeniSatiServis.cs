using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf;

public class PdfPrekovremeniSatiServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfPrekovremeniSatiServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(
        List<IzvestajPrekovremeniSatiViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.UkupnoDodatnihSati)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal)
                ? grupa.Key
                : grupa.First().SifraZuOriginal,
            KreirajPdfZaUstanovu,
            "Izvestaj_Prekovremeni_Sati",
            "Izvestaji_Prekovremeni_Sati_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajPrekovremeniSatiViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Преглед прековременог рада, дежурства, рада по позиву и приправности (ЗУ)",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            true);

        var zutaBoja = new Color(255, 193, 7); // #FFC107
        var tamniTekst = new Color(33, 37, 41); // #212529

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
            "Приказ запослених из ЗУ евиденције код којих је збир додатних сати већи од нуле (сортирано опадајуће)");

        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 2.5, 2.3, 2.6, 7.5, 2.0, 2.0, 2.0, 2.0, 2.2, 2.2 },
            new[]
            {
                "ЈМБГ",
                "Име",
                "Презиме",
                "Радно место",
                "Прековр.\nсати",
                "Дежурство\nсати",
                "По позиву\nсати",
                "Приправ.\nсати",
                "Укупно\nсати",
                "Радно\nвреме"
            });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.VerticalAlignment = VerticalAlignment.Center;

            row.Cells[0].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Ime);
            row.Cells[2].AddParagraph(item.Prezime).Format.Font.Bold = true;
            row.Cells[3].AddParagraph(item.RadnoMesto);
            row.Cells[4].AddParagraph(item.PrekovremeniSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[5].AddParagraph(item.DezurstvoSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[6].AddParagraph(item.PoPozivuSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
            row.Cells[7].AddParagraph(item.PripravnostSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;

            var ukupnoPar = row.Cells[8].AddParagraph(item.UkupnoDodatnihSati.ToString("N2"));
            ukupnoPar.Format.Alignment = ParagraphAlignment.Right;
            ukupnoPar.Format.Font.Bold = true;

            row.Cells[9].AddParagraph(item.RadnoVreme.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
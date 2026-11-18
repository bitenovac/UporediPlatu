using MigraDocCore.DocumentObjectModel;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf;

public class PdfNepronadjeniURfzoServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfNepronadjeniURfzoServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajNepronadjeniViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Nivo)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal)
                ? grupa.Key
                : grupa.First().SifraZuOriginal,
            KreirajPdfZaUstanovu,
            "Izvestaj_Nepronadjeni_U_RFZO",
            "Izvestaji_Nepronadjeni_U_RFZO_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajNepronadjeniViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Радници који се налазе на списку ЗУ а који нису на списку РФЗО-а",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            false);

        var zutaBoja = new Color(255, 193, 7); // #FFC107
        var tamniTekst = new Color(33, 37, 41); // #212529

        // Жута трака са разлогом неслагања
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
        statusPar.AddText("ЈМБГ није пронађен у РФЗО евиденцији за овај период");

        // Табела са 5 колона (без колоне Статус провере)
        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 1.8, 1.4, 2.8, 4.6, 8.0 },
            new[] { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место" });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Nivo).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[3].AddParagraph($"{item.Prezime} {item.Ime}").Format.Font.Bold = true;
            row.Cells[4].AddParagraph(item.RadnoMesto);
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
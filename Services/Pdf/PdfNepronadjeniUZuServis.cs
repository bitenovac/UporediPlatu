using MigraDoc.DocumentObjectModel;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf;

public class PdfNepronadjeniUZuServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfNepronadjeniUZuServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(
        List<IzvestajNepronadjeniUZuViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => string.IsNullOrWhiteSpace(grupa.Key) ? "BezSifre" : grupa.Key,
            KreirajPdfZaUstanovu,
            "Izvestaj_Nepronadjeni_U_ZU",
            "Izvestaji_Nepronadjeni_U_ZU_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajNepronadjeniUZuViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Радници на списку РФЗО-а који нису на списку ЗУ",
            $"Шифра ЗУ: {prvi.SifraZuNormalizovana} | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            false);


        // Жута трака са разлогом неслагања
        _pomocnik.DodajStatusnuTraku(section,
            "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу");

        // Табела са 4 колоне
        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 2.0, 3.0, 5.0, 8.6 },
            new[] { "Период", "ЈМБГ", "Презиме и име", "Радно место / Занимање" });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph($"{item.Prezime} {item.Ime}").Format.Font.Bold = true;
            row.Cells[3].AddParagraph(string.IsNullOrWhiteSpace(item.NazivRM) ? item.NazivZanimanja : item.NazivRM);
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
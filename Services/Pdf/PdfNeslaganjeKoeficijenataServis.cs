using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Pdf;

public class PdfNeslaganjeKoeficijenataServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfNeslaganjeKoeficijenataServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(
        List<IzvestajNeslaganjeKoeficijenataViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => IzvestajPomocnik.SifraZaPrikaz(grupa.First().SifraZuOriginal, grupa.Key),
            KreirajPdfZaUstanovu,
            "Izvestaj_Neslaganje_Koeficijenata",
            "Izvestaji_Neslaganje_Koeficijenata_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajNeslaganjeKoeficijenataViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Неслагање у основним и укупним коефицијентима (РФЗО и ЗУ)",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            true);


        _pomocnik.DodajStatusnuTraku(section,
            "Утврђена је разлика у основном или укупном коефицијенту између РФЗО и ЗУ евиденције");

        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 1.6, 2.7, 2.7, 3.0, 8.5, 2.2, 2.2, 2.2, 2.2 },
            new[]
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Назив радног места (РФЗО)",
                "Осн. коеф.\n(РФЗО)",
                "Осн. коеф.\n(ЗУ)",
                "Укуп. коеф.\n(РФЗО)",
                "Укуп. коеф.\n(ЗУ)"
            });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.VerticalAlignment = VerticalAlignment.Center;

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph(item.Ime);
            row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
            row.Cells[4].AddParagraph(item.NazivRM);
            _pomocnik.DodajBroj(row.Cells[5], item.OsnovniKoefRfzo);
            _pomocnik.DodajBroj(row.Cells[6], item.OsnovniKoefZu);
            _pomocnik.DodajBroj(row.Cells[7], item.UkupniKoefRfzo);
            _pomocnik.DodajBroj(row.Cells[8], item.UkupniKoefZu);
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
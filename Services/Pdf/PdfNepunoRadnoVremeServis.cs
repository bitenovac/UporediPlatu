using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Pdf;

public class PdfNepunoRadnoVremeServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfNepunoRadnoVremeServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(
        List<IzvestajNepunoRadnoVremeViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.ProcenatRadnogVremenaRfzo)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => IzvestajPomocnik.SifraZaPrikaz(grupa.First().SifraZuOriginal, grupa.Key),
            KreirajPdfZaUstanovu,
            "Izvestaj_Nepuno_Radno_Vreme",
            "Izvestaji_Nepuno_Radno_Vreme_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajNepunoRadnoVremeViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Контрола запослених са непуним радним временом (РФЗО vs ЗУ)",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            true);

        var crvenaBoja = new Color(220, 53, 69); // #DC3545

        _pomocnik.DodajStatusnuTraku(section,
            "Напомена: Црвеним фонтом су истакнути запослени код којих су сати редовног рада већи од максимално дозвољеног фонда.");

        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 1.6, 2.4, 2.1, 2.4, 5.8, 1.7, 1.9, 1.9, 1.9, 1.9, 2.5 },
            new[]
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Радно место",
                "% РВ\n(РФЗО)",
                "Макс.\nсати",
                "Редован\nрад (ЗУ)",
                "Прековр.\n(ЗУ)",
                "Дежурство\n(ЗУ)",
                "Укупно\nнето (ЗУ)"
            });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.VerticalAlignment = VerticalAlignment.Center;

            if (item.RedovanRadZu > item.MaksimalnoDozvoljeniSati)
            {
                row.Format.Font.Color = crvenaBoja;
                row.Format.Font.Bold = true;
            }

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph(item.Ime);
            row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
            row.Cells[4].AddParagraph(item.RadnoMesto);

            var procPar = row.Cells[5].AddParagraph(item.ProcenatRadnogVremenaRfzo.ToString("N2") + "%");
            procPar.Format.Alignment = ParagraphAlignment.Right;
            procPar.Format.Font.Bold = true;

            row.Cells[6].AddParagraph(item.MaksimalnoDozvoljeniSati.ToString("N2")).Format.Alignment =
                ParagraphAlignment.Right;
            _pomocnik.DodajBroj(row.Cells[7], item.RedovanRadZu);
            row.Cells[8].AddParagraph(item.PrekovremeniSatiZu.ToString("N2")).Format.Alignment =
                ParagraphAlignment.Right;
            _pomocnik.DodajBroj(row.Cells[9], item.DezurstvoSatiZu);

            var netoPar = row.Cells[10].AddParagraph(item.UkupanNetoZu.ToString("N2"));
            netoPar.Format.Alignment = ParagraphAlignment.Right;
            netoPar.Format.Font.Bold = true;
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
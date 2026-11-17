using System.Collections.Generic;
using System.Linq;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf
{
    public class PdfNeslaganjeKoeficijenataServis
    {
        private readonly PdfPomocnik _pomocnik;

        public PdfNeslaganjeKoeficijenataServis(PdfPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajNeslaganjeKoeficijenataViewModel> podaci)
        {
            var grupePoZu = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .GroupBy(x => x.SifraZuNormalizovana);

            return _pomocnik.SpakujPdfIliZip(
                grupePoZu,
                grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal) ? grupa.Key : grupa.First().SifraZuOriginal,
                KreirajPdfZaUstanovu,
                prefiksFajla: "Izvestaj_Neslaganje_Koeficijenata",
                nazivZipArhive: "Izvestaji_Neslaganje_Koeficijenata_PDF");
        }

        private byte[] KreirajPdfZaUstanovu(List<IzvestajNeslaganjeKoeficijenataViewModel> stavke)
        {
            var prvi = stavke.First();
            var doc = _pomocnik.KreirajOsnovniDokument(
                out var section,
                "Неслагање у основним и укупним коефицијентима (РФЗО и ЗУ)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
                landscape: true);

            var zutaBoja = new Color(255, 193, 7);     // #FFC107
            var tamniTekst = new Color(33, 37, 41);    // #212529

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
            statusPar.AddText("Утврђена је разлика у основном или укупном коефицијенту између РФЗО и ЗУ евиденције");

            var table = _pomocnik.KreirajTabeluSaZaglavljem(
                section,
                new double[] { 1.6, 2.7, 2.7, 3.0, 8.5, 2.2, 2.2, 2.2, 2.2 },
                new string[]
                {
                    "Период",
                    "ЈМБГ",
                    "Име",
                    "Презиме",
                    "Назив радног места (РФЗО)",
                    "Осн. коэф.\n(РФЗО)",
                    "Осн. коэф.\n(ЗУ)",
                    "Укуп. коэф.\n(РФЗО)",
                    "Укуп. коэф.\n(ЗУ)"
                });

            foreach (var item in stavke)
            {
                var row = table.AddRow();
                row.TopPadding = Unit.FromPoint(3);
                row.BottomPadding = Unit.FromPoint(3);
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[2].AddParagraph(item.Ime);
                row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
                row.Cells[4].AddParagraph(item.NazivRM);
                row.Cells[5].AddParagraph(item.OsnovniKoefRfzo.ToString("N4")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].AddParagraph(item.OsnovniKoefZu.ToString("N4")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].AddParagraph(item.UkupniKoefRfzo.ToString("N4")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[8].AddParagraph(item.UkupniKoefZu.ToString("N4")).Format.Alignment = ParagraphAlignment.Right;
            }

            return _pomocnik.RenderujDokument(doc, section);
        }
    }
}

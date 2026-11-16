using System.Collections.Generic;
using System.Linq;
using MigraDocCore.DocumentObjectModel;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf
{
    public class PdfNepronadjeniUZuServis
    {
        private readonly PdfPomocnik _pomocnik;

        public PdfNepronadjeniUZuServis(PdfPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajNepronadjeniUZuViewModel> podaci)
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
                prefiksFajla: "Izvestaj_Nepronadjeni_U_ZU",
                nazivZipArhive: "Izvestaji_Nepronadjeni_U_ZU_PDF");
        }

        private byte[] KreirajPdfZaUstanovu(List<IzvestajNepronadjeniUZuViewModel> stavke)
        {
            var prvi = stavke.First();
            var doc = _pomocnik.KreirajOsnovniDokument(
                out var section,
                "ИЗВЕШТАЈ: РАДНИЦИ НА СПИСКУ РФЗО-А КОЈИ НИСУ НА СПИСКУ ЗУ",
                $"Шифра ЗУ: {prvi.SifraZuNormalizovana} — {prvi.NazivZu}",
                landscape: true);

            var table = _pomocnik.KreirajTabeluSaZaglavljem(
                section,
                new double[] { 1.7, 2.7, 4.0, 6.3, 1.8, 1.6, 2.4, 6.8 },
                new string[] { "Период", "ЈМБГ", "Презиме и име", "Радно место / Занимање", "Коеф. 1", "% РВ", "Престанак РО", "Статус провере" });

            foreach (var item in stavke)
            {
                var row = table.AddRow();
                row.TopPadding = Unit.FromPoint(3);
                row.BottomPadding = Unit.FromPoint(3);

                row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[2].AddParagraph($"{item.Prezime} {item.Ime}").Format.Font.Bold = true;
                row.Cells[3].AddParagraph(string.IsNullOrWhiteSpace(item.NazivRM) ? item.NazivZanimanja : item.NazivRM);
                row.Cells[4].AddParagraph(item.Koef1.HasValue ? item.Koef1.Value.ToString("N4") : "-").Format.Alignment = ParagraphAlignment.Right;
                row.Cells[5].AddParagraph(item.ProcRVr.HasValue ? item.ProcRVr.Value.ToString("N2") : "-").Format.Alignment = ParagraphAlignment.Right;
                row.Cells[6].AddParagraph(item.DatumPrestankaRO.HasValue ? item.DatumPrestankaRO.Value.ToString("dd.MM.yyyy") : "-").Format.Alignment = ParagraphAlignment.Center;
                row.Cells[7].AddParagraph(item.RazlogNeslaganja);
            }

            return _pomocnik.RenderujDokument(doc, section);
        }
    }
}

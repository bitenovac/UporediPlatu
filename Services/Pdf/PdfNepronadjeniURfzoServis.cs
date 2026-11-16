using System.Collections.Generic;
using System.Linq;
using MigraDocCore.DocumentObjectModel;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf
{
    public class PdfNepronadjeniURfzoServis
    {
        private readonly PdfPomocnik _pomocnik;

        public PdfNepronadjeniURfzoServis(PdfPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

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
                grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal) ? grupa.Key : grupa.First().SifraZuOriginal,
                KreirajPdfZaUstanovu,
                prefiksFajla: "Izvestaj_Nepronadjeni_U_RFZO",
                nazivZipArhive: "Izvestaji_Nepronadjeni_U_RFZO_PDF");
        }

        private byte[] KreirajPdfZaUstanovu(List<IzvestajNepronadjeniViewModel> stavke)
        {
            var prvi = stavke.First();
            var doc = _pomocnik.KreirajOsnovniDokument(
                out var section,
                "ИЗВЕШТАЈ О КОНТРОЛИ ПОДАТАКА О ДОСТАВЉЕНИМ ЗАРАДАМА",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) — {prvi.NazivZu}",
                landscape: false);

            var table = _pomocnik.KreirajTabeluSaZaglavljem(
                section,
                new double[] { 1.6, 1.2, 2.6, 3.8, 5.4, 4.0 },
                new string[] { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место", "Статус провере" });

            foreach (var item in stavke)
            {
                var row = table.AddRow();
                row.TopPadding = Unit.FromPoint(3);
                row.BottomPadding = Unit.FromPoint(3);

                row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph(item.Nivo).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[2].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[3].AddParagraph($"{item.Prezime} {item.Ime}").Format.Font.Bold = true;
                row.Cells[4].AddParagraph(item.RadnoMesto);
                row.Cells[5].AddParagraph(item.RazlogNeslaganja);
            }

            return _pomocnik.RenderujDokument(doc, section);
        }
    }
}

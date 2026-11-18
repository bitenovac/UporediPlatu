using System.Collections.Generic;
using System.Linq;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using UporediPlate.Models;

namespace UporediPlate.Services.Pdf
{
    public class PdfNepunoRadnoVremeServis
    {
        private readonly PdfPomocnik _pomocnik;

        public PdfNepunoRadnoVremeServis(PdfPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajNepunoRadnoVremeViewModel> podaci)
        {
            var grupePoZu = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.ProcenatRadnogVremenaRfzo)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .GroupBy(x => x.SifraZuNormalizovana);

            return _pomocnik.SpakujPdfIliZip(
                grupePoZu,
                grupa => string.IsNullOrWhiteSpace(grupa.First().SifraZuOriginal) ? grupa.Key : grupa.First().SifraZuOriginal,
                KreirajPdfZaUstanovu,
                prefiksFajla: "Izvestaj_Nepuno_Radno_Vreme",
                nazivZipArhive: "Izvestaji_Nepuno_Radno_Vreme_PDF");
        }

        private byte[] KreirajPdfZaUstanovu(List<IzvestajNepunoRadnoVremeViewModel> stavke)
        {
            var prvi = stavke.First();
            var doc = _pomocnik.KreirajOsnovniDokument(
                out var section,
                "Контрола запослених са непуним радним временом (РФЗО vs ЗУ)",
                $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
                landscape: true);

            var zutaBoja = new Color(255, 193, 7);     // #FFC107
            var tamniTekst = new Color(33, 37, 41);    // #212529
            var crvenaBoja = new Color(220, 53, 69);   // #DC3545

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
            statusPar.AddText("Напомена: Црвеним фонтом су истакнути запослени код којих су сати редовног рада већи од максимално дозвољеног фонда.");

            var table = _pomocnik.KreirajTabeluSaZaglavljem(
                section,
                new double[] { 1.6, 2.4, 2.1, 2.4, 5.8, 1.7, 1.9, 1.9, 1.9, 1.9, 2.5 },
                new string[]
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

                row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
                row.Cells[2].AddParagraph(item.Ime);
                row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
                row.Cells[4].AddParagraph(item.RadnoMesto);

                var procPar = row.Cells[5].AddParagraph(item.ProcenatRadnogVremenaRfzo.ToString("N2") + "%");
                procPar.Format.Alignment = ParagraphAlignment.Right;
                procPar.Format.Font.Bold = true;

                row.Cells[6].AddParagraph(item.MaksimalnoDozvoljeniSati.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[7].AddParagraph(item.RedovanRadZu.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[8].AddParagraph(item.PrekovremeniSatiZu.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;
                row.Cells[9].AddParagraph(item.DezurstvoSatiZu.ToString("N2")).Format.Alignment = ParagraphAlignment.Right;

                var netoPar = row.Cells[10].AddParagraph(item.UkupanNetoZu.ToString("N2"));
                netoPar.Format.Alignment = ParagraphAlignment.Right;
                netoPar.Format.Font.Bold = true;
            }

            return _pomocnik.RenderujDokument(doc, section);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using UporediPlate.Models;

namespace UporediPlate.Services
{
    public class PdfIzvozServis
    {
        public (byte[] Sadrzaj, string ContentType, string NazivFajla) GenerisiPdfIliZipNepronadjeni(List<IzvestajNepronadjeniViewModel> podaci)
        {
            var grupePoZu = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.Nivo)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .GroupBy(x => x.SifraZuNormalizovana)
                .ToList();

            if (grupePoZu.Count == 1)
            {
                var grupa = grupePoZu.First();
                var prvi = grupa.First();
                string sifra = string.IsNullOrWhiteSpace(prvi.SifraZuOriginal) ? grupa.Key : prvi.SifraZuOriginal;
                byte[] pdfBytes = KreirajPdfZaUstanovu(grupa.ToList());

                return (pdfBytes, "application/pdf", $"Izvestaj_{sifra}_{DateTime.Now:yyyyMMdd}.pdf");
            }

            using var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var grupa in grupePoZu)
                {
                    var prvi = grupa.First();
                    string sifra = string.IsNullOrWhiteSpace(prvi.SifraZuOriginal) ? grupa.Key : prvi.SifraZuOriginal;
                    byte[] pdfBytes = KreirajPdfZaUstanovu(grupa.ToList());

                    var zipEntry = archive.CreateEntry($"Izvestaj_{sifra}_{DateTime.Now:yyyyMMdd}.pdf", CompressionLevel.Fastest);
                    using var entryStream = zipEntry.Open();
                    entryStream.Write(pdfBytes, 0, pdfBytes.Length);
                }
            }

            zipStream.Position = 0;
            return (zipStream.ToArray(), "application/zip", $"Izvestaji_ZU_PDF_{DateTime.Now:yyyyMMdd_HHmm}.zip");
        }

        private byte[] KreirajPdfZaUstanovu(List<IzvestajNepronadjeniViewModel> stavke)
        {
            var prvi = stavke.First();
            var doc = new Document();

            var normalStyle = doc.Styles["Normal"];
            normalStyle.Font.Name = "Arial";
            normalStyle.Font.Size = 8.5;

            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.Orientation = Orientation.Portrait;
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

            var pNaslov = section.AddParagraph("ИЗВЕШТАЈ О КОНТРОЛИ ПОДАТАКА О ДОСТАВЉЕНИМ ЗАРАДАМА");
            pNaslov.Format.Alignment = ParagraphAlignment.Center;
            pNaslov.Format.Font.Size = 12;
            pNaslov.Format.Font.Bold = true;
            pNaslov.Format.Font.Color = new Color(0, 59, 115);
            pNaslov.Format.SpaceAfter = Unit.FromPoint(6);

            var pZu = section.AddParagraph($"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) — {prvi.NazivZu}");
            pZu.Format.Alignment = ParagraphAlignment.Left;
            pZu.Format.Font.Size = 9.5;
            pZu.Format.Font.Bold = true;
            pZu.Format.Font.Color = Colors.White;
            pZu.Format.Shading.Color = new Color(25, 135, 84);
            pZu.Format.SpaceBefore = Unit.FromPoint(2);
            pZu.Format.SpaceAfter = Unit.FromPoint(8);
            pZu.Format.LeftIndent = Unit.FromPoint(4);

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = new Color(180, 180, 180);
            table.Rows.VerticalAlignment = VerticalAlignment.Center;

            table.AddColumn(Unit.FromCentimeter(1.6)); // Период
            table.AddColumn(Unit.FromCentimeter(1.2)); // Ниво
            table.AddColumn(Unit.FromCentimeter(2.6)); // ЈМБГ
            table.AddColumn(Unit.FromCentimeter(3.8)); // Презиме и име
            table.AddColumn(Unit.FromCentimeter(5.4)); // Радно место
            table.AddColumn(Unit.FromCentimeter(4.0)); // Статус провере

            var headerRow = table.AddRow();
            headerRow.HeadingFormat = true;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Format.Font.Bold = true;
            headerRow.Format.Font.Color = Colors.White;
            headerRow.Shading.Color = new Color(0, 59, 115);
            headerRow.TopPadding = Unit.FromPoint(4);
            headerRow.BottomPadding = Unit.FromPoint(4);

            string[] naziviKolona = { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место", "Статус провере" };
            for (int i = 0; i < naziviKolona.Length; i++)
            {
                headerRow.Cells[i].AddParagraph(naziviKolona[i]);
            }

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

            var footer = section.Footers.Primary.AddParagraph();
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.Format.Font.Size = 8;
            footer.Format.Font.Color = Colors.Gray;
            footer.AddText("Страна ");
            footer.AddPageField();
            footer.AddText(" од ");
            footer.AddNumPagesField();

            var pdfRenderer = new PdfDocumentRenderer(unicode: true);
            pdfRenderer.Document = doc;
            pdfRenderer.RenderDocument();

            using var stream = new MemoryStream();
            pdfRenderer.PdfDocument.Save(stream, closeStream: false);
            return stream.ToArray();
        }
    }
}

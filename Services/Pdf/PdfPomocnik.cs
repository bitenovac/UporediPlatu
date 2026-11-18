using System.IO.Compression;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace UporediPlate.Services.Pdf;

public class PdfPomocnik
{
    public Document KreirajOsnovniDokument(out Section section,
        string glavniNaslov,
        string podnaslovZu,
        bool landscape = false)
    {
        var doc = new Document();

        var normalStyle = doc.Styles[StyleNames.Normal]!;
        normalStyle.Font.Name = "Arial";
        normalStyle.Font.Size = 8.5;

        section = doc.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.Orientation = landscape ? Orientation.Landscape : Orientation.Portrait;
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.2);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.2);
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.2);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);

        var pNaslov = section.AddParagraph(glavniNaslov);
        pNaslov.Format.Alignment = ParagraphAlignment.Center;
        pNaslov.Format.Font.Size = 12;
        pNaslov.Format.Font.Bold = true;
        pNaslov.Format.Font.Color = new Color(0, 59, 115);
        pNaslov.Format.SpaceAfter = Unit.FromPoint(6);

        var pZu = section.AddParagraph(podnaslovZu);
        pZu.Format.Alignment = ParagraphAlignment.Left;
        pZu.Format.Font.Size = 9.5;
        pZu.Format.Font.Bold = true;
        pZu.Format.Font.Color = Colors.White;
        pZu.Format.Shading.Color = new Color(25, 135, 84);
        pZu.Format.SpaceBefore = Unit.FromPoint(2);
        pZu.Format.SpaceAfter = Unit.FromPoint(8);
        pZu.Format.LeftIndent = Unit.FromPoint(4);

        return doc;
    }

    public Table KreirajTabeluSaZaglavljem(Section section,
        double[] sirineKolonaCm,
        string[] naziviKolona)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Borders.Color = new Color(180, 180, 180);
        table.Rows.VerticalAlignment = VerticalAlignment.Center;

        foreach (var sirina in sirineKolonaCm)
        {
            table.AddColumn(Unit.FromCentimeter(sirina));
        }

        var headerRow = table.AddRow();
        headerRow.HeadingFormat = true;
        headerRow.Format.Alignment = ParagraphAlignment.Center;
        headerRow.Format.Font.Bold = true;
        headerRow.Format.Font.Color = Colors.White;
        headerRow.Shading.Color = new Color(0, 59, 115);
        headerRow.TopPadding = Unit.FromPoint(4);
        headerRow.BottomPadding = Unit.FromPoint(4);

        for (var i = 0; i < naziviKolona.Length; i++)
        {
            headerRow.Cells[i].AddParagraph(naziviKolona[i]);
        }

        return table;
    }

    public byte[] RenderujDokument(Document doc, Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Right;
        footer.Format.Font.Size = 8;
        footer.Format.Font.Color = Colors.Gray;
        footer.AddText("Страна ");
        footer.AddPageField();
        footer.AddText(" од ");
        footer.AddNumPagesField();

        var pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = doc;
        pdfRenderer.RenderDocument();

        using var stream = new MemoryStream();
        pdfRenderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) SpakujPdfIliZip<T>(
        IEnumerable<IGrouping<string, T>> grupePoZu,
        Func<IGrouping<string, T>, string> dohvatiSifruFajla,
        Func<List<T>, byte[]> kreirajPdfZaGrupu,
        string prefiksFajla,
        string nazivZipArhive)
    {
        var listaGrupa = grupePoZu.ToList();

        if (listaGrupa.Count == 1)
        {
            var grupa = listaGrupa.First();
            var sifra = dohvatiSifruFajla(grupa);
            var pdfBytes = kreirajPdfZaGrupu(grupa.ToList());

            return (pdfBytes, "application/pdf", $"{prefiksFajla}_{sifra}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var grupa in listaGrupa)
            {
                var sifra = dohvatiSifruFajla(grupa);
                var pdfBytes = kreirajPdfZaGrupu(grupa.ToList());

                var zipEntry = archive.CreateEntry($"{prefiksFajla}_{sifra}_{DateTime.Now:yyyyMMdd}.pdf",
                    CompressionLevel.Fastest);
                using var entryStream = zipEntry.Open();
                entryStream.Write(pdfBytes, 0, pdfBytes.Length);
            }
        }

        zipStream.Position = 0;
        return (zipStream.ToArray(), "application/zip", $"{nazivZipArhive}_{DateTime.Now:yyyyMMdd_HHmm}.zip");
    }
}
using System.IO.Compression;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace UporediPlate.Izvestaji.Okvir;

// PDF извоз за сваки извештај: један PDF по здравственој установи, више установа -> ZIP
public static class PdfIzvoz
{
    private static readonly Color ZutaBoja = new(255, 193, 7); // #FFC107
    private static readonly Color TamniTekst = new(33, 37, 41); // #212529
    private static readonly Color CrvenaBoja = new(220, 53, 69); // #DC3545

    public static Datoteka Generisi<T>(Izvestaj<T> izvestaj, List<T> stavke) where T : IStavkaIzvestaja
    {
        var grupe = stavke.GroupBy(x => x.SifraZuNormalizovana).ToList();
        var prefiks = izvestaj.PrefiksFajla;

        string NazivPdfa(IGrouping<string, T> grupa)
        {
            var sifra = IzvestajPomocnik.SifraZaPrikaz(grupa.First().SifraZuOriginal, grupa.Key);
            return $"{prefiks}_{(string.IsNullOrWhiteSpace(sifra) ? "BezSifre" : sifra)}_{DateTime.Now:yyyyMMdd}.pdf";
        }

        if (grupe.Count == 1)
        {
            return new Datoteka(KreirajPdf(izvestaj, grupe[0].ToList()), "application/pdf", NazivPdfa(grupe[0]));
        }

        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var grupa in grupe)
            {
                var pdfBytes = KreirajPdf(izvestaj, grupa.ToList());
                using var entryStream = archive.CreateEntry(NazivPdfa(grupa), CompressionLevel.Fastest).Open();
                entryStream.Write(pdfBytes, 0, pdfBytes.Length);
            }
        }

        // "Izvestaj_Neslaganje_Koeficijenata" -> "Izvestaji_Neslaganje_Koeficijenata_PDF"
        var nazivZipa = $"Izvestaji{prefiks["Izvestaj".Length..]}_PDF";
        return new Datoteka(zipStream.ToArray(), "application/zip", $"{nazivZipa}_{DateTime.Now:yyyyMMdd_HHmm}.zip");
    }

    private static byte[] KreirajPdf<T>(Izvestaj<T> izvestaj, List<T> stavke) where T : IStavkaIzvestaja
    {
        var prvi = stavke.First();
        var kolone = izvestaj.KolonePdfa;
        var istaknutRed = izvestaj.IstaknutRed;

        var doc = KreirajOsnovniDokument(out var section,
            izvestaj.Naslov,
            $"Шифра ЗУ: {Izvestaj<T>.SifraZaNaslov(prvi)} | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            izvestaj.Polozeno);
        DodajStatusnuTraku(section, izvestaj.Napomena);

        var table = KreirajTabeluSaZaglavljem(section,
            kolone.Select(k => k.SirinaCm).ToArray(),
            kolone.Select(k => k.Zaglavlje).ToArray());

        foreach (var stavka in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            if (izvestaj.CentrirajRedove)
            {
                row.VerticalAlignment = VerticalAlignment.Center;
            }

            if (istaknutRed?.Invoke(stavka) == true)
            {
                row.Format.Font.Color = CrvenaBoja;
                row.Format.Font.Bold = true;
            }

            for (var i = 0; i < kolone.Count; i++)
            {
                kolone[i].Upisi(row.Cells[i], stavka);
            }
        }

        return RenderujDokument(doc, section);
    }

    private static Document KreirajOsnovniDokument(out Section section, string glavniNaslov, string podnaslovZu,
        bool landscape)
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

    // Жута трака са напоменом/разлогом испод заглавља документа
    private static void DodajStatusnuTraku(Section section, string tekst)
    {
        var statusPar = section.AddParagraph();
        statusPar.Format.Shading.Color = ZutaBoja;
        statusPar.Format.Font.Color = TamniTekst;
        statusPar.Format.Font.Bold = true;
        statusPar.Format.Font.Size = 8.5;
        statusPar.Format.LeftIndent = Unit.FromPoint(4);
        statusPar.Format.RightIndent = Unit.FromPoint(4);
        statusPar.Format.SpaceBefore = Unit.FromPoint(0);
        statusPar.Format.SpaceAfter = Unit.FromPoint(6);
        statusPar.Format.Borders.Top.Width = 4;
        statusPar.Format.Borders.Top.Color = ZutaBoja;
        statusPar.Format.Borders.Bottom.Width = 4;
        statusPar.Format.Borders.Bottom.Color = ZutaBoja;
        statusPar.AddText(tekst);
    }

    private static Table KreirajTabeluSaZaglavljem(Section section, double[] sirineKolonaCm, string[] naziviKolona)
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

    private static byte[] RenderujDokument(Document doc, Section section)
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
}

using ClosedXML.Excel;

namespace UporediPlate.Izvestaji.Okvir;

// Excel извоз за сваки извештај: један лист по здравственој установи
public static class ExcelIzvoz
{
    private const int RedZaglavlja = 4;

    public static Datoteka Generisi<T>(Izvestaj<T> izvestaj, List<T> stavke) where T : IStavkaIzvestaja
    {
        var kolone = izvestaj.KoloneExcela;
        var brojKolona = kolone.Count;
        var istaknutRed = izvestaj.IstaknutRed;

        using var workbook = new XLWorkbook();
        foreach (var grupa in stavke.GroupBy(x => x.SifraZuNormalizovana))
        {
            var prvi = grupa.First();
            var ws = workbook.Worksheets.Add(
                OcistiNazivLista(IzvestajPomocnik.SifraZaPrikaz(prvi.SifraZuOriginal, grupa.Key)));

            PrimeniZaglavljeLista(ws,
                izvestaj.Naslov,
                $"Шифра ЗУ: {Izvestaj<T>.SifraZaNaslov(prvi)} | Назив ЗУ: {prvi.NazivZu} (Број радника: {grupa.Count()})",
                brojKolona,
                izvestaj.Polozeno);
            PrimeniStatusnuTraku(ws, 3, brojKolona, izvestaj.Napomena);
            PrimeniZaglavljeTabele(ws, RedZaglavlja, kolone.Select(k => k.Zaglavlje).ToArray());

            var red = RedZaglavlja + 1;
            foreach (var stavka in grupa)
            {
                for (var i = 0; i < brojKolona; i++)
                {
                    kolone[i].Upisi(ws.Cell(red, i + 1), stavka);
                }

                StilizujRedPodataka(ws, red, brojKolona);
                if (izvestaj.CentrirajRedove)
                {
                    ws.Row(red).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                }

                if (istaknutRed?.Invoke(stavka) == true)
                {
                    var redRange = ws.Range(red, 1, red, brojKolona);
                    redRange.Style.Font.FontColor = XLColor.FromHtml("#DC3545");
                    redRange.Style.Font.Bold = true;
                }

                red++;
            }

            var koloneSaPrelomom = kolone.Select((k, i) => (k, i)).Where(x => x.k.Prelom).Select(x => x.i + 1).ToArray();
            PodesiSirinuKolona(ws, RedZaglavlja, red - 1, brojKolona, koloneSaPrelomom);
            for (var i = 0; i < brojKolona; i++)
            {
                if (kolone[i].Sirina is { } sirina)
                {
                    ws.Column(i + 1).Width = sirina;
                }
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new Datoteka(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{izvestaj.PrefiksFajla}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    private static string OcistiNazivLista(string? naziv)
    {
        if (string.IsNullOrWhiteSpace(naziv))
        {
            return "BezSifre";
        }

        return naziv.Length > 31 ? naziv.Substring(0, 31) : naziv;
    }

    private static void PrimeniZaglavljeLista(IXLWorksheet ws, string glavniNaslov, string podnaslovZu, int brojKolona,
        bool landscape)
    {
        ws.PageSetup.PageOrientation = landscape ? XLPageOrientation.Landscape : XLPageOrientation.Portrait;
        ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
        ws.PageSetup.FitToPages(1, 0);

        var naslovRange = ws.Range(1, 1, 1, brojKolona);
        naslovRange.Merge();
        naslovRange.Value = glavniNaslov;
        naslovRange.Style.Font.Bold = true;
        naslovRange.Style.Font.FontSize = 13;
        naslovRange.Style.Font.FontColor = XLColor.FromHtml("#003b73");
        naslovRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        naslovRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = 26;

        var zuInfoRange = ws.Range(2, 1, 2, brojKolona);
        zuInfoRange.Merge();
        zuInfoRange.Value = podnaslovZu;
        zuInfoRange.Style.Font.Bold = true;
        zuInfoRange.Style.Font.FontSize = 11;
        zuInfoRange.Style.Font.FontColor = XLColor.White;
        zuInfoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");
        zuInfoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        zuInfoRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(2).Height = 22;
    }

    // Жута трака са напоменом/разлогом испод заглавља листа
    private static void PrimeniStatusnuTraku(IXLWorksheet ws, int red, int brojKolona, string tekst)
    {
        var statusRange = ws.Range(red, 1, red, brojKolona);
        statusRange.Merge();
        statusRange.Value = tekst;
        statusRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC107");
        statusRange.Style.Font.FontColor = XLColor.FromHtml("#212529");
        statusRange.Style.Font.Bold = true;
        statusRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        statusRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(red).Height = 22;
    }

    private static void PrimeniZaglavljeTabele(IXLWorksheet ws, int headerRow, string[] kolone)
    {
        for (var i = 0; i < kolone.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = kolone[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#003b73");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        ws.Row(headerRow).Height = 22;
    }

    private static void StilizujRedPodataka(IXLWorksheet ws, int red, int brojKolona, double visina = 28)
    {
        var rowRange = ws.Range(red, 1, red, brojKolona);
        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        rowRange.Style.Font.FontSize = 10;
        rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(red).Height = visina;
    }

    private static void PodesiSirinuKolona(IXLWorksheet ws, int headerRow, int poslednjiRed, int brojKolona,
        params int[] koloneSaPrelomom)
    {
        for (var col = 1; col <= brojKolona; col++)
        {
            var maxDuzina = 0;
            for (var row = headerRow; row <= poslednjiRed; row++)
            {
                var tekst = ws.Cell(row, col).GetString();
                if (!string.IsNullOrEmpty(tekst) && tekst.Length > maxDuzina)
                {
                    maxDuzina = tekst.Length;
                }
            }

            var optimalnaSirina = maxDuzina * 1.04 + 1.8;
            if (Array.Exists(koloneSaPrelomom, c => c == col))
            {
                optimalnaSirina = optimalnaSirina / 2.0 + 2.5;
                ws.Range(headerRow + 1, col, poslednjiRed, col).Style.Alignment.WrapText = true;
            }

            ws.Column(col).Width = optimalnaSirina;
        }
    }
}

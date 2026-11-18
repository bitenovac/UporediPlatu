using ClosedXML.Excel;

namespace UporediPlate.Services.Excel;

public class ExcelPomocnik
{
    public string OcistiNazivLista(string? naziv)
    {
        if (string.IsNullOrWhiteSpace(naziv))
        {
            return "BezSifre";
        }

        return naziv.Length > 31 ? naziv.Substring(0, 31) : naziv;
    }

    public void PrimeniZaglavljeLista(IXLWorksheet ws,
        string glavniNaslov,
        string podnaslovZu,
        int brojKolona,
        bool landscape = false)
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

    public void PrimeniZaglavljeTabele(IXLWorksheet ws,
        int headerRow,
        string[] kolone)
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

    public void StilizujRedPodataka(IXLWorksheet ws,
        int red,
        int brojKolona,
        double visina = 28)
    {
        var rowRange = ws.Range(red, 1, red, brojKolona);
        rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        rowRange.Style.Font.FontSize = 10;
        rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(red).Height = visina;
    }

    public void PodesiSirinuKolona(IXLWorksheet ws,
        int headerRow,
        int poslednjiRed,
        int brojKolona,
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

    public byte[] KonvertujUBajtove(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
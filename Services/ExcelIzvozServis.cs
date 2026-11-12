using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using UporediPlate.Models;

namespace UporediPlate.Services
{
    public class ExcelIzvozServis
    {
        public byte[] GenerisiExcelNepronadjeni(List<IzvestajNepronadjeniViewModel> podaci)
        {
            var sortirano = podaci
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.Nivo)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .ToList();

            using var workbook = new XLWorkbook();
            var grupePoZu = sortirano.GroupBy(x => x.SifraZuNormalizovana);

            foreach (var grupa in grupePoZu)
            {
                var prvi = grupa.First();
                string nazivLista = string.IsNullOrWhiteSpace(prvi.SifraZuOriginal)
                    ? grupa.Key
                    : prvi.SifraZuOriginal;

                if (nazivLista.Length > 31) nazivLista = nazivLista.Substring(0, 31);
                if (string.IsNullOrWhiteSpace(nazivLista)) nazivLista = "BezSifre";

                var ws = workbook.Worksheets.Add(nazivLista);

                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
                ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
                ws.PageSetup.FitToPages(1, 0);

                // 1. ред: Центрирано заглавље
                var naslovRange = ws.Range("A1:F1");
                naslovRange.Merge();
                naslovRange.Value = "ИЗВЕШТАЈ О КОНТРОЛИ ПОДАТАКА О ДОСТАВЉЕНИМ ЗАРАДАМА";
                naslovRange.Style.Font.Bold = true;
                naslovRange.Style.Font.FontSize = 13;
                naslovRange.Style.Font.FontColor = XLColor.FromHtml("#003b73");
                naslovRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                naslovRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(1).Height = 26;

                // 2. ред: Лево поравнато Шифра ЗУ и Назив ЗУ
                var zuInfoRange = ws.Range("A2:F2");
                zuInfoRange.Merge();
                zuInfoRange.Value = $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) — {prvi.NazivZu}";
                zuInfoRange.Style.Font.Bold = true;
                zuInfoRange.Style.Font.FontSize = 11;
                zuInfoRange.Style.Font.FontColor = XLColor.White;
                zuInfoRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#198754");
                zuInfoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                zuInfoRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Row(2).Height = 22;

                // 4. ред: Заглавље табеле
                int headerRow = 4;
                string[] kolone = { "Период", "Ниво", "ЈМБГ", "Презиме и име", "Радно место", "Статус провере" };
                for (int i = 0; i < kolone.Length; i++)
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

                // Попуњавање редова табеле
                int trenutniRed = headerRow + 1;
                foreach (var item in grupa)
                {
                    ws.Cell(trenutniRed, 1).Value = item.PeriodUvoza.ToString("MM.yyyy");
                    ws.Cell(trenutniRed, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 2).Value = item.Nivo;
                    ws.Cell(trenutniRed, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    var jmbgCell = ws.Cell(trenutniRed, 3);
                    jmbgCell.SetValue(item.Jmbg);
                    jmbgCell.Style.NumberFormat.Format = "@";
                    jmbgCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(trenutniRed, 4).Value = $"{item.Prezime} {item.Ime}";
                    ws.Cell(trenutniRed, 5).Value = item.RadnoMesto;
                    ws.Cell(trenutniRed, 6).Value = item.RazlogNeslaganja;

                    var rowRange = ws.Range(trenutniRed, 1, trenutniRed, 6);
                    rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Font.FontSize = 10;
                    rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    ws.Row(trenutniRed).Height = 28;
                    trenutniRed++;
                }

                // Оптимално подешавање ширине колона
                int poslednjiRed = trenutniRed - 1;
                for (int col = 1; col <= 6; col++)
                {
                    int maxDuzina = 0;
                    for (int row = headerRow; row <= poslednjiRed; row++)
                    {
                        string tekst = ws.Cell(row, col).GetString();
                        if (!string.IsNullOrEmpty(tekst) && tekst.Length > maxDuzina)
                        {
                            maxDuzina = tekst.Length;
                        }
                    }

                    double optimalnaSirina = (maxDuzina * 1.03) + 1.8;
                    if (col == 5 || col == 6)
                    {
                        optimalnaSirina = (optimalnaSirina / 2.0) + 2.0;
                        ws.Range(headerRow + 1, col, poslednjiRed, col).Style.Alignment.WrapText = true;
                    }
                    ws.Column(col).Width = optimalnaSirina;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

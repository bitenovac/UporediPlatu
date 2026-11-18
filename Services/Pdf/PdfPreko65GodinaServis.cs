using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using UporediPlate.Models;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Services.Pdf;

public class PdfPreko65GodinaServis
{
    private readonly PdfPomocnik _pomocnik;

    public PdfPreko65GodinaServis(PdfPomocnik pomocnik) => _pomocnik = pomocnik;

    public (byte[] Sadrzaj, string ContentType, string NazivFajla) Generisi(List<IzvestajPreko65GodinaViewModel> podaci)
    {
        var grupePoZu = podaci
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.DatumRodjenja)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .GroupBy(x => x.SifraZuNormalizovana);

        return _pomocnik.SpakujPdfIliZip(
            grupePoZu,
            grupa => IzvestajPomocnik.SifraZaPrikaz(grupa.First().SifraZuOriginal, grupa.Key),
            KreirajPdfZaUstanovu,
            "Izvestaj_Preko_65_Godina",
            "Izvestaji_Preko_65_Godina_PDF");
    }

    private byte[] KreirajPdfZaUstanovu(List<IzvestajPreko65GodinaViewModel> stavke)
    {
        var prvi = stavke.First();
        var doc = _pomocnik.KreirajOsnovniDokument(
            out var section,
            "Запослени старији од 65 година живота (услов за старосну пензију)",
            $"Шифра ЗУ: {prvi.SifraZuOriginal} ({prvi.SifraZuNormalizovana}) | Назив ЗУ: {prvi.NazivZu} | Број радника: {stavke.Count}",
            true);

        var crvenaBoja = new Color(220, 53, 69); // #DC3545

        _pomocnik.DodajStatusnuTraku(section,
            "Напомена: Црвеним словима су означени запослени који у обрачунском месецу пуне 65 година живота.");

        var table = _pomocnik.KreirajTabeluSaZaglavljem(
            section,
            new[] { 1.7, 2.6, 2.6, 3.0, 2.2, 2.0, 2.0, 2.0, 2.0, 2.0, 2.6 },
            new[]
            {
                "Период",
                "ЈМБГ",
                "Име",
                "Презиме",
                "Датум\nрођења",
                "Редован\nрад",
                "Прековр.\nсати",
                "Дежурство\nсати",
                "По позиву\nсати",
                "Приправ.\nсати",
                "Укупно\nнето"
            });

        foreach (var item in stavke)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromPoint(3);
            row.BottomPadding = Unit.FromPoint(3);
            row.VerticalAlignment = VerticalAlignment.Center;

            if (item.Puni65UTomMesecu)
            {
                row.Format.Font.Color = crvenaBoja;
                row.Format.Font.Bold = true;
            }

            row.Cells[0].AddParagraph(item.PeriodUvoza.ToString("MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            row.Cells[1].AddParagraph(item.Jmbg).Format.Alignment = ParagraphAlignment.Center;
            row.Cells[2].AddParagraph(item.Ime);
            row.Cells[3].AddParagraph(item.Prezime).Format.Font.Bold = true;
            row.Cells[4].AddParagraph(item.DatumRodjenja.ToString("dd.MM.yyyy")).Format.Alignment =
                ParagraphAlignment.Center;
            _pomocnik.DodajBroj(row.Cells[5], item.RedovanRad);
            _pomocnik.DodajBroj(row.Cells[6], item.PrekovremeniSati);
            _pomocnik.DodajBroj(row.Cells[7], item.DezurstvoSati);
            _pomocnik.DodajBroj(row.Cells[8], item.PoPozivuSati);
            _pomocnik.DodajBroj(row.Cells[9], item.PripravnostSati);

            var netoPar = row.Cells[10].AddParagraph(item.UkupanNeto.ToString("N2"));
            netoPar.Format.Alignment = ParagraphAlignment.Right;
            netoPar.Format.Font.Bold = true;
        }

        return _pomocnik.RenderujDokument(doc, section);
    }
}
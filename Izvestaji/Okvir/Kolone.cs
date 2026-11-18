using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace UporediPlate.Izvestaji.Okvir;

// Колона на екрану. Заглавље може да садржи <br/> (константа из кода, не кориснички унос).
public record KolonaEkrana<T>(
    string Zaglavlje,
    Func<T, string> Vrednost,
    string? KlasaZaglavlja = null,
    string? KlasaCelije = null,
    Func<T, string>? UslovnaKlasa = null,
    bool RadnoMesto = false);

// Колона у Excel-у. Prelom: текст се прелама и ширина се рачуна за преломљен текст; Sirina: фиксна ширина.
public record KolonaExcela<T>(string Zaglavlje, Action<IXLCell, T> Upisi, bool Prelom = false, double? Sirina = null);

// Колона у PDF-у, са ширином у центиметрима. Заглавље може да садржи \n.
public record KolonaPdfa<T>(string Zaglavlje, double SirinaCm, Action<Cell, T> Upisi);

// Готове врсте колона - извештај бира колоне, не пише форматирање.
public static class Ekran<T>
{
    public static KolonaEkrana<T> Tekst(string zaglavlje, Func<T, string> vrednost,
        string? klasaZaglavlja = null, string? klasaCelije = null) =>
        new(zaglavlje, vrednost, klasaZaglavlja, klasaCelije);

    public static KolonaEkrana<T> Centrirano(string zaglavlje, Func<T, string> vrednost) =>
        new(zaglavlje, vrednost, "text-center", "text-center");

    public static KolonaEkrana<T> Jmbg(Func<T, string> vrednost, bool centrirano = true) =>
        new("ЈМБГ", vrednost, centrirano ? "text-center" : null, centrirano ? "text-center font-monospace" : "font-monospace");

    // Дугачак назив радног места: скраћује се на екрану, цео текст је у tooltip-у
    public static KolonaEkrana<T> RadnoMesto(string zaglavlje, Func<T, string> vrednost) =>
        new(zaglavlje, vrednost, RadnoMesto: true);

    public static KolonaEkrana<T> Broj(string zaglavlje, Func<T, decimal> vrednost, string? dodatnaKlasa = null,
        Func<T, string>? uslovnaKlasa = null, string sufiks = "") =>
        new(zaglavlje, x => vrednost(x).ToString("N2") + sufiks, "text-end",
            dodatnaKlasa is null ? "text-end" : $"text-end {dodatnaKlasa}", uslovnaKlasa);
}

public static class Excel<T>
{
    public static KolonaExcela<T> Tekst(string zaglavlje, Func<T, string> vrednost, bool centrirano = false,
        bool prelom = false, double? sirina = null) =>
        new(zaglavlje, (cell, x) =>
        {
            cell.Value = vrednost(x);
            if (centrirano)
            {
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            if (prelom)
            {
                cell.Style.Alignment.WrapText = true;
            }
        }, prelom, sirina);

    // ЈМБГ се уписује као текст да Excel не би одсекао водећу нулу
    public static KolonaExcela<T> Jmbg(Func<T, string> vrednost) =>
        new("ЈМБГ", (cell, x) =>
        {
            cell.SetValue(vrednost(x));
            cell.Style.NumberFormat.Format = "@";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        });

    public static KolonaExcela<T> Broj(string zaglavlje, Func<T, decimal> vrednost, bool podebljano = false) =>
        new(zaglavlje, (cell, x) =>
        {
            cell.Value = vrednost(x);
            cell.Style.NumberFormat.Format = "#,##0.00";
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (podebljano)
            {
                cell.Style.Font.Bold = true;
            }
        });
}

public static class Pdf<T>
{
    public static KolonaPdfa<T> Tekst(string zaglavlje, double sirinaCm, Func<T, string> vrednost) =>
        new(zaglavlje, sirinaCm, (cell, x) => cell.AddParagraph(vrednost(x)));

    public static KolonaPdfa<T> Centrirano(string zaglavlje, double sirinaCm, Func<T, string> vrednost) =>
        new(zaglavlje, sirinaCm, (cell, x) => cell.AddParagraph(vrednost(x)).Format.Alignment = ParagraphAlignment.Center);

    public static KolonaPdfa<T> Podebljano(string zaglavlje, double sirinaCm, Func<T, string> vrednost) =>
        new(zaglavlje, sirinaCm, (cell, x) => cell.AddParagraph(vrednost(x)).Format.Font.Bold = true);

    public static KolonaPdfa<T> Jmbg(double sirinaCm, Func<T, string> vrednost) => Centrirano("ЈМБГ", sirinaCm, vrednost);

    // Број са две децимале, поравнат десно
    public static KolonaPdfa<T> Broj(string zaglavlje, double sirinaCm, Func<T, decimal> vrednost,
        bool podebljano = false, string sufiks = "") =>
        new(zaglavlje, sirinaCm, (cell, x) =>
        {
            var paragraph = cell.AddParagraph(vrednost(x).ToString("N2") + sufiks);
            paragraph.Format.Alignment = ParagraphAlignment.Right;
            if (podebljano)
            {
                paragraph.Format.Font.Bold = true;
            }
        });
}

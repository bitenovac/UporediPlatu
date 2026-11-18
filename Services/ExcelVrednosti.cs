using System.Globalization;

namespace UporediPlate.Services;

// Претварање вредности ћелија из учитаних Excel фајлова (РФЗО и ЗУ)
public static class ExcelVrednosti
{
    // Препознаје и "1.234,56" и "1,234.56" запис; враћа null за празну или неисправну вредност
    public static decimal? DekodirajDecimal(object? obj)
    {
        if (obj == null || obj == DBNull.Value)
        {
            return null;
        }

        switch (obj)
        {
            case double d:
                return Convert.ToDecimal(d);
            case decimal dec:
                return dec;
            case float f:
                return Convert.ToDecimal(f);
            case int i:
                return i;
            case long l:
                return l;
        }

        var tekst = obj.ToString()?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(tekst))
        {
            return null;
        }

        tekst = tekst.Replace(" ", "");
        if (tekst.Contains('.') && tekst.Contains(','))
        {
            tekst = tekst.LastIndexOf(',') > tekst.LastIndexOf('.') ? tekst.Replace(".", "").Replace(",", ".") : tekst.Replace(",", "");
        }
        else if (tekst.Contains(','))
        {
            tekst = tekst.Replace(",", ".");
        }

        return decimal.TryParse(tekst, NumberStyles.Any, CultureInfo.InvariantCulture, out var rez) ? rez : null;
    }

    // Прихвата DateTime, текстуални датум или Excel серијски број дана
    public static DateTime? KonvertujDatum(object obj)
    {
        if (string.IsNullOrWhiteSpace(obj.ToString()))
        {
            return null;
        }

        if (obj is DateTime dt)
        {
            return dt;
        }

        var strVal = obj.ToString()!.Trim();
        if (strVal.Contains(".") || strVal.Contains("-"))
        {
            if (DateTime.TryParse(strVal, out var regularanDatum))
            {
                return regularanDatum;
            }

            if (DateTime.TryParseExact(strVal, ["dd.MM.yyyy", "d.M.yyyy", "dd.MM.yyyy.", "yyyy-MM-dd"],
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var specDatum))
            {
                return specDatum;
            }
        }

        var brojDana = DekodirajDecimal(obj);
        if (!brojDana.HasValue)
        {
            return null;
        }

        try
        {
            var dDana = Convert.ToDouble(brojDana.Value);
            if (dDana is > 1 and < 2958465)
            {
                return DateTime.FromOADate(dDana);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}

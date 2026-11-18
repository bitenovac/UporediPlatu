namespace UporediPlate.Models;

// Заједничко за РФЗО и ЗУ увоз - омогућава исте упите над обе табеле
public interface IUvozniZapis
{
    DateTime PeriodUvoza { get; }
    DateTime DatumUvozaPodataka { get; }
}

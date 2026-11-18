using Microsoft.AspNetCore.Mvc;
using UporediPlate.Izvestaji.Okvir;

namespace UporediPlate.Controllers;

public class IzvozIzvestajaController : Controller
{
    private readonly RegistarIzvestaja _registar;

    public IzvozIzvestajaController(RegistarIzvestaja registar) => _registar = registar;

    // нпр. POST /Izvestaj/Izvoz/NeslaganjeKoeficijenata/pdf
    [HttpPost("Izvestaj/Izvoz/{izvestaj}/{format}")]
    public async Task<IActionResult> Izvoz(string izvestaj, string format, string izabraniKljuceviStr)
    {
        var definicija = _registar.Nadji(izvestaj);
        if (definicija == null || string.IsNullOrWhiteSpace(izabraniKljuceviStr) || format is not ("excel" or "pdf"))
        {
            return RedirectToAction("Index", "Izvestaj");
        }

        var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var rezultat = await definicija.GenerisiAsync(kljucevi);
        if (rezultat.BrojStavki == 0)
        {
            return RedirectToAction("Index", "Izvestaj");
        }

        var datoteka = format == "excel" ? rezultat.Excel() : rezultat.Pdf();
        return File(datoteka.Sadrzaj, datoteka.ContentType, datoteka.Naziv);
    }
}

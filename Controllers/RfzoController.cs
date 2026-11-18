using Microsoft.AspNetCore.Mvc;
using UporediPlate.Services;

namespace UporediPlate.Controllers;

public class RfzoController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<RfzoController> _logger;
    private readonly UvozRfzoServis _uvozRfzoServis;

    public RfzoController(AppDbContext context,
        ILogger<RfzoController> logger,
        UvozRfzoServis uvozRfzoServis)
    {
        _context = context;
        _logger = logger;
        _uvozRfzoServis = uvozRfzoServis;
    }

    // Главна РФЗО страница (некадашња акција Rfzo)
    public IActionResult Index()
    {
        var dtIstorija = _uvozRfzoServis.DobijIstorijuUvoza();
        return View(dtIstorija);
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UveziRfzoPodatke(DateTime period, IFormFile excelFajl)
    {
        var (uspeh, poruka) = await _uvozRfzoServis.ObradiRfzoExcel(excelFajl, period);

        if (uspeh)
        {
            TempData["UspehPoruka"] = poruka;
        }
        else
        {
            TempData["GreskaPoruka"] = poruka;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ObrisiUvozRfzo(string vremeKljuc)
    {
        var (uspeh, poruka) = await _uvozRfzoServis.ObrisiUvoz(vremeKljuc);

        if (uspeh)
        {
            TempData["UspehPoruka"] = poruka;
        }
        else
        {
            TempData["GreskaPoruka"] = poruka;
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult PregledUvozaRfzo(string vremeKljuc)
    {
        try
        {
            if (string.IsNullOrEmpty(vremeKljuc) || vremeKljuc.Length < 14)
            {
                return RedirectToAction(nameof(Index));
            }

            var godina = int.Parse(vremeKljuc.Substring(0, 4));
            var mesec = int.Parse(vremeKljuc.Substring(4, 2));
            var dan = int.Parse(vremeKljuc.Substring(6, 2));
            var sat = int.Parse(vremeKljuc.Substring(8, 2));
            var minut = int.Parse(vremeKljuc.Substring(10, 2));
            var sekund = int.Parse(vremeKljuc.Substring(12, 2));

            var pocetakSekunde = new DateTime(godina, mesec, dan, sat, minut, sekund);
            var krajSekunde = pocetakSekunde.AddSeconds(1);

            var radnici = _context.RfzoUvozi
                .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
                .OrderBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .ToList();

            if (radnici.Any())
            {
                ViewBag.NazivFajla = radnici.First().NazivFajla;
                ViewBag.Period = radnici.First().PeriodUvoza.ToString("MM/yyyy");
                return View(radnici);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом отварања прегледа РФЗО.");
        }

        return RedirectToAction(nameof(Index));
    }
}
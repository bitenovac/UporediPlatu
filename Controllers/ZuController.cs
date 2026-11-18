using Microsoft.AspNetCore.Mvc;
using UporediPlate.Services;

namespace UporediPlate.Controllers;

public class ZuController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ZuController> _logger;
    private readonly UvozZuServis _uvozZuServis;

    public ZuController(AppDbContext context,
        ILogger<ZuController> logger,
        UvozZuServis uvozZuServis)
    {
        _context = context;
        _logger = logger;
        _uvozZuServis = uvozZuServis;
    }

    // Главна ЗУ страница (некадашња акција Zu)
    public IActionResult Index()
    {
        var dtIstorija = _uvozZuServis.DobijIstorijuUvoza();
        return View(dtIstorija);
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UveziZuPodatke(DateTime period, IFormFile excelFajl)
    {
        var (uspeh, poruka) = await _uvozZuServis.ObradiZuExcel(excelFajl, period);

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

    public async Task<IActionResult> ObrisiUvozZu(string vremeKljuc)
    {
        var (uspeh, poruka) = await _uvozZuServis.ObrisiUvoz(vremeKljuc);

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

    public IActionResult PregledUvozaZu(string vremeKljuc)
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

            var radnici = _context.ZuUvozi
                .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
                .OrderBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .ToList();

            if (radnici.Any())
            {
                var prvi = radnici.First();
                ViewBag.NazivFajla = $"{prvi.SifraZu}_{prvi.Nivo}.xlsx";
                ViewBag.Period = prvi.PeriodUvoza.ToString("MM/yyyy");
                return View(radnici);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Грешка приликом отварања прегледа ЗУ.");
        }

        return RedirectToAction(nameof(Index));
    }
}
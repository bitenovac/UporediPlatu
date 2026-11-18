using Microsoft.AspNetCore.Mvc;
using UporediPlate.Data;
using UporediPlate.Uvoz;

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
            if (!VremenskiKljuc.PokusajParsiranje(vremeKljuc, out var pocetakSekunde))
            {
                return RedirectToAction(nameof(Index));
            }

            var radnici = _context.ZuUvozi
                .IzUvoza(pocetakSekunde)
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
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class PortfolioController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PortfolioController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login", "Account");
        }

        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("Portfolio");

        if (!response.IsSuccessStatusCode)
        {
            return RedirectToAction("Login", "Account");
        }

        var portfolios = await response.Content.ReadFromJsonAsync<List<PortfolioSummary>>();

        ViewBag.UserEmail = email;
        return View(portfolios);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePortfolioForm model)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("Portfolio", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to create portfolio");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"Portfolio/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDetail>();
        return View(portfolio);
    }
    
    [HttpGet]
    public IActionResult AddHolding(Guid portfolioId)
    {
        ViewBag.PortfolioId = portfolioId;
        return View(new List<StockSearchResultView>());
    }

    [HttpGet]
    public async Task<IActionResult> SearchStocks(Guid portfolioId, string query)
    {
        ViewBag.PortfolioId = portfolioId;
        ViewBag.Query = query;

        if (string.IsNullOrWhiteSpace(query))
        {
            return View("AddHolding", new List<StockSearchResultView>());
        }

        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"Stocks/search?query={Uri.EscapeDataString(query)}");

        var results = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<List<StockSearchResultView>>()
            : new List<StockSearchResultView>();

        return View("AddHolding", results);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmAddHolding(Guid portfolioId, string symbol, string currency)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync(
            $"Portfolio/{portfolioId}/holdings",
            new { symbol, currency });

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Failed to add holding";
        }

        return RedirectToAction("Details", new { id = portfolioId });
    }
    
    [HttpGet]
    public async Task<IActionResult> AddTransaction(Guid portfolioId)
    {
        var client = _httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync($"Portfolio/{portfolioId}");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var portfolio = await response.Content.ReadFromJsonAsync<PortfolioDetail>();

        ViewBag.PortfolioId = portfolioId;
        ViewBag.Holdings = portfolio!.Holdings;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction(Guid portfolioId, CreateTransactionForm model)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var fixedModel = model with
        {
            ExecutedAt = DateTime.SpecifyKind(model.ExecutedAt, DateTimeKind.Utc)
        };

        var response = await client.PostAsJsonAsync($"Portfolio/{portfolioId}/transactions", fixedModel);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Failed to record transaction");

            var portfolioResponse = await client.GetAsync($"Portfolio/{portfolioId}");
            var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioDetail>();
            ViewBag.PortfolioId = portfolioId;
            ViewBag.Holdings = portfolio!.Holdings;

            return View(model);
        }

        return RedirectToAction("Details", new { id = portfolioId });
    }
}
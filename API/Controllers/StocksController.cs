using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StocksController : ControllerBase
{
    private readonly IStockSearchProvider _stockSearchProvider;

    public StocksController(IStockSearchProvider stockSearchProvider)
    {
        _stockSearchProvider = stockSearchProvider;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<StockSearchResult>>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<StockSearchResult>());
        }

        var results = await _stockSearchProvider.SearchAsync(query);
        return Ok(results);
    }
}
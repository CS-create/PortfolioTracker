using System.Security.Claims;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortfolioTracker.Domain.Entities;

namespace PortfolioTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;

    public PortfolioController(IPortfolioService portfolioService)
    {
        _portfolioService = portfolioService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub")
                   ?? throw new UnauthorizedAccessException("No user id in token"));

    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePortfolio(CreatePortfolioDto dto)
    {
        var id = await _portfolioService.CreatePortfolioAsync(CurrentUserId, dto);
        return Ok(id);
    }

    [HttpGet("{portfolioId}")]
    public async Task<ActionResult<PortfolioDto>> GetPortfolio(Guid portfolioId)
    {
        try
        {
            var result = await _portfolioService.GetPortfolioOverviewAsync(portfolioId, CurrentUserId);
            return Ok(result);

        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{portfolioId}/holdings")]
    public async Task<ActionResult<Guid>> AddHolding(Guid portfolioId, CreateHoldingDto dto)
    {
        try
        {
            var id = await _portfolioService.AddHoldingAsync(portfolioId, CurrentUserId, dto);
            return Ok(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    
    [HttpPost("{portfolioId}/transactions")]
    public async Task<ActionResult> AddTransaction(Guid portfolioId, CreateTransactionDto dto)
    {
        try
        {
            await _portfolioService.AddTransactionAsync(portfolioId, CurrentUserId, dto);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
    

}
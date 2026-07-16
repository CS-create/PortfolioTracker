using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var response = await client.PostAsJsonAsync("Auth/login", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        var authResult = await response.Content.ReadFromJsonAsync<AuthApiResponse>();

        HttpContext.Session.SetString("JwtToken", authResult!.Token);
        HttpContext.Session.SetString("UserEmail", authResult.Email);

        return RedirectToAction("Index", "Portfolio");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var client = _httpClientFactory.CreateClient("Api");

        var response = await client.PostAsJsonAsync("Auth/register", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Registration failed — email may already be in use");
            return View(model);
        }

        var authResult = await response.Content.ReadFromJsonAsync<AuthApiResponse>();

        HttpContext.Session.SetString("JwtToken", authResult!.Token);
        HttpContext.Session.SetString("UserEmail", authResult.Email);

        return RedirectToAction("Index", "Portfolio");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
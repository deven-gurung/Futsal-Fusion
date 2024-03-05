using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class CartController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
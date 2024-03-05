using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class OrderController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class FutsalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
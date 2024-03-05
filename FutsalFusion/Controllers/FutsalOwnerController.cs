using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class FutsalOwnerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
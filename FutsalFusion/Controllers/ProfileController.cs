using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ProfileController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
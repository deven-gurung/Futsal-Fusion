using FutsalFusion.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class CartController : BaseController<CartController>
{
    public IActionResult Index()
    {
        return View();
    }
}
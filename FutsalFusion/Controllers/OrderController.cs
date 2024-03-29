using FutsalFusion.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class OrderController : BaseController<OrderController>
{
    public IActionResult Index()
    {
        return View();
    }
}
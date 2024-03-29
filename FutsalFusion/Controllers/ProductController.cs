using FutsalFusion.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ProductController : BaseController<ProductController>
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Detail()
    {
        return View();
    }
}
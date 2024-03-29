using FutsalFusion.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers;

public class ProfileController : BaseController<ProfileController>
{
    public IActionResult Index()
    {
        return View();
    }
}
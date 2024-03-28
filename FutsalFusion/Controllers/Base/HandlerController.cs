using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Controllers.Base;

public class HandlerController : Controller
{
    public ViewResult UnauthorizedAccess()
    {
        return View("UnauthorizedAccess");
    }
    
    public ViewResult BadRequestView()
    {
        return View("BadRequest");
    }
    
    public ViewResult Forbidden()
    {
        return View("Forbidden");
    }
    
    public ViewResult NotFoundView()
    {
        return View("NotFound");
    }
}
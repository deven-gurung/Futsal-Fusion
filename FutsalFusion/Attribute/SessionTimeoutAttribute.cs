﻿using FutsalFusion.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ICDS.Attribute;

public class SessionTimeoutAttribute : ActionFilterAttribute, IActionFilter
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            filterContext.Result = new ContentResult { Content = "308", StatusCode = 308 };
        }
        else
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                action = "Login", 
                controller = "Account"
            }));
            
            base.OnActionExecuting(filterContext);
        }

    }
}
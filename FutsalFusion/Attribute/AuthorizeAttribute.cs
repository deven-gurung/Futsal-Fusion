using FutsalFusion.Application.DTOs.Account;
using FutsalFusion.Domain.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FutsalFusion.Attribute;

public class AuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var actionName = string.Empty;
        var controllerName = string.Empty;

        if (filterContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            controllerName = controllerActionDescriptor.ControllerName.ToLower();
            actionName = controllerActionDescriptor.ActionName.ToLower();
        }
        
        var user = filterContext.HttpContext.Session.GetComplexData<UserDetailDto?>("User");

        if (user == null)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "UnauthorizedAccess", controller = "Handler" }));

            base.OnActionExecuting(filterContext);
            
            return;
        }
        
        var indexPageUrl = $"{controllerName}/index";

        if (SharedControllers.Controllers.Contains(controllerName)) return;
        
        if (actionName != "index" && user.UserAccessDetails.Count > 0)
        {
            var menuRights = user.UserAccessDetails;
                
            if (actionName is "viewprofile" or "updateprofile")
            {
                indexPageUrl = $"{controllerName}/viewprofile";
            }
            
            if (menuRights.Any(x => x.MenuDetail.URL?.ToLower() == (indexPageUrl)))
            {
                var menuRight = menuRights.Where(x => x.MenuDetail.URL?.ToLower() == indexPageUrl).Select(
                    y => new
                    {
                        ShowMenuRight = y.ShowMenu
                    }).FirstOrDefault();

                filterContext.Result = actionName switch
                {
                    "index" when menuRight != null && !menuRight.ShowMenuRight => 
                        new RedirectToRouteResult(
                            new RouteValueDictionary(new
                            {
                                action = "UnAuthorizeAccess", 
                                controller = "Handler"
                            })),
                    _ => filterContext.Result
                };
            }
            else
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    action = "UnauthorizedAccess", 
                    controller = "Handler"
                }));
        }
        else
        {
            if (user.UserAccessDetails.Count > 0)
            {
                if (user.UserAccessDetails.All(x => x.MenuDetail.URL?.ToLower() != indexPageUrl))
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "UnauthorizedAccess", controller = "Handler" }));
                }
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "UnauthorizedAccess", controller = "Handler" }));
            }
        
            base.OnActionExecuting(filterContext);
        }
    }
}
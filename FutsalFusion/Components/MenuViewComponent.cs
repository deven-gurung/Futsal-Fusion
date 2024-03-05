using FutsalFusion.Application.DTOs.Account;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Attribute;
using Microsoft.AspNetCore.Mvc;

namespace FutsalFusion.Components;

public class MenuViewComponent: ViewComponent
{
    private readonly IMenuService _menuService;

    public MenuViewComponent(IMenuService menuService)
    {
        _menuService = menuService;
    }
    
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // var userDetail = HttpContext.Session.GetComplexData<UserDetailDto>("User");

        var roleId = "95205A34-05E0-46DB-BAA6-B1BCAEA10D63";
        
        var userMenu = _menuService.GetMenuByRole(new Guid(roleId));

        return View("Menu", userMenu);
    }
}
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
    
    public IViewComponentResult InvokeAsync()
    {
        var userDetail = HttpContext.Session.GetComplexData<UserDetailDto>("User");

        var userMenu = _menuService.GetMenuByRole(userDetail.RoleId);

        return View("Menu", userMenu);
    }
}
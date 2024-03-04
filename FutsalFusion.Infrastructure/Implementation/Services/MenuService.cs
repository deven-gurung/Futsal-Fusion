using FutsalFusion.Application.DTOs.Menu;
using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Application.Interfaces.Services;
using FutsalFusion.Domain.Entities;

namespace FutsalFusion.Infrastructure.Implementation.Services;

public class MenuService : IMenuService
{
    private readonly IGenericRepository _genericRepository;

    public List<MenuDto> GetMenuByRole(Guid roleId)
    {
        var roleRights = _genericRepository.Get<RoleRights>(x => 
            x.RoleId == roleId);

        var menus = _genericRepository.Get<Menu>(x => 
            roleRights.Select(y => y.MenuId).Contains(x.Id));

        var userRights = from menu in menus
            join roleRight in roleRights
                on menu.Id equals roleRight.MenuId
            select new Menu()
            {
                Id = menu.Id,    
                Name = menu.Name,
                SequenceNo = menu.SequenceNo,
                URL = menu.URL,
                ParentMenuId = menu.ParentMenuId,   
                IconClass = menu.IconClass,
                CreatedBy = menu.CreatedBy, 
            };

        var userMenu = userRights.Select(x => new MenuDto()
        {
            Id = x.Id,
            IconClass =x.IconClass,
            Name = x.Name,
            ParentMenuId = x.ParentMenuId,
            SequenceNumber = x.SequenceNo,
            URL = x.URL,
        }).ToList();

        return userMenu;
    }
}
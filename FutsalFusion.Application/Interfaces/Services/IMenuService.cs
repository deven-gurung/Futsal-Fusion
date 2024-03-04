using FutsalFusion.Application.DTOs.Menu;

namespace FutsalFusion.Application.Interfaces.Services;

public interface IMenuService
{
    List<MenuDto> GetMenuByRole(Guid roleId);
}
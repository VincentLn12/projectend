using ComSci.Application.Dtos.Menu;

namespace ComSci.Application.Abstractions;

public interface IMenuService
{
    Task<List<MenuItemDto>> GetPublicAsync(CancellationToken cancellationToken);

    Task<List<AdminMenuItemDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AdminMenuItemDto> GetByIdAsync(Guid menuItemId, CancellationToken cancellationToken);

    Task<AdminMenuItemDto> CreateAsync(SaveMenuItemRequest request, CancellationToken cancellationToken);

    Task<AdminMenuItemDto> UpdateAsync(Guid menuItemId, SaveMenuItemRequest request, CancellationToken cancellationToken);

    Task<AdminMenuItemDto> SetVisibleAsync(Guid menuItemId, bool isVisible, CancellationToken cancellationToken);

    Task DeleteAsync(Guid menuItemId, CancellationToken cancellationToken);

    Task ReorderAsync(ReorderMenuRequest request, CancellationToken cancellationToken);

    Task ResetToDefaultAsync(CancellationToken cancellationToken);
}

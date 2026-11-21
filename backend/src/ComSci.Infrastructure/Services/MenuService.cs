using ComSci.Application.Abstractions;
using ComSci.Application.Constants;
using ComSci.Application.Dtos.Menu;
using ComSci.Application.Exceptions;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using ComSci.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly AppDbContext _dbContext;

    public MenuService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<MenuItemDto>> GetPublicAsync(CancellationToken cancellationToken)
    {
        List<MenuItem> visibleItems = await _dbContext.MenuItems
            .AsNoTracking()
            .Where(item => item.IsVisible)
            .OrderBy(item => item.SortOrder)
            .ToListAsync(cancellationToken);

        List<MenuItemDto> menu = new List<MenuItemDto>();

        foreach (MenuItem topItem in visibleItems.Where(item => item.ParentId == null))
        {
            MenuItemDto dto = ToPublicDto(topItem);

            if (topItem.Kind == MenuLinkKind.Group)
            {
                foreach (MenuItem child in visibleItems.Where(item => item.ParentId == topItem.Id))
                {
                    dto.Children.Add(ToPublicDto(child));
                }

                // กลุ่มที่ไม่มีเมนูย่อยให้เห็นเลย ไม่แสดง เพราะจะเป็นปุ่มที่กดแล้วไม่มีอะไร
                if (dto.Children.Count == 0)
                {
                    continue;
                }
            }

            menu.Add(dto);
        }

        return menu;
    }

    public async Task<List<AdminMenuItemDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<AdminMenuItemDto> items = await _dbContext.MenuItems
            .AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .Select(item => new AdminMenuItemDto
            {
                Id = item.Id,
                Label = item.Label,
                Kind = item.Kind,
                Target = item.Target,
                ParentId = item.ParentId,
                SortOrder = item.SortOrder,
                IsVisible = item.IsVisible,
                OpenInNewTab = item.OpenInNewTab
            })
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<AdminMenuItemDto> GetByIdAsync(Guid menuItemId, CancellationToken cancellationToken)
    {
        MenuItem item = await LoadAsync(menuItemId, false, cancellationToken);
        return ToAdminDto(item);
    }

    public async Task<AdminMenuItemDto> CreateAsync(SaveMenuItemRequest request, CancellationToken cancellationToken)
    {
        int itemCount = await _dbContext.MenuItems.CountAsync(cancellationToken);
        if (itemCount >= MenuPages.MaxMenuItems)
        {
            throw new BadRequestException("The menu has too many items.");
        }

        string? target = await ValidateAsync(request, null, cancellationToken);

        MenuItem item = new MenuItem();
        ApplyRequest(item, request, target);
        item.SortOrder = await NextSortOrderAsync(request.ParentId, cancellationToken);

        _dbContext.MenuItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToAdminDto(item);
    }

    public async Task<AdminMenuItemDto> UpdateAsync(Guid menuItemId, SaveMenuItemRequest request, CancellationToken cancellationToken)
    {
        MenuItem item = await LoadAsync(menuItemId, true, cancellationToken);
        string? target = await ValidateAsync(request, item, cancellationToken);

        bool parentChanged = item.ParentId != request.ParentId;
        ApplyRequest(item, request, target);
        if (parentChanged)
        {
            item.SortOrder = await NextSortOrderAsync(request.ParentId, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToAdminDto(item);
    }

    public async Task<AdminMenuItemDto> SetVisibleAsync(Guid menuItemId, bool isVisible, CancellationToken cancellationToken)
    {
        MenuItem item = await LoadAsync(menuItemId, true, cancellationToken);
        item.IsVisible = isVisible;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToAdminDto(item);
    }

    public async Task DeleteAsync(Guid menuItemId, CancellationToken cancellationToken)
    {
        MenuItem item = await LoadAsync(menuItemId, true, cancellationToken);

        List<MenuItem> children = await _dbContext.MenuItems
            .Where(child => child.ParentId == menuItemId)
            .ToListAsync(cancellationToken);

        _dbContext.MenuItems.RemoveRange(children);
        _dbContext.MenuItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(ReorderMenuRequest request, CancellationToken cancellationToken)
    {
        List<MenuItem> siblings = await _dbContext.MenuItems
            .Where(item => item.ParentId == request.ParentId)
            .ToListAsync(cancellationToken);

        bool sameCount = request.OrderedIds.Count == siblings.Count;
        bool sameItems = request.OrderedIds.Distinct().Count() == siblings.Count
            && request.OrderedIds.All(id => siblings.Any(item => item.Id == id));
        if (!sameCount || !sameItems)
        {
            throw new BadRequestException("The list must contain every item of this menu level exactly once.");
        }

        for (int position = 0; position < request.OrderedIds.Count; position++)
        {
            MenuItem item = siblings.First(sibling => sibling.Id == request.OrderedIds[position]);
            item.SortOrder = position;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetToDefaultAsync(CancellationToken cancellationToken)
    {
        List<MenuItem> existing = await _dbContext.MenuItems.ToListAsync(cancellationToken);

        _dbContext.MenuItems.RemoveRange(existing);
        _dbContext.MenuItems.AddRange(MenuDefaults.Create());
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> ValidateAsync(SaveMenuItemRequest request, MenuItem? existing, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
        {
            throw new BadRequestException("Label is required.");
        }

        string? target = string.IsNullOrWhiteSpace(request.Target) ? null : request.Target.Trim();

        if (request.Kind == MenuLinkKind.Page)
        {
            bool isSitePage = target != null && MenuPages.All.Any(page => page.Path == target);
            if (!isSitePage)
            {
                throw new BadRequestException("Page link must be one of the site pages.");
            }
        }
        else if (request.Kind == MenuLinkKind.External)
        {
            if (!IsHttpAddress(target))
            {
                throw new BadRequestException("External link must be a full http or https address.");
            }
        }
        else
        {
            target = null;
            if (request.ParentId.HasValue)
            {
                throw new BadRequestException("A menu group cannot be placed inside another group.");
            }
        }

        if (request.ParentId.HasValue)
        {
            MenuItem? parent = await _dbContext.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == request.ParentId.Value, cancellationToken);
            bool parentIsTopLevelGroup = parent != null && parent.Kind == MenuLinkKind.Group && parent.ParentId == null;
            if (!parentIsTopLevelGroup || parent!.Id == existing?.Id)
            {
                throw new BadRequestException("Parent must be a top-level menu group.");
            }
        }

        if (existing != null && existing.Kind == MenuLinkKind.Group && request.Kind != MenuLinkKind.Group)
        {
            bool hasChildren = await _dbContext.MenuItems.AnyAsync(item => item.ParentId == existing.Id, cancellationToken);
            if (hasChildren)
            {
                throw new BadRequestException("Move or delete the items inside this group first.");
            }
        }

        return target;
    }

    private static bool IsHttpAddress(string? text)
    {
        if (text == null || !Uri.TryCreate(text, UriKind.Absolute, out Uri? address))
        {
            return false;
        }

        bool isHttp = address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps;
        return isHttp && !string.IsNullOrEmpty(address.Host);
    }

    private static void ApplyRequest(MenuItem item, SaveMenuItemRequest request, string? target)
    {
        item.Label = request.Label.Trim();
        item.Kind = request.Kind;
        item.Target = target;
        item.ParentId = request.ParentId;
        item.IsVisible = request.IsVisible;
        // กลุ่มไม่มีลิงก์ จึงไม่มีเรื่องเปิดแท็บใหม่
        item.OpenInNewTab = request.Kind != MenuLinkKind.Group && request.OpenInNewTab;
    }

    private async Task<int> NextSortOrderAsync(Guid? parentId, CancellationToken cancellationToken)
    {
        int? highest = await _dbContext.MenuItems
            .Where(item => item.ParentId == parentId)
            .MaxAsync(item => (int?)item.SortOrder, cancellationToken);

        return (highest ?? -1) + 1;
    }

    private async Task<MenuItem> LoadAsync(Guid menuItemId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<MenuItem> query = track ? _dbContext.MenuItems : _dbContext.MenuItems.AsNoTracking();

        MenuItem? item = await query.FirstOrDefaultAsync(menuItem => menuItem.Id == menuItemId, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException("Menu item not found.");
        }

        return item;
    }

    private static MenuItemDto ToPublicDto(MenuItem item)
    {
        MenuItemDto dto = new MenuItemDto();
        dto.Id = item.Id;
        dto.Label = item.Label;
        dto.Kind = item.Kind;
        dto.Target = item.Target;
        dto.OpenInNewTab = item.OpenInNewTab;
        return dto;
    }

    private static AdminMenuItemDto ToAdminDto(MenuItem item)
    {
        AdminMenuItemDto dto = new AdminMenuItemDto();
        dto.Id = item.Id;
        dto.Label = item.Label;
        dto.Kind = item.Kind;
        dto.Target = item.Target;
        dto.ParentId = item.ParentId;
        dto.SortOrder = item.SortOrder;
        dto.IsVisible = item.IsVisible;
        dto.OpenInNewTab = item.OpenInNewTab;
        return dto;
    }
}

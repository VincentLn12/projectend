using ComSci.Application.Constants;
using ComSci.Domain.Entities;
using ComSci.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ComSci.Infrastructure.Persistence;

public static class MenuDefaults
{
    public static List<MenuItem> Create()
    {
        List<MenuItem> items = new List<MenuItem>();
        int sortOrder = 0;

        foreach (MenuPageOption page in MenuPages.Defaults)
        {
            MenuItem item = new MenuItem();
            item.Label = page.Label;
            item.Kind = MenuLinkKind.Page;
            item.Target = page.Path;
            item.SortOrder = sortOrder;
            item.IsVisible = true;
            items.Add(item);
            sortOrder++;
        }

        return items;
    }

    public static async Task SeedIfEmptyAsync(AppDbContext dbContext)
    {
        bool hasItems = await dbContext.MenuItems.AnyAsync();
        if (hasItems)
        {
            return;
        }

        dbContext.MenuItems.AddRange(Create());
        await dbContext.SaveChangesAsync();
    }
}

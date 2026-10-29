using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Users;

namespace ComSci.Application.Abstractions;

public interface IUserAdminService
{
    Task<PagedResult<AdminUserListItemDto>> GetAllAsync(AdminUserQuery query, CancellationToken cancellationToken);

    Task<AdminUserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<AdminUserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task<AdminUserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken);

    Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken);

    Task SetLockedAsync(Guid userId, bool isLocked, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}

using ComSci.Application.Dtos.Common;
using ComSci.Domain.Enums;

namespace ComSci.Application.Dtos.Users;

public class AdminUserQuery : PageQuery
{
    public UserType? UserType { get; set; }
}

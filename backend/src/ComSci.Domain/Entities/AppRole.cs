using Microsoft.AspNetCore.Identity;

namespace ComSci.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public AppRole() { }
    public AppRole(string name) : base(name) { }
}

namespace ComSci.Application.Abstractions;

public interface ICurrentUser
{
    Guid? UserId { get; }
}

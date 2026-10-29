using ComSci.Application.Dtos.Common;
using ComSci.Application.Dtos.Profiles;

namespace ComSci.Application.Abstractions;

public interface IProfileService
{
    Task<MyProfileDto> GetMyProfileAsync(CancellationToken cancellationToken);

    Task<MyProfileDto> UpdateMyProfileAsync(UpdateMyProfileRequest request, CancellationToken cancellationToken);

    Task ChangeMyPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);

    Task<PagedResult<StudentListItemDto>> GetStudentsAsync(StudentQuery query, CancellationToken cancellationToken);

    Task<StudentDetailDto> GetStudentAsync(Guid userId, CancellationToken cancellationToken);
}

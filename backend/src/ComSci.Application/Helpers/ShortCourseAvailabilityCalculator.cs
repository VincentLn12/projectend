using ComSci.Domain.Enums;

namespace ComSci.Application.Helpers;

public static class ShortCourseAvailabilityCalculator
{
    public static DateTime GetRegistrationDeadline(DateTime? registrationCloseAt, DateTime startDate)
    {
        return registrationCloseAt ?? startDate;
    }

    public static ShortCourseAvailability Calculate(
        bool isRegistrationEnabled,
        DateTime? registrationOpenAt,
        DateTime? registrationCloseAt,
        DateTime startDate,
        int maxParticipants,
        int registeredCount,
        DateTime now)
    {
        if (!isRegistrationEnabled)
        {
            return ShortCourseAvailability.Closed;
        }

        if (registrationOpenAt.HasValue && registrationOpenAt.Value > now)
        {
            return ShortCourseAvailability.NotOpenYet;
        }

        if (now >= GetRegistrationDeadline(registrationCloseAt, startDate))
        {
            return ShortCourseAvailability.Closed;
        }

        if (maxParticipants > 0 && registeredCount >= maxParticipants)
        {
            return ShortCourseAvailability.Full;
        }

        return ShortCourseAvailability.Open;
    }
}

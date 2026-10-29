using iMasjid.Application.Interfaces.Services;

namespace iMasjid.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Praxis.Backend.Application.Helper;

/// <summary>
/// Persists UTC as a timezone-naive MariaDB DATETIME and restores timezone
/// awareness (Kind = Utc) on reads. Rejects non-UTC values on write.
/// </summary>
public static class UtcDateTimeConversion
{
    public static readonly ValueConverter<DateTime, DateTime> Required = new(
        toProvider => ToStorage(toProvider),
        fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc));

    public static readonly ValueConverter<DateTime?, DateTime?> Nullable = new(
        toProvider => toProvider.HasValue ? ToStorage(toProvider.Value) : null,
        fromProvider => fromProvider.HasValue
            ? DateTime.SpecifyKind(fromProvider.Value, DateTimeKind.Utc)
            : null);

    /// <summary>
    /// For database-computed columns (e.g. CURRENT_TIMESTAMP defaults): the CLR
    /// property only ever holds a placeholder value that EF Core omits from the
    /// generated SQL, so writes must not throw on a non-UTC placeholder.
    /// </summary>
    public static readonly ValueConverter<DateTime, DateTime> Computed = new(
        toProvider => DateTime.SpecifyKind(toProvider, DateTimeKind.Unspecified),
        fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc));

    private static DateTime ToStorage(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("UtcDateTimeConversion requires a UTC value.", nameof(value));
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
    }
}

using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Praxis.Backend.Application.DB;

/// <summary>Forces the MariaDB session time zone to UTC on every new connection.</summary>
public sealed class UtcSessionTimeZoneInterceptor : DbConnectionInterceptor
{
    private const string SetUtcSessionTimeZone = "SET time_zone = '+00:00';";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var command = connection.CreateCommand();
        command.CommandText = SetUtcSessionTimeZone;
        command.ExecuteNonQuery();
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = SetUtcSessionTimeZone;
        await command.ExecuteNonQueryAsync(cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }
}

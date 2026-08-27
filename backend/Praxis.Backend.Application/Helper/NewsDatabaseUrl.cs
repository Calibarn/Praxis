namespace Praxis.Backend.Application.Helper;

/// <summary>
/// Parses the `mysql+asyncmy://user:pass@host:port/db` connection URL format
/// (kept for compose.yaml compatibility) into a MySqlConnector connection string.
/// </summary>
public static class NewsDatabaseUrl
{
    private const string ExpectedScheme = "mysql+asyncmy";

    public static string ToConnectionString(string databaseUrl)
    {
        if (!databaseUrl.StartsWith($"{ExpectedScheme}://", StringComparison.Ordinal))
        {
            throw new ArgumentException("News Service requires a mysql+asyncmy database URL", nameof(databaseUrl));
        }

        var uri = new Uri("mysql://" + databaseUrl[(ExpectedScheme.Length + 3)..]);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port == -1 ? 3306 : uri.Port;

        return $"Server={uri.Host};Port={port};Database={database};User={user};Password={password};" +
               "AllowPublicKeyRetrieval=true;";
    }
}

using System.Data.Common;

namespace Praxis.Backend.Tests.Api;

/// <summary>Stands in for a MySqlConnector/ADO.NET failure without needing a real database.</summary>
public sealed class FakeDbException(string message) : DbException(message);

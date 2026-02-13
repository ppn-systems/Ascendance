// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Nalix.Framework.Configuration.Binding;

namespace Ascendance.Hosting.Configurations;

/// <summary>
/// Configuration options for database connections.
/// </summary>
public sealed class DatabaseOptions : ConfigurationLoader
{
    /// <summary>
    /// Maximum pool size for database connections.
    /// </summary>
    public System.Int32 MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Command timeout in seconds.
    /// </summary>
    public System.Int32 CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Enable query logging for debugging.
    /// </summary>
    public System.Boolean EnableLogging { get; set; } = false;

    /// <summary>
    /// Redis connection string.
    /// </summary>
    public System.String RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// PostgreSQL connection string.
    /// </summary>
    public System.String PostgreSqlConnectionString { get; set; } = System.String.Empty;
}
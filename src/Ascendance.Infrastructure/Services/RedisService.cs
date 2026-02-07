// Copyright (c) 2026 Ascendance Team. All rights reserved.

using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Ascendance.Infrastructure.Services;

/// <summary>
/// Service for Redis operations (session management, caching, rate limiting).
/// </summary>
public sealed class RedisService : System.IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisService"/> class.
    /// </summary>
    /// <param name="connectionString">Redis connection string.</param>
    public RedisService(System.String connectionString)
    {
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
    }

    #region Session Management

    /// <summary>
    /// Stores a session token with associated user data.
    /// </summary>
    /// <param name="token">Session token.</param>
    /// <param name="username">Username.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <param name="expiry">Token expiration time (default: 1 hour).</param>
    public async System.Threading.Tasks.Task<System.Boolean> StoreSessionTokenAsync(
        System.String token,
        System.String username,
        System.String ipAddress,
        System.TimeSpan? expiry = null)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(token);
        System.ArgumentException.ThrowIfNullOrWhiteSpace(username);

        var sessionData = new
        {
            Username = username,
            IpAddress = ipAddress,
            CreatedAt = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        System.String key = $"session:{token}";
        System.String value = JsonSerializer.Serialize(
            sessionData,
            RedisServiceJsonContext.Default.Object
        );

        return await _db.StringSetAsync(
            key,
            value,
            expiry ?? System.TimeSpan.FromHours(1)
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates a session token and returns associated username.
    /// </summary>
    /// <param name="token">Session token to validate.</param>
    /// <returns>Username if valid, null otherwise.</returns>
    public async System.Threading.Tasks.Task<System.String> ValidateSessionTokenAsync(
        System.String token)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(token);

        System.String key = $"session:{token}";
        RedisValue value = await _db.StringGetAsync(key).ConfigureAwait(false);

        if (!value.HasValue)
        {
            return null;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(value.ToString());
            return doc.RootElement.GetProperty("Username").GetString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Removes a session token (logout).
    /// </summary>
    /// <param name="token">Session token to remove.</param>
    public async System.Threading.Tasks.Task<System.Boolean> RemoveSessionTokenAsync(
        System.String token)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return await _db.KeyDeleteAsync($"session:{token}").ConfigureAwait(false);
    }

    #endregion

    #region Rate Limiting

    /// <summary>
    /// Checks if an IP address is rate limited.
    /// </summary>
    /// <param name="ipAddress">IP address to check.</param>
    /// <param name="maxAttempts">Maximum attempts allowed.</param>
    /// <param name="window">Time window for rate limiting.</param>
    /// <returns>True if rate limited, false otherwise.</returns>
    public async System.Threading.Tasks.Task<System.Boolean> IsRateLimitedAsync(
        System.String ipAddress,
        System.Int32 maxAttempts,
        System.TimeSpan window)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);

        System.String key = $"ratelimit:{ipAddress}";
        RedisValue value = await _db.StringGetAsync(key).ConfigureAwait(false);

        return value.HasValue && System.Int32.TryParse(value.ToString(), out System.Int32 attempts)
            && attempts >= maxAttempts;
    }

    /// <summary>
    /// Increments failed login attempts for an IP address.
    /// </summary>
    /// <param name="ipAddress">IP address to track.</param>
    /// <param name="window">Time window for tracking.</param>
    public async System.Threading.Tasks.Task IncrementFailedAttemptsAsync(
        System.String ipAddress,
        System.TimeSpan window)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);

        System.String key = $"ratelimit:{ipAddress}";
        System.Int64 count = await _db.StringIncrementAsync(key).ConfigureAwait(false);

        // Set expiration on first attempt
        if (count == 1)
        {
            await _db.KeyExpireAsync(key, window).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Resets failed attempts for an IP address (on successful login).
    /// </summary>
    /// <param name="ipAddress">IP address to reset.</param>
    public async System.Threading.Tasks.Task ResetFailedAttemptsAsync(
        System.String ipAddress)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        await _db.KeyDeleteAsync($"ratelimit:{ipAddress}").ConfigureAwait(false);
    }

    #endregion

    #region Player Online Status

    /// <summary>
    /// Marks a player as online.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="serverId">Server ID (for multi-server setup).</param>
    public async System.Threading.Tasks.Task SetPlayerOnlineAsync(
        System.String username,
        System.String serverId = "default")
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(username);

        await _db.HashSetAsync(
            "players:online",
            username,
            serverId
        ).ConfigureAwait(false);

        // Set last activity timestamp
        await _db.StringSetAsync(
            $"player:{username}:lastactivity",
            System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            System.TimeSpan.FromHours(24)
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Marks a player as offline.
    /// </summary>
    /// <param name="username">Username.</param>
    public async System.Threading.Tasks.Task SetPlayerOfflineAsync(System.String username)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(username);
        await _db.HashDeleteAsync("players:online", username).ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if a player is currently online.
    /// </summary>
    /// <param name="username">Username to check.</param>
    /// <returns>True if online, false otherwise.</returns>
    public async System.Threading.Tasks.Task<System.Boolean> IsPlayerOnlineAsync(
        System.String username)
    {
        System.ArgumentException.ThrowIfNullOrWhiteSpace(username);
        return await _db.HashExistsAsync("players:online", username).ConfigureAwait(false);
    }

    #endregion

    #region Pub/Sub (For multi-server communication)

    /// <summary>
    /// Publishes a message to a channel.
    /// </summary>
    /// <param name="channel">Channel name.</param>
    /// <param name="message">Message to publish.</param>
    [System.Obsolete]
    public async System.Threading.Tasks.Task PublishAsync(
        System.String channel,
        System.String message)
    {
        ISubscriber sub = _redis.GetSubscriber();
        await sub.PublishAsync(channel, message).ConfigureAwait(false);
    }

    /// <summary>
    /// Subscribes to a channel.
    /// </summary>
    /// <param name="channel">Channel name.</param>
    /// <param name="handler">Message handler.</param>
    [System.Obsolete]
    public async System.Threading.Tasks.Task SubscribeAsync(
        System.String channel,
        System.Action<System.String> handler)
    {
        ISubscriber sub = _redis.GetSubscriber();
        await sub.SubscribeAsync(channel, (_, message) =>
        {
            handler(message.ToString());
        }).ConfigureAwait(false);
    }

    #endregion

    /// <summary>
    /// Disposes Redis connection.
    /// </summary>
    public void Dispose() => _redis?.Dispose();
}

// Add this context class at the end of the file (or in a separate file if preferred)
[System.Text.Json.Serialization.JsonSerializable(typeof(System.Object))]
internal partial class RedisServiceJsonContext : System.Text.Json.Serialization.JsonSerializerContext
{
    protected override JsonSerializerOptions GeneratedSerializerOptions => throw new System.NotImplementedException();

    public override JsonTypeInfo GetTypeInfo(System.Type type) => throw new System.NotImplementedException();
}
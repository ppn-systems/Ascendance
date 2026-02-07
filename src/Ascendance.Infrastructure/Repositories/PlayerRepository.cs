// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Ascendance.Infrastructure.Data;
using Ascendance.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascendance.Infrastructure.Repositories;

/// <summary>
/// Repository for player account operations.
/// </summary>
public sealed class PlayerRepository
{
    private readonly GameDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerRepository"/> class.
    /// </summary>
    public PlayerRepository(GameDbContext context) => _context = context;

    /// <summary>
    /// Gets a player account by username.
    /// </summary>
    /// <param name="username">Username to search for.</param>
    /// <returns>Player account if found, null otherwise.</returns>
    public async System.Threading.Tasks.Task<PlayerAccount> GetByUsernameAsync(
        System.String username)
    {
        return await _context.PlayerAccounts
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Username == username)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new player account.
    /// </summary>
    /// <param name="account">Player account to create.</param>
    /// <returns>Created player account with generated ID.</returns>
    public async System.Threading.Tasks.Task<PlayerAccount> CreateAsync(
        PlayerAccount account)
    {
        _ = await _context.PlayerAccounts.AddAsync(account).ConfigureAwait(false);
        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
        return account;
    }

    /// <summary>
    /// Updates player's last login timestamp.
    /// </summary>
    /// <param name="playerId">Player ID.</param>
    public async System.Threading.Tasks.Task UpdateLastLoginAsync(System.Int64 playerId)
    {
        PlayerAccount player = await _context.PlayerAccounts
            .FindAsync(playerId)
            .ConfigureAwait(false);

        if (player is not null)
        {
            player.LastLoginAt = System.DateTime.UtcNow;
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Validates player credentials.
    /// </summary>
    /// <returns>True if credentials are valid, false otherwise.</returns>
    //public async System.Threading.Tasks.Task<System.Boolean> ValidateCredentialsAsync(
    //    System.String username,
    //    System.String passwordHash)
    //{
    //    PlayerAccount? player = await GetByUsernameAsync(username).ConfigureAwait(false);

    //    if (player is null)
    //    {
    //        return false;
    //    }

    //    // Check account status
    //    if (player.Status != AccountStatus.Active)
    //    {
    //        return false;
    //    }

    //    // Check if ban has expired
    //    if (player.BanExpiresAt.HasValue && player.BanExpiresAt > System.DateTime.UtcNow)
    //    {
    //        return false;
    //    }

    //    return Pbkdf2.Verify(passwordHash, player.PasswordHash);
    //}
}
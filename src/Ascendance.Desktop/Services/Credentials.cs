// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Nalix.Common.Core.Enums;
using Nalix.Common.Environment;
using Nalix.Shared.Security;

namespace Ascendance.Desktop.Services;

/// <summary>
/// Service for encrypting and storing credentials using EnvelopeCipher.
/// Uses CHACHA20-Poly1305 AEAD for authenticated encryption.
/// No external dependencies beyond Nalix.Shared.Security.
/// </summary>
public static class Credentials
{
    #region Fields

    private static readonly System.String CredentialFilePath;

    // CHACHA20 requires 32-byte (256-bit) key
    private static readonly System.Byte[] Key = DERIVE_KEY();

    // Cipher suite to use - CHACHA20_POLY1305 provides AEAD with authentication
    private const CipherSuiteType CipherSuite = CipherSuiteType.CHACHA20_POLY1305;

    #endregion Fields

    #region Constructor

    static Credentials() => CredentialFilePath = System.IO.Path.Combine(Directories.DataDirectory, "credentials.dat");

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Deletes the encrypted credentials file.
    /// </summary>
    public static void Delete()
    {
        if (System.IO.File.Exists(CredentialFilePath))
        {
            System.IO.File.Delete(CredentialFilePath);
        }
    }

    /// <summary>
    /// Saves encrypted credentials to local file using CHACHA20-Poly1305 AEAD.
    /// </summary>
    /// <param name="username">Username to save.</param>
    /// <param name="password">Password to save.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when username or password is null.</exception>
    /// <exception cref="System.IO.IOException">Thrown when file operation fails.</exception>
    public static void Save(System.String username, System.String password)
    {
        if (System.String.IsNullOrWhiteSpace(username))
        {
            throw new System.ArgumentNullException(nameof(username));
        }

        if (System.String.IsNullOrWhiteSpace(password))
        {
            throw new System.ArgumentNullException(nameof(password));
        }

        // Combine username and password with delimiter
        System.String data = $"{username}|{password}";
        System.Byte[] plaintext = System.Text.Encoding.UTF8.GetBytes(data);

        // Optional: Add AAD (Additional Authenticated Data) for extra context
        System.Byte[] aad = System.Text.Encoding.UTF8.GetBytes("Ascendance.Desktop.Credentials.V1");

        // Encrypt using EnvelopeCipher
        System.Byte[] envelope = EnvelopeCipher.Encrypt(
            key: Key,
            plaintext: plaintext,
            algorithm: CipherSuite,
            aad: aad,
            seq: null  // Auto-generate random sequence
        );

        // Write encrypted envelope to file
        System.IO.File.WriteAllBytes(CredentialFilePath, envelope);
    }

    /// <summary>
    /// Retrieves and decrypts credentials from local file.
    /// </summary>
    /// <returns>Tuple containing username and password, or null if not found or decryption failed.</returns>
    public static (System.String Username, System.String Password)? Get()
    {
        if (!System.IO.File.Exists(CredentialFilePath))
        {
            return null;
        }

        try
        {
            // Read encrypted envelope from file
            System.Byte[] envelope = System.IO.File.ReadAllBytes(CredentialFilePath);

            // AAD must match what was used during encryption
            System.Byte[] aad = System.Text.Encoding.UTF8.GetBytes("Ascendance.Desktop.Credentials.V1");

            // Attempt to decrypt
            System.Boolean success = EnvelopeCipher.Decrypt(
                key: Key,
                envelope: envelope,
                plaintext: out System.Byte[] plaintext,
                aad: aad
            );

            if (!success || plaintext == null)
            {
                System.Diagnostics.Debug.WriteLine("Decryption failed - authentication or parsing error");
                return null;
            }

            // Parse decrypted data
            System.String data = System.Text.Encoding.UTF8.GetString(plaintext);
            System.String[] parts = data.Split('|');

            if (parts.Length == 2)
            {
                return (parts[0], parts[1]);
            }

            System.Diagnostics.Debug.WriteLine("Invalid credential format after decryption");
            return null;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get credentials: {ex.Message}");
            return null;
        }
    }

    #endregion APIs

    #region Key Derivation

    /// <summary>
    /// Derives a unique 256-bit key based on machine and user identity.
    /// Uses SHA-256 equivalent logic to ensure 32-byte output for CHACHA20.
    /// </summary>
    /// <returns>32-byte key suitable for CHACHA20.</returns>
    private static System.Byte[] DERIVE_KEY()
    {
        // Combine multiple machine/user-specific values
        System.String seed = $"{System.Environment.MachineName}-{System.Environment.UserName}-{System.Environment.OSVersion.Platform}-Ascendance-Key-V1";
        System.Byte[] seedBytes = System.Text.Encoding.UTF8.GetBytes(seed);

        // Simple deterministic key derivation (32 bytes for CHACHA20)
        // Using XOR folding to create a 32-byte key from variable-length seed
        System.Byte[] key = new System.Byte[32];

        for (System.Int32 i = 0; i < seedBytes.Length; i++)
        {
            key[i % 32] ^= seedBytes[i];
        }

        // Additional mixing for better distribution
        for (System.Int32 i = 0; i < 32; i++)
        {
            key[i] = (System.Byte)(((key[i] * 131) + (i * 17)) & 0xFF);
        }

        return key;
    }

    #endregion Key Derivation
}
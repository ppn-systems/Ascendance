using SFML.Graphics;

namespace Ascendance.Rendering.Loaders;

/// <summary>
/// Font management class. Handles loading/unloading of unmanaged font resources.
/// </summary>
/// <remarks>
/// Creates a new instance of the FontLoader class.
/// </remarks>
/// <param name="assetRoot">Optional root path of the managed asset folder</param>
public sealed class FontLoader(System.String assetRoot = "") : AssetLoader<Font>(AvailableFormats, assetRoot)
{
    /// <summary>
    /// List of supported file endings for this FontLoader
    /// </summary>
    public static readonly System.Collections.Generic.IEnumerable<System.String> AvailableFormats = [".ttf", ".cff", ".fnt", ".ttf", ".otf", ".eot"];

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override Font CreateInstanceFromRawData(System.Byte[] rawData)
    {
        if (rawData == null || rawData.Length == 0)
        {
            throw new System.ArgumentException("Raw data is null or empty.", nameof(rawData));
        }

        using System.IO.MemoryStream ms = new(rawData, writable: false);
        return new Font(ms);
    }

    /// <inheritdoc/>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    protected override Font CreateInstanceFromPath(System.String path)
        => System.String.IsNullOrWhiteSpace(path) ? throw new System.ArgumentException("Path is null or empty.", nameof(path)) : new Font(path);
}

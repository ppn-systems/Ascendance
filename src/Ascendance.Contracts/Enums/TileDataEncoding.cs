// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Contracts.Enums;

/// <summary>
/// Specifies the encoding format used for tile data in TMX files.
/// </summary>
/// <remarks>
/// Tiled Map Editor supports multiple encoding formats for tile layer data.
/// Different formats offer trade-offs between file size, readability, and parsing performance.
/// </remarks>
public enum TileDataEncoding : System.Byte
{
    /// <summary>
    /// XML tile elements format (default). Each tile is represented as a &lt;tile gid="..."&gt; element.
    /// </summary>
    /// <remarks>
    /// Most verbose format but easiest to read and debug. No additional parsing required beyond XML.
    /// </remarks>
    Xml = 0,

    /// <summary>
    /// Comma-separated values format. All tile GIDs are stored as comma-separated integers.
    /// </summary>
    /// <remarks>
    /// Compact and human-readable format. Faster to parse than XML elements.
    /// This is the recommended format for most use cases.
    /// </remarks>
    Csv = 1,

    /// <summary>
    /// Base64-encoded binary data (not currently supported).
    /// </summary>
    /// <remarks>
    /// Most compact format but requires base64 decoding. Future implementation.
    /// </remarks>
    Base64 = 2,

    /// <summary>
    /// Base64-encoded and compressed data using gzip or zlib (not currently supported).
    /// </summary>
    /// <remarks>
    /// Maximum compression for large maps. Requires decompression step. Future implementation.
    /// </remarks>
    Base64Compressed = 3
}
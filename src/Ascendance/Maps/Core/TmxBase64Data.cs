// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Tiled.Core;

/// <summary>
/// Decodes base64-encoded tile layer data and wraps optional compression (gzip/zlib).
/// </summary>
public class TmxBase64Data
{
    /// <summary>
    /// Decoded data stream. Caller should dispose when finished.
    /// </summary>
    public System.IO.Stream Data { get; }

    /// <summary>
    /// Parse &lt;data&gt; element containing base64 (and optional compression).
    /// </summary>
    /// <param name="xData">The &lt;data&gt; element or a &lt;chunk&gt; element inside &lt;data&gt;.</param>
    public TmxBase64Data(System.Xml.Linq.XElement xData)
    {
        System.ArgumentNullException.ThrowIfNull(xData);

        System.String encoding = (System.String)(xData.Attribute("encoding") ?? xData.Parent?.Attribute("encoding")) ?? System.String.Empty;
        System.String compression = (System.String)(xData.Attribute("compression") ?? xData.Parent?.Attribute("compression")) ?? System.String.Empty;

        if (!System.String.Equals(encoding, "base64", System.StringComparison.OrdinalIgnoreCase))
        {
            throw new System.NotSupportedException("TmxBase64Data: Only Base64-encoded data is supported.");
        }

        // Be tolerant of whitespace/newlines in base64 text
        System.String base64Text = (xData.Value ?? System.String.Empty).Trim();
        System.Byte[] rawData = System.Convert.FromBase64String(base64Text);

        // Default memory stream (not writable)
        System.IO.Stream stream = new System.IO.MemoryStream(rawData, writable: false);

        if (System.String.Equals(compression, "gzip", System.StringComparison.OrdinalIgnoreCase))
        {
            stream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
        }
        else if (System.String.Equals(compression, "zlib", System.StringComparison.OrdinalIgnoreCase))
        {
            // zlib = zlib header (2 bytes) + deflate data + adler32 (4 bytes)
            // Skip first two bytes and last four bytes as a pragmatic handling.
            if (rawData.Length <= 6)
            {
                throw new System.IO.InvalidDataException("TmxBase64Data: zlib-compressed data is too short.");
            }

            System.Int32 bodyLength = rawData.Length - 6;
            System.Byte[] bodyData = new System.Byte[bodyLength];
            System.Array.Copy(rawData, 2, bodyData, 0, bodyLength);
            System.IO.MemoryStream bodyStream = new(bodyData, writable: false);
            stream = new System.IO.Compression.DeflateStream(bodyStream, System.IO.Compression.CompressionMode.Decompress);
        }
        else if (!System.String.IsNullOrEmpty(compression))
        {
            throw new System.NotSupportedException("TmxBase64Data: Unknown compression.");
        }

        Data = stream;
    }
}
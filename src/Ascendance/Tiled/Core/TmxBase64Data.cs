using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace Ascendance.Tiled.Core;

/// <summary>
/// Decodes base64-encoded tile layer data and wraps optional compression (gzip/zlib).
/// </summary>
public class TmxBase64Data
{
    /// <summary>
    /// Decoded data stream. Caller should dispose when finished.
    /// </summary>
    public System.IO.Stream Data { get; private set; }

    /// <summary>
    /// Parse &lt;data&gt; element containing base64 (and optional compression).
    /// </summary>
    /// <param name="xData">The &lt;data&gt; element or a &lt;chunk&gt; element inside &lt;data&gt;.</param>
    public TmxBase64Data(XElement xData)
    {
        System.ArgumentNullException.ThrowIfNull(xData);

        System.String encoding = (System.String)(xData.Attribute("encoding") ?? xData.Parent?.Attribute("encoding")) ?? System.String.Empty;
        System.String compression = (System.String)(xData.Attribute("compression") ?? xData.Parent?.Attribute("compression")) ?? System.String.Empty;

        if (!System.String.Equals(encoding, "base64", System.StringComparison.OrdinalIgnoreCase))
        {
            throw new System.NotSupportedException("TmxBase64Data: Only Base64-encoded data is supported.");
        }

        // Be tolerant of whitespace/newlines in base64 text
        var base64Text = (xData.Value ?? System.String.Empty).Trim();
        var rawData = System.Convert.FromBase64String(base64Text);

        // Default memory stream (not writable)
        System.IO.Stream stream = new System.IO.MemoryStream(rawData, writable: false);

        if (System.String.Equals(compression, "gzip", System.StringComparison.OrdinalIgnoreCase))
        {
            stream = new GZipStream(stream, CompressionMode.Decompress);
        }
        else if (System.String.Equals(compression, "zlib", System.StringComparison.OrdinalIgnoreCase))
        {
            // zlib = zlib header (2 bytes) + deflate data + adler32 (4 bytes)
            // Skip first two bytes and last four bytes as a pragmatic handling.
            if (rawData.Length <= 6)
            {
                throw new InvalidDataException("TmxBase64Data: zlib-compressed data is too short.");
            }

            var bodyLength = rawData.Length - 6;
            var bodyData = new System.Byte[bodyLength];
            System.Array.Copy(rawData, 2, bodyData, 0, bodyLength);
            var bodyStream = new System.IO.MemoryStream(bodyData, writable: false);
            stream = new DeflateStream(bodyStream, CompressionMode.Decompress);
        }
        else if (!System.String.IsNullOrEmpty(compression))
        {
            throw new System.NotSupportedException("TmxBase64Data: Unknown compression.");
        }

        Data = stream;
    }
}
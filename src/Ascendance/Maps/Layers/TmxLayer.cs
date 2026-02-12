// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Abstractions;
using Ascendance.Maps.Collections;
using Ascendance.Maps.Core;
using System.Linq;

namespace Ascendance.Maps.Layers;

/// <summary>
/// Represents a tile layer parsed from a Tiled TMX file.
/// </summary>
public class TmxLayer : ITmxLayer
{
    #region Properties

    /// <summary>
    /// Layer name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Layer opacity (0.0 - 1.0). Defaults to 1.0 when attribute is missing.
    /// </summary>
    public System.Double Opacity { get; }

    /// <summary>
    /// Whether the layer is visible. Defaults to true when attribute is missing.
    /// </summary>
    public System.Boolean Visible { get; }

    /// <summary>
    /// Horizontal offset in pixels. Nullable when the attribute was not present.
    /// </summary>
    public System.Double? OffsetX { get; }

    /// <summary>
    /// Vertical offset in pixels. Nullable when the attribute was not present.
    /// </summary>
    public System.Double? OffsetY { get; }

    /// <summary>
    /// Layer tint color (if any).
    /// </summary>
    public TmxColor Tint { get; }

    /// <summary>
    /// Collection of tiles in this layer. Items are stored in reading order.
    /// </summary>
    public System.Collections.ObjectModel.Collection<TmxLayerTile> Tiles { get; }

    /// <summary>
    /// Custom properties attached to this layer.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="TmxLayer"/> by parsing the provided &lt;layer&gt; element.
    /// </summary>
    /// <param name="xLayer">The XML element representing the &lt;layer&gt;.</param>
    /// <param name="width">Map width in tiles (used when layer data is not chunked).</param>
    /// <param name="height">Map height in tiles (used when layer data is not chunked).</param>
    /// <exception cref="System.ArgumentNullException">If <paramref name="xLayer"/> is null.</exception>
    /// <exception cref="System.ArgumentOutOfRangeException">If <paramref name="width"/> or <paramref name="height"/> is negative.</exception>
    /// <exception cref="System.InvalidOperationException">If required &lt;data&gt; child is missing.</exception>
    public TmxLayer(System.Xml.Linq.XElement xLayer, System.Int32 width, System.Int32 height)
    {
        System.ArgumentNullException.ThrowIfNull(xLayer);
        System.ArgumentOutOfRangeException.ThrowIfNegative(width);
        System.ArgumentOutOfRangeException.ThrowIfNegative(height);

        Name = (System.String)xLayer.Attribute("name") ?? System.String.Empty;

        Opacity = (System.Double?)xLayer.Attribute("opacity") ?? 1.0;
        Visible = (System.Boolean?)xLayer.Attribute("visible") ?? true;

        // Keep nullable semantics so caller can detect "attribute absent"
        OffsetX = (System.Double?)xLayer.Attribute("offsetx");
        OffsetY = (System.Double?)xLayer.Attribute("offsety");

        Tint = new TmxColor(xLayer.Attribute("tint"));

        System.Xml.Linq.XElement xData = xLayer.Element("data") ?? throw new System.InvalidOperationException("Layer is missing required <data> element.");
        System.String encoding = (System.String)xData.Attribute("encoding");

        // Determine expected capacity to avoid reallocations (sum of chunks or full layer size).
        System.Int32 expectedCount = 0;
        System.Collections.Generic.List<System.Xml.Linq.XElement> xChunks = [.. xData.Elements("chunk")];
        if (xChunks.Count != 0)
        {
            foreach (System.Xml.Linq.XElement c in xChunks)
            {
                System.Int32 cw = (System.Int32?)c.Attribute("width") ?? 0;
                System.Int32 ch = (System.Int32?)c.Attribute("height") ?? 0;
                expectedCount += System.Math.Max(0, cw * ch);
            }
        }
        else
        {
            expectedCount = System.Math.Max(0, width * height);
        }

        // Use a List<T> with capacity then wrap into a Collection<T> so external API remains Collection<T>.
        System.Collections.Generic.List<TmxLayerTile> backingList = new(expectedCount);
        Tiles = new System.Collections.ObjectModel.Collection<TmxLayerTile>(backingList);

        if (xChunks.Count != 0)
        {
            foreach (System.Xml.Linq.XElement xChunk in xChunks)
            {
                System.Int32 chunkX = (System.Int32?)xChunk.Attribute("x") ?? 0;
                System.Int32 chunkY = (System.Int32?)xChunk.Attribute("y") ?? 0;
                System.Int32 chunkWidth = (System.Int32?)xChunk.Attribute("width") ?? 0;
                System.Int32 chunkHeight = (System.Int32?)xChunk.Attribute("height") ?? 0;

                READ_CHUNK(chunkWidth, chunkHeight, chunkX, chunkY, encoding, xChunk);
            }
        }
        else
        {
            READ_CHUNK(width, height, 0, 0, encoding, xData);
        }

        Properties = new PropertyDict(xLayer.Element("properties"));
    }

    #endregion Constructor

    #region Private Methods

    /// <summary>
    /// Read one chunk (or the entire layer when not chunked) and append tiles to <see cref="Tiles"/>.
    /// </summary>
    /// <param name="width">Chunk width in tiles.</param>
    /// <param name="height">Chunk height in tiles.</param>
    /// <param name="startX">Chunk X offset in tiles.</param>
    /// <param name="startY">Chunk Y offset in tiles.</param>
    /// <param name="encoding">Data encoding (e.g. "base64", "csv") or null for XML tiles.</param>
    /// <param name="xData">The &lt;data&gt; element or &lt;chunk&gt; element containing tile data.</param>
    private void READ_CHUNK(System.Int32 width, System.Int32 height, System.Int32 startX, System.Int32 startY, System.String encoding, System.Xml.Linq.XElement xData)
    {
        System.ArgumentNullException.ThrowIfNull(xData);
        System.ArgumentOutOfRangeException.ThrowIfNegative(width);
        System.ArgumentOutOfRangeException.ThrowIfNegative(height);

        if (System.String.Equals(encoding, "base64", System.StringComparison.OrdinalIgnoreCase))
        {
            // TmxBase64Data is expected to provide a stream positioned at the start of decoded data
            TmxBase64Data decoded = new(xData);
            using System.IO.Stream stream = decoded.Data;
            using System.IO.BinaryReader br = new(stream);

            for (System.Int32 y = 0; y < height; y++)
            {
                for (System.Int32 x = 0; x < width; x++)
                {
                    // ReadUInt32 reads the GID (and flags) as little-endian as per TMX spec.
                    System.UInt32 gid = br.ReadUInt32();
                    Tiles.Add(new TmxLayerTile(gid, x + startX, y + startY));
                }
            }
        }
        else if (System.String.Equals(encoding, "csv", System.StringComparison.OrdinalIgnoreCase))
        {
            System.String csvData = xData.Value ?? System.String.Empty;
            System.Int32 k = 0;
            foreach (System.String raw in csvData.Split([',', '\n'], System.StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(s => s.Trim().Trim('\r')))
            {
                if (raw.Length == 0)
                {
                    continue;
                }

                if (!System.UInt32.TryParse(raw, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out System.UInt32 gid))
                {
                    throw new System.FormatException($"Invalid GID in CSV tile data: '{raw}'.");
                }

                System.Int32 x = k % width;
                System.Int32 y = k / width;
                Tiles.Add(new TmxLayerTile(gid, x + startX, y + startY));
                k++;
            }
        }
        else if (encoding is null)
        {
            System.Int32 k = 0;
            foreach (System.Xml.Linq.XElement e in xData.Elements("tile"))
            {
                System.UInt32 gid = (System.UInt32?)e.Attribute("gid") ?? 0u;
                System.Int32 x = k % width;
                System.Int32 y = k / width;
                Tiles.Add(new TmxLayerTile(gid, x + startX, y + startY));
                k++;
            }
        }
        else
        {
            throw new System.NotSupportedException($"TmxLayer: Unknown encoding '{encoding ?? "<null>"}'.");
        }
    }

    #endregion Private Methods
}
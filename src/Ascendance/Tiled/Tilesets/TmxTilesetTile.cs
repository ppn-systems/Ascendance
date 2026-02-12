using Ascendance.Tiled.Collections;
using Ascendance.Tiled.Core;
using Ascendance.Tiled.Layers;
using System.Xml.Linq;

namespace Ascendance.Tiled.Tilesets;

/// <summary>
/// Represents per-tile information inside a tileset (tile id specific data).
/// </summary>
public class TmxTilesetTile
{
    /// <summary>
    /// Local tile id within the tileset (not the global GID).
    /// </summary>
    public System.Int32 Id { get; }

    /// <summary>
    /// Terrain markers for the tile. Length is typically 4 (top-left, top-right, bottom-left, bottom-right).
    /// Some entries may be null when no terrain marker is present.
    /// </summary>
    public System.Collections.ObjectModel.Collection<TmxTerrain> TerrainEdges { get; }

    /// <summary>
    /// Probability weight for random tile selection.
    /// </summary>
    public System.Double Probability { get; }

    /// <summary>
    /// The "class" (preferred) or legacy "type" string for the tile.
    /// </summary>
    public System.String Type { get; }

    /// <summary>
    /// Custom properties for this tile.
    /// </summary>
    public PropertyDict Properties { get; }

    /// <summary>
    /// Optional image for this tile (if tileset uses per-tile images).
    /// </summary>
    public TmxImage Image { get; }

    /// <summary>
    /// Object groups attached to this tile (collision, etc.).
    /// </summary>
    public TmxList<TmxObjectGroup> ObjectGroups { get; }

    /// <summary>
    /// Animation frames for this tile (if animated).
    /// </summary>
    public System.Collections.ObjectModel.Collection<TmxAnimationFrame> AnimationFrames { get; }

    /// <summary>
    /// Convenience accessors for terrain corners (may return null if absent).
    /// </summary>
    public TmxTerrain TopLeft => TerrainEdges.Count > 0 ? TerrainEdges[0] : null;
    public TmxTerrain TopRight => TerrainEdges.Count > 1 ? TerrainEdges[1] : null;
    public TmxTerrain BottomLeft => TerrainEdges.Count > 2 ? TerrainEdges[2] : null;
    public TmxTerrain BottomRight => TerrainEdges.Count > 3 ? TerrainEdges[3] : null;

    /// <summary>
    /// Parse a &lt;tile&gt; element.
    /// </summary>
    /// <param name="xTile">Tile element to parse.</param>
    /// <param name="terrains">List of terrains declared on the parent tileset.</param>
    /// <param name="tmxDir">Base directory for resolving image paths.</param>
    public TmxTilesetTile(XElement xTile, TmxList<TmxTerrain> terrains, System.String tmxDir = "")
    {
        System.ArgumentNullException.ThrowIfNull(xTile);

        Id = (System.Int32?)xTile.Attribute("id") ?? 0;

        TerrainEdges = [];
        var strTerrain = (System.String)xTile.Attribute("terrain") ?? ",,,";
        var parts = strTerrain.Split(',');
        // Ensure we add exactly 4 entries (terrain markers are 4 comma-separated indices)
        for (System.Int32 i = 0; i < 4; i++)
        {
            if (i < parts.Length && System.Int32.TryParse(parts[i], out System.Int32 idx) && idx >= 0 && idx < terrains.Count)
            {
                TerrainEdges.Add(terrains[idx]);
            }
            else
            {
                TerrainEdges.Add(null);
            }
        }

        Probability = (System.Double?)xTile.Attribute("probability") ?? 1.0;
        Type = (System.String)xTile.Attribute("class") ?? (System.String)xTile.Attribute("type") ?? System.String.Empty;

        Image = new TmxImage(xTile.Element("image"), tmxDir);

        ObjectGroups = [];
        foreach (var e in xTile.Elements("objectgroup"))
        {
            ObjectGroups.Add(new TmxObjectGroup(e));
        }

        AnimationFrames = [];
        var anim = xTile.Element("animation");
        if (anim != null)
        {
            foreach (var e in anim.Elements("frame"))
            {
                AnimationFrames.Add(new TmxAnimationFrame(e));
            }
        }

        Properties = new PropertyDict(xTile.Element("properties"));
    }
}

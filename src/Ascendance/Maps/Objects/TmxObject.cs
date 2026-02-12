// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Abstractions;
using Ascendance.Maps.Collections;
using Ascendance.Maps.Enums;
using Ascendance.Maps.Layers;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Linq;

namespace Ascendance.Maps.Objects;

/// <summary>
/// Represents an object parsed from a Tiled &lt;object&gt; element.
/// An object may be a basic rectangle, tile object, ellipse, polygon, polyline, or text.
/// </summary>
public class TmxObject : ITmxElement
{
    #region Properties

    /// <summary>
    /// Object id.
    /// </summary>
    public System.Int32 Id { get; }

    /// <summary>
    /// Object name.
    /// </summary>
    public System.String Name { get; }

    /// <summary>
    /// Resolved object type (enum) describing the kind of object.
    /// </summary>
    public TmxObjectType ObjectType { get; }

    /// <summary>
    /// The "class" (preferred) or legacy "type" string from Tiled.
    /// </summary>
    public System.String Type { get; }

    /// <summary>
    /// X position in pixels.
    /// </summary>
    public System.Double X { get; }

    /// <summary>
    /// Y position in pixels.
    /// </summary>
    public System.Double Y { get; }

    /// <summary>
    /// Width in pixels (0.0 when absent/unused).
    /// </summary>
    public System.Double Width { get; }

    /// <summary>
    /// Height in pixels (0.0 when absent/unused).
    /// </summary>
    public System.Double Height { get; }

    /// <summary>
    /// Rotation in degrees (clockwise).
    /// </summary>
    public System.Double Rotation { get; }

    /// <summary>
    /// When the object is a tile object, this holds the tile information (GID + flip flags).
    /// </summary>
    public TmxLayerTile Tile { get; }

    /// <summary>
    /// Whether the object is visible. Defaults to true.
    /// </summary>
    public System.Boolean Visible { get; }

    /// <summary>
    /// Text content when the object contains a &lt;text&gt; child. Null when not a text object.
    /// </summary>
    public TmxText Text { get; }

    /// <summary>
    /// Points when the object is a polygon or polyline. Null when not applicable.
    /// </summary>
    public Collection<TmxObjectPoint> Points { get; }

    /// <summary>
    /// Custom properties attached to this object.
    /// </summary>
    public PropertyDict Properties { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Parse an &lt;object&gt; element into a <see cref="TmxObject"/>.
    /// </summary>
    /// <param name="xObject">The &lt;object&gt; element to parse. Must not be null.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xObject"/> is null.</exception>
    public TmxObject(XElement xObject)
    {
        System.ArgumentNullException.ThrowIfNull(xObject);

        Id = (System.Int32?)xObject.Attribute("id") ?? 0;
        Name = (System.String)xObject.Attribute("name") ?? System.String.Empty;

        // Positions: default to 0.0 when missing
        X = (System.Double?)xObject.Attribute("x") ?? 0.0;
        Y = (System.Double?)xObject.Attribute("y") ?? 0.0;
        Width = (System.Double?)xObject.Attribute("width") ?? 0.0;
        Height = (System.Double?)xObject.Attribute("height") ?? 0.0;

        // Tiled 1.9+ uses 'class'; legacy maps use 'type'
        Type = (System.String)xObject.Attribute("class") ?? (System.String)xObject.Attribute("type") ?? System.String.Empty;

        Visible = (System.Boolean?)xObject.Attribute("visible") ?? true;
        Rotation = (System.Double?)xObject.Attribute("rotation") ?? 0.0;

        // Determine object kind by presence of child/attributes
        var xGid = xObject.Attribute("gid");
        var xEllipse = xObject.Element("ellipse");
        var xPolygon = xObject.Element("polygon");
        var xPolyline = xObject.Element("polyline");

        if (xGid != null && System.UInt32.TryParse(xGid.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedGid))
        {
            // Convert object position to tile indices by rounding pixel positions.
            System.Int32 tileX = System.Convert.ToInt32(System.Math.Round(X));
            System.Int32 tileY = System.Convert.ToInt32(System.Math.Round(Y));
            Tile = new TmxLayerTile(parsedGid, tileX, tileY);
            ObjectType = TmxObjectType.Tile;
        }
        else if (xEllipse != null)
        {
            ObjectType = TmxObjectType.Ellipse;
        }
        else if (xPolygon != null)
        {
            Points = PARSE_POINTS(xPolygon);
            ObjectType = TmxObjectType.Polygon;
        }
        else if (xPolyline != null)
        {
            Points = PARSE_POINTS(xPolyline);
            ObjectType = TmxObjectType.Polyline;
        }
        else
        {
            ObjectType = TmxObjectType.Basic;
        }

        var xText = xObject.Element("text");
        if (xText != null)
        {
            Text = new TmxText(xText);
        }

        Properties = new PropertyDict(xObject.Element("properties"));
    }

    #endregion Constructor

    #region Private Methods

    /// <summary>
    /// Parse a &lt;polygon points="x1,y1 x2,y2 ..." /&gt; or &lt;polyline ... /&gt; element into a collection of points.
    /// </summary>
    /// <param name="xPoints">The &lt;polygon&gt; or &lt;polyline&gt; element. Must not be null.</param>
    /// <returns>A collection of parsed <see cref="TmxObjectPoint"/> objects (may be empty).</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="xPoints"/> is null.</exception>
    private static Collection<TmxObjectPoint> PARSE_POINTS(XElement xPoints)
    {
        System.ArgumentNullException.ThrowIfNull(xPoints);

        var points = new Collection<TmxObjectPoint>();
        var raw = (System.String)xPoints.Attribute("points") ?? System.String.Empty;

        foreach (var token in raw.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            // Each token is "x,y"
            var pair = token.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length != 2)
            {
                continue;
            }

            // Parse using invariant culture
            if (System.Double.TryParse(pair[0].Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var px)
                && System.Double.TryParse(pair[1].Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var py))
            {
                points.Add(new TmxObjectPoint(px, py));
            }
            // If parsing fails, skip the point silently to be robust against malformed data.
        }

        return points;
    }

    #endregion Private Methods
}
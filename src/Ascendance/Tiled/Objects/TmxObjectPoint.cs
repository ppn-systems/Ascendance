// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Tiled.Objects;

/// <summary>
/// Represents a point used by TmxObject polygon/polyline definitions.
/// </summary>
public class TmxObjectPoint
{
    /// <summary>
    /// X coordinate in pixels.
    /// </summary>
    public System.Double X { get; }

    /// <summary>
    /// Y coordinate in pixels.
    /// </summary>
    public System.Double Y { get; }

    /// <summary>
    /// Creates a new TmxObjectPoint with explicit coordinates.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public TmxObjectPoint(System.Double x, System.Double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Parses a point from a string of the form "x,y" using invariant culture.
    /// Throws <see cref="System.ArgumentNullException"/> if <paramref name="s"/> is null or empty.
    /// Throws <see cref="System.FormatException"/> if the string is not a valid "x,y" pair.
    /// </summary>
    /// <param name="s">Point string (e.g. "12.5,3.0").</param>
    public TmxObjectPoint(System.String s)
    {
        if (System.String.IsNullOrWhiteSpace(s))
        {
            throw new System.ArgumentNullException(nameof(s), "Point string must not be null or empty.");
        }

        var (x, y) = PARSE_POINT_STRING(s);
        X = x;
        Y = y;
    }

    /// <summary>
    /// Attempts to parse a point string of the form "x,y".
    /// Returns true on success and assigns <paramref name="point"/>; otherwise returns false.
    /// Parsing uses <see cref="System.Globalization.CultureInfo.InvariantCulture"/>.
    /// </summary>
    /// <param name="s">Point string to parse.</param>
    /// <param name="point">Parsed point on success; null on failure.</param>
    /// <returns>True if parse succeeded; otherwise false.</returns>
    public static System.Boolean TryParse(System.String s, out TmxObjectPoint point)
    {
        point = null;
        if (System.String.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        try
        {
            var (x, y) = PARSE_POINT_STRING(s);
            point = new TmxObjectPoint(x, y);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Internal helper that parses and returns a tuple (x,y).
    private static (System.Double X, System.Double Y) PARSE_POINT_STRING(System.String s)
    {
        // Expect "x,y" where both parts are parseable doubles (invariant culture).
        System.String[] parts = s.Split([','], System.StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 2
            ? throw new System.FormatException($"Invalid point format: '{s}'. Expected format 'x,y'.")
            : !System.Double.TryParse(parts[0].Trim(),
            System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
            System.Globalization.CultureInfo.InvariantCulture, out var x)
            ? throw new System.FormatException($"Invalid X coordinate in point: '{parts[0]}'.")
            : !System.Double.TryParse(parts[1].Trim(),
            System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands,
            System.Globalization.CultureInfo.InvariantCulture, out var y)
            ? throw new System.FormatException($"Invalid Y coordinate in point: '{parts[1]}'.")
            : ((System.Double X, System.Double Y))(x, y);
    }
}
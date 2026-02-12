using System.Xml.Linq;

namespace Ascendance.Tiled.Core;

/// <summary>
/// Representation of an image used by tilesets, image-layers, or tile definitions.
/// </summary>
public class TmxImage
{
    /// <summary>
    /// Source path to the image file (when present).
    /// </summary>
    public System.String Source { get; private set; }

    /// <summary>
    /// Declared image format when embedded as data.
    /// </summary>
    public System.String Format { get; private set; }

    /// <summary>
    /// Decoded image data stream (when embedded as base64).
    /// Caller is responsible for not disposing this stream if it's needed elsewhere.
    /// </summary>
    public System.IO.Stream Data { get; private set; }

    /// <summary>
    /// Transparency color (if specified).
    /// </summary>
    public TmxColor Trans { get; private set; }

    /// <summary>
    /// Optional declared width.
    /// </summary>
    public System.Int32? Width { get; private set; }

    /// <summary>
    /// Optional declared height.
    /// </summary>
    public System.Int32? Height { get; private set; }

    /// <summary>
    /// Parse an &lt;image&gt; element. If xImage is null the instance will be left empty.
    /// </summary>
    /// <param name="xImage">The &lt;image&gt; element.</param>
    /// <param name="tmxDir">Base directory for resolving relative Source paths.</param>
    public TmxImage(XElement xImage, System.String tmxDir = "")
    {
        if (xImage == null)
        {
            return;
        }

        var xSource = xImage.Attribute("source");
        if (xSource != null)
        {
            // Combine with tmxDir when relative
            Source = System.IO.Path.Combine(tmxDir ?? System.String.Empty, (System.String)xSource);
        }
        else
        {
            Format = (System.String)xImage.Attribute("format") ?? System.String.Empty;
            var xData = xImage.Element("data");
            if (xData != null)
            {
                var decodedStream = new TmxBase64Data(xData);
                Data = decodedStream.Data;
            }
        }

        Trans = new TmxColor(xImage.Attribute("trans"));
        Width = (System.Int32?)xImage.Attribute("width");
        Height = (System.Int32?)xImage.Attribute("height");
    }
}

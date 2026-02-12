// Copyright (c) 2025 PPN Corporation. All rights reserved.


// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Maps.Abstractions;

namespace Ascendance.Maps.Core;

/// <summary>
/// Base functionality for reading TMX/TSX documents and providing a small parsing helper surface.
/// </summary>
public abstract class TmxDocument
{
    /// <summary>
    /// Directory containing the loaded TMX/TSX file. Empty when resource-loaded.
    /// </summary>
    public System.String TmxDirectory { get; private set; }

    /// <summary>
    /// Optional custom loader to delegate reading XML (useful for embedded resources, VFS, test stubs).
    /// </summary>
    protected ICustomLoader CustomLoader { get; }

    /// <summary>
    /// Create a new instance with an optional custom loader.
    /// </summary>
    /// <param name="customLoader">Optional custom loader.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    protected TmxDocument(ICustomLoader customLoader)
    {
        CustomLoader = customLoader;
        TmxDirectory = System.String.Empty;
    }

    /// <summary>
    /// Reads an XML document either via the custom loader or directly from the filesystem/assembly resources.
    /// </summary>
    /// <param name="filepath">File path to load (relative or absolute).</param>
    /// <returns>Parsed XDocument.</returns>
    /// <exception cref="System.ArgumentNullException">If <paramref name="filepath"/> is null or empty.</exception>
    /// <exception cref="System.IO.FileNotFoundException">If the file does not exist on disk when no resource is found.</exception>
    protected System.Xml.Linq.XDocument ReadXml(System.String filepath)
    {
        if (System.String.IsNullOrWhiteSpace(filepath))
        {
            throw new System.ArgumentNullException(nameof(filepath));
        }

        if (CustomLoader != null)
        {
            return CustomLoader.ReadXml(filepath);
        }

        System.Xml.Linq.XDocument xDoc;

        System.String[] manifest = [];
        System.Reflection.Assembly asm = System.Reflection.Assembly.GetEntryAssembly();

        if (asm != null)
        {
            manifest = asm.GetManifestResourceNames();
        }

        // Try to match an embedded resource by transforming the filesystem path to a resource-style path.
        System.String fileResPath = filepath.Replace(System.IO.Path.DirectorySeparatorChar.ToString(), ".");
        System.String fileRes = System.Array.Find(manifest, s => s.EndsWith(fileResPath));

        if (fileRes != null && asm != null)
        {
            using (System.IO.Stream xmlStream = asm.GetManifestResourceStream(fileRes))
            {
                if (xmlStream == null)
                {
                    throw new System.IO.FileNotFoundException("Embedded resource not found.", fileRes);
                }

                using System.Xml.XmlReader reader = System.Xml.XmlReader.Create(xmlStream);
                xDoc = System.Xml.Linq.XDocument.Load(reader);
            }

            // Resource-loaded, no directory on disk
            TmxDirectory = System.String.Empty;
        }
        else
        {
            // Load from disk path
            if (!System.IO.File.Exists(filepath))
            {
                throw new System.IO.FileNotFoundException("TMX/TSX file not found.", filepath);
            }

            xDoc = System.Xml.Linq.XDocument.Load(filepath);
            TmxDirectory = System.IO.Path.GetDirectoryName(filepath) ?? System.String.Empty;
        }

        return xDoc;
    }
}

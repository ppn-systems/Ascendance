// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Maps.Collections;


/// <summary>
/// Dictionary of Tiled custom properties parsed from a &lt;properties&gt; element.
/// </summary>
[System.Serializable]
public class PropertyDict : System.Collections.Generic.Dictionary<System.String, System.String>
{
    /// <summary>
    /// Build the dictionary from a &lt;properties&gt; container element. If xmlProp is null, an empty dictionary is created.
    /// </summary>
    /// <param name="xmlProp">The &lt;properties&gt; element or null.</param>
    public PropertyDict(System.Xml.Linq.XContainer xmlProp)
    {
        if (xmlProp == null)
        {
            return;
        }

        foreach (System.Xml.Linq.XElement p in xmlProp.Elements("property"))
        {
            System.String pname = (System.String)p.Attribute("name") ?? System.String.Empty;
            System.String pval;

            // Try attribute "value" first, otherwise fall back to element body
            System.Xml.Linq.XAttribute valAttr = p.Attribute("value");
            pval = valAttr != null ? (System.String)valAttr : p.Value ?? System.String.Empty;

            // If duplicate property name exists, overwrite with the later one (same behavior as original)
            this[pname] = pval;
        }
    }
}
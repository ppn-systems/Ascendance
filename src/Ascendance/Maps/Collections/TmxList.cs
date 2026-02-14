// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Abstractions;

namespace Ascendance.Maps.Collections;

/// <summary>
/// Keyed collection that attempts to keep unique keys when multiple items share the same Name.
/// Public API: T implements ITmxElement and the collection is keyed by a generated unique string.
/// </summary>
/// <typeparam name="T">Item type implementing ITmxElement.</typeparam>
public class TmxList<T> : System.Collections.ObjectModel.KeyedCollection<System.String, T> where T : ITmxElement
{
    // Track counts for base names so we can generate stable unique keys like "name", "name_1", "name_2", ...
    private readonly System.Collections.Generic.Dictionary<System.String, System.Int32> nameCount = [];

    /// <summary>
    /// Add an item to the collection. This method ensures nameCount has an entry so GetKeyForItem can produce a unique key.
    /// </summary>
    /// <param name="t">Item to add.</param>
    public new void Add(T t)
    {
        System.String tName = t?.Name ?? System.String.Empty;

        // Ensure a counter exists for this base name
        if (!nameCount.ContainsKey(tName))
        {
            nameCount[tName] = 0;
        }

        base.Add(t);
    }

    /// <summary>
    /// Produces a unique key for the given item based on its Name, appending an incrementing suffix when necessary.
    /// </summary>
    /// <param name="item">Item to generate a key for.</param>
    /// <returns>Unique key string.</returns>
    protected override System.String GetKeyForItem(T item)
    {
        System.String baseName = item?.Name ?? System.String.Empty;

        // If the base name is not present yet as a key, use it directly.
        if (!this.Contains(baseName))
        {
            // Ensure the counter exists for future duplicates.
            if (!nameCount.ContainsKey(baseName))
            {
                nameCount[baseName] = 0;
            }

            return baseName;
        }

        // Otherwise generate a unique suffixed key: "name_1", "name_2", ...
        System.Int32 count = nameCount.TryGetValue(baseName, out System.Int32 existing) ? existing : 0;

        System.String candidate;
        do
        {
            count++;
            candidate = baseName + "_" + count.ToString();
        } while (this.Contains(candidate));

        // Persist the updated counter
        nameCount[baseName] = count;

        return candidate;
    }
}

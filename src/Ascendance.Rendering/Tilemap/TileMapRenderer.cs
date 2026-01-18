using Ascendance.Rendering.Entities;
using Ascendance.Shared.Enums;
using Ascendance.Shared.Map;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Tilemap;

public class TileMapRenderer
{
    private readonly SpriteBatch[] _batches;

    public TileMapRenderer(Texture[] tileTextures, System.Int32 tileWidth, System.Int32 tileHeight, TileMap map)
    {
        _batches = new SpriteBatch[tileTextures.Length];

        for (System.Int32 i = 0; i < tileTextures.Length; i++)
        {
            _batches[i] = new SpriteBatch(tileTextures[i]);
        }
        for (System.UInt32 y = 0; y < map.Height; y++)
        {
            for (System.UInt32 x = 0; x < map.Width; x++)
            {
                Tile tile = map.GetTile((System.Int32)x, (System.Int32)y);
                if (tile.Type == TileType.Unknown)
                {
                    continue;
                }

                System.Int32 id = (System.Int32)tile.Type;
                System.Int32 cols = (System.Int32)tileTextures[id].Size.X / tileWidth;
                System.Int32 tx = id % cols, ty = id / cols;
                IntRect src = new(tx * tileWidth, ty * tileHeight, tileWidth, tileHeight);

                _batches[id].Add(new Vector2f(x * tileWidth, y * tileHeight), src);
            }
        }
    }

    public void Draw(RenderTarget target)
    {
        foreach (var batch in _batches)
        {
            batch?.Draw(target);
        }
    }
}
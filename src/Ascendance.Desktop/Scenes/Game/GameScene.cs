// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Tiles;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Game;

[DynamicLoad]
public sealed class GameScene : BaseScene
{
    public readonly Vector2f PlayerStartPosition = new(400, 170);
    public GameScene() : base(SceneConstants.MainGame)
    {
    }

    protected override void LoadObjects()
    {
        TileMap tileMap = TmxMapLoader.Load("res/maps/1.tmx");
        Texture playerTexture = AssetManager.Instance.LoadTexture("res/texture/characters/2.png");

        Characters.Character player = new(playerTexture, this.PlayerStartPosition)
        {
            TileMap = tileMap,
            CollisionLayerName = "collision"
        };

        // Ensure camera viewport size is set first (match your window or desired viewport)
        Camera2D.Instance.Reset(PlayerStartPosition, new Vector2f(720, 405));

        // Set the world bounds: width = number of tiles horizontally * tile pixel width,
        // height = number of tiles vertically * tile pixel height.
        // Note: use tileMap.Width for horizontal tile count, tileMap.Height for vertical.
        Camera2D.Instance.Bounds = new FloatRect(
            0f,
            0f,
            tileMap.PixelWidth,    // total map pixel width (not tile count * tile size)
            tileMap.PixelHeight    // total map pixel height
        );

        // Enable clamping
        Camera2D.Instance.ClampEnabled = true;

        // Apply zoom AFTER Reset so it is not overwritten by Reset's SetZoom(1)
        Camera2D.Instance.Zoom(0.85f);

        player.Camera = Camera2D.Instance;
        player.SetZIndex(10);

        base.AddObject(player);
        base.AddObject(tileMap);
    }
}
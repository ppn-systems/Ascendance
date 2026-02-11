// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Game.Entities;
using Ascendance.Game.Loaders;
using Ascendance.Game.Tilemaps;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Game;

[DynamicLoad]
public sealed class GameScene : BaseScene
{
    public GameScene() : base(SceneConstants.MainGame)
    {
    }

    protected override void LoadObjects()
    {
        TileMap tileMap = TmxMapLoader.Load("res/maps/2.tmx");
        Texture playerTexture = AssetManager.Instance.LoadTexture("res/texture/characters/1.png");

        Player player = new(playerTexture, new Vector2f(400, 300))
        {
            TileMap = tileMap,
            CollisionLayerName = "Collision"
        };

        Camera2D.Instance.Reset(new Vector2f(400, 300), new Vector2f(600, 400));
        player.Camera = Camera2D.Instance;
        player.SetZIndex(10);

        base.AddObject(player);
        base.AddObject(tileMap);
    }
}

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
    public readonly Vector2f PlayerStartPosition = new(320, 325);
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
            CollisionLayerName = "Collision"
        };

        Camera2D.Instance.Reset(PlayerStartPosition, new Vector2f(720, 405));
        player.Camera = Camera2D.Instance;
        player.SetZIndex(10);

        base.AddObject(player);
        base.AddObject(tileMap);
    }
}

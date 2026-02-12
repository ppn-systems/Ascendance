// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Rendering;
using Ascendance.Maps;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Injection;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Game;

[DynamicLoad]
public sealed class GameScene : BaseScene
{
    private readonly Vector2f Position = new(400, 170);

    public GameScene() : base(SceneConstants.MainGame)
    {
        Texture texture = AssetManager.Instance.LoadTexture("res/texture/characters/2.png");

        Characters.Character character = new(texture, this.Position);
        InstanceManager.Instance.Register<Characters.Character>(character);
    }

    protected override void LoadObjects()
    {
        TmxMap map = new("res/maps/1.tmx");

        TmxMapRenderer mapRenderer = new(map)
        {
            UseViewportCulling = true,
            MapOffset = new Vector2f(0, 0)
        };

        Characters.Character character = InstanceManager.Instance.GetExistingInstance<Characters.Character>();

        character.TmxMap = map;
        character.Camera = Camera2D.Instance;
        character.CollisionLayerName = "collision";

        Camera2D.Instance.ClampEnabled = true;
        Camera2D.Instance.Bounds = new FloatRect(
            0f, 0f,
            map.Width * map.TileWidth,     // total map pixel width (not tile count * tile size)
            map.Height * map.TileHeight    // total map pixel height
        );

        Camera2D.Instance.Zoom(0.85f);
        Camera2D.Instance.Reset(Position, new Vector2f(720, 405));


        character.SetZIndex(10);

        base.AddObject(character);
        base.AddObject(mapRenderer);
    }
}
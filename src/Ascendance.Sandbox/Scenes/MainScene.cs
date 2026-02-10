// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Domain.Players;
using Ascendance.Domain.Tiles;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Indicators;
using Nalix.Framework.Configuration;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Sandbox.Scenes;

[DynamicLoad]
public sealed class MainScene : BaseScene
{
    public static readonly DebugOverlay Overlay = new();

    private static void OnFrameRender(RenderTarget target) => Overlay.Draw(target);

    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene) => GraphicsEngine.Instance.FrameRender += OnFrameRender;

    protected override void LoadObjects()
    {
        // 1. Load texture nguyên sheet (KHÔNG cần cắt thủ công)
        Texture playerTexture = AssetManager.Instance.LoadTexture("res/texture/characters/1.png");

        // 2. Tạo Player - AnimationController tự động cắt sprite bên trong
        Player player = new(playerTexture, new Vector2f(400, 300));

        // 3. Setup tile map collision
        TileMap tileMap = TmxMapLoader.Load("res/maps/2.tmx");
        player.TileMap = tileMap;
        player.CollisionLayerName = "Collision";

        // 4. Setup camera
        Camera2D.Instance.Reset(new Vector2f(400, 300), new Vector2f(600, 400));
        player.Camera = Camera2D.Instance;
        player.SetZIndex(10);

        base.AddObject(player);
        base.AddObject(tileMap);
    }
}

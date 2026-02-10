// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.Tiles;
using Nalix.Framework.Configuration;
using SFML.System;

namespace Ascendance.Sandbox.Rendering.Scenes;

/// <summary>
/// Test scene for tile map rendering with player movement and collision.
/// Scene test để render tile map với di chuyển player và va chạm.
/// </summary>
[DynamicLoad]
internal sealed class TileMapTestScene : BaseScene
{
    private TileMap _tileMap;
    private Camera2D _camera;
    private PlayerCharacter _player;

    public TileMapTestScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects()
    {
        // Setup camera
        Vector2f screenSize = new(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);
        _camera = new Camera2D(new Vector2f(0, 0), screenSize);

        // Load tile map from TMX file
        _tileMap = TmxMapLoader.Load("res/maps/test_map.tmx", AssetManager.Instance);

        if (_tileMap != null)
        {
            _tileMap.Camera = _camera;
            _tileMap.SetZIndex(-10); // Draw map behind everything
            base.AddObject(_tileMap);

            // Set camera bounds to map size
            _camera.Bounds = new SFML.Graphics.FloatRect(0, 0, _tileMap.PixelWidth, _tileMap.PixelHeight);
        }

        // Create player
        _player = new PlayerCharacter(_tileMap)
        {
            Position = new Vector2f(100, 100)
        };
        _player.SetZIndex(0);
        base.AddObject(_player);
    }
}
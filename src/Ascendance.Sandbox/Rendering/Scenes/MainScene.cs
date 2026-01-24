// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Indicators;

namespace Ascendance.Sandbox.Rendering.Scenes;

[AutoLoad]
internal sealed class MainScene : BaseScene
{
    private readonly LoadingOverlay _overlay;

    public MainScene()
        : base(SceneNames.Main)
    {
        _overlay = new LoadingOverlay();
        _overlay.Show();
    }

    public class DummyRenderObject : RenderObject
    {
        private SFML.Graphics.RectangleShape _rect;
        public DummyRenderObject()
        {
            System.Console.WriteLine($"Khởi tạo DummyRect — ScreenSize: {GraphicsEngine.ScreenSize.X},{GraphicsEngine.ScreenSize.Y}");
            _rect = new SFML.Graphics.RectangleShape(new SFML.System.Vector2f(100, 100))
            {
                FillColor = SFML.Graphics.Color.Red
            };
        }
        protected override SFML.Graphics.Drawable GetDrawable() => _rect;
    }

    protected override void LoadObjects()
    {
        //System.Console.WriteLine("=== LoadObjects của MainScene đang được gọi ===");

        base.AddObject(_overlay);
        //System.Console.WriteLine("Đã Add DummyRect vào scene!");
        //AssetManager.Instance.LoadTexture(GraphicsConfig.AssetRoot + "");//Button button = new("OK", );
        _overlay.Show();
    }
}

// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
using Ascendance.Rendering.UI.Indicators;
using SFML.Graphics;

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

    protected override void LoadObjects()
    {
        Texture texture = AssetManager.Instance.LoadTexture("res/texture/panels/000.png");
        Font font = AssetManager.Instance.LoadFont("res/fonts/1.ttf");
        Button button = new("OK", texture, font);
        button.SetPosition(new(20, 20));
        base.AddObject(button);
    }
}

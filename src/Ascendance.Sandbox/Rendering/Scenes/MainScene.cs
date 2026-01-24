// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
using Ascendance.Rendering.UI.Indicators;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Sandbox.Rendering.Scenes;

[AutoLoad]
internal sealed class MainScene : BaseScene
{
    private readonly LoadingOverlay _overlay;

    public MainScene()
        : base(SceneConstants.Main)
    {
        _overlay = new LoadingOverlay();
        _overlay.Show();
    }

    protected override void LoadObjects()
    {
        Texture texture = AssetManager.Instance.LoadTexture("res/texture/panels/000.png");
        Font font = AssetManager.Instance.LoadFont("res/fonts/1.ttf");
        TextInputField textInputField = new(texture, default, font, 20, new Vector2f(200, 40), new Vector2f(100, 100));
        Button button = new("OK", texture, font);
        button.SetPosition(new(20, 20));
        base.AddObject(button);
        base.AddObject(textInputField);
    }
}

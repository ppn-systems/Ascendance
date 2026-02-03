// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
using Ascendance.Rendering.UI.Indicators;
using Ascendance.Rendering.UI.Notifications;
using Nalix.Framework.Configuration;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Sandbox.Rendering.Scenes;

[DynamicLoad]
internal sealed class MainScene : BaseScene
{
    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects()
    {
        Font font = AssetManager.Instance.LoadFont("res/fonts/1.ttf");
        Texture texture = AssetManager.Instance.LoadTexture("res/texture/transparent_border/015");

        DebugOverlay debug = new(font);
        Button button = new("OK", texture, font);
        NotificationButton notification = new(font, texture, "askdmasdaklsndkl", Direction2D.Down);
        PasswordField passwordField = new(texture, default, font, 20, new Vector2f(200, 40), new Vector2f(200, 200));
        TextInputField textInputField = new(texture, default, font, 20, new Vector2f(200, 40), new Vector2f(100, 100));

        notification.SetZIndex(10);
        button.SetPosition(new(20, 20));

        base.AddObject(debug);
        base.AddObject(button);
        base.AddObject(notification);
        base.AddObject(passwordField);
        base.AddObject(textInputField);
    }
}

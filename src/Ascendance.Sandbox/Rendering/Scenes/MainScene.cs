// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Managers;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Sandbox.Rendering.Scenes;

[DynamicLoad]
internal sealed class MainScene : BaseScene
{
    public MainScene() : base(SceneConstants.Main)
    {
    }

    protected override void LoadObjects()
    {
        Font font = AssetManager.Instance.LoadFont("res/fonts/1.ttf");
        Texture texture = AssetManager.Instance.LoadTexture("res/texture/transparent_border/015");

        Button button = new("OK", texture, font);
        ButtonNotification notification = new(font, texture, "askdmasdaklsndkl", Direction2D.Down);
        PasswordField passwordField = new(texture, default, font, 20, new Vector2f(200, 40), new Vector2f(200, 200));
        TextInputField textInputField = new(texture, default, font, 20, new Vector2f(200, 40), new Vector2f(100, 100));

        notification.SetZIndex(10);
        button.SetPosition(new(20, 20));

        base.AddObject(button);
        base.AddObject(notification);
        base.AddObject(passwordField);
        base.AddObject(textInputField);
    }
}

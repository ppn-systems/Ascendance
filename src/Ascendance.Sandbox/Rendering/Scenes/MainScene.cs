// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Enums;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Controls;
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
        Texture texture = EmbeddedAssets.SquareOutline.ToTexture();

        Button button = new("OK", texture);
        NotificationButton notification = new(texture, "D=false", Direction2D.Down);
        PasswordField passwordField = new(texture, default, new Vector2f(200, 40), new Vector2f(200, 200));
        TextInputField textInputField = new(texture, default, new Vector2f(200, 40), new Vector2f(100, 100));

        notification.SetZIndex(10);
        button.SetPosition(new(20, 20));

        base.AddObject(button);
        base.AddObject(notification);
        base.AddObject(passwordField);
        base.AddObject(textInputField);
    }
}

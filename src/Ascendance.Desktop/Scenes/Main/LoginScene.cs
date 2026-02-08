// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.Main.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.Main;

[DynamicLoad]
public sealed class LoginScene : BaseScene
{
    public LoginScene() : base(SceneConstants.Login)
    {
    }

    protected override void LoadObjects()
    {
        LoginView loginView = new();
        loginView.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        base.AddObject(loginView);
    }
}

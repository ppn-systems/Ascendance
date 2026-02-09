// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.Login.View;
using Ascendance.Desktop.Scenes.ServerSelection.View;
using Ascendance.Desktop.Services;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.Login;

[DynamicLoad]
public sealed class LoginScene : BaseScene
{
    public LoginScene() : base(SceneConstants.Login)
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Roslynator", "RCS1021:Convert lambda expression body to expression body", Justification = "<Pending>")]
    protected override void LoadObjects()
    {
        LoginView loginView = new();
        BackdropView background = new();

        loginView.SetZIndex(1);
        background.SetZIndex(0);

        loginView.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        loginView.SubmitRequested += () =>
        {
            Credentials.Save(loginView.Username, loginView.Password);
        };

        base.AddObject(loginView);
        base.AddObject(background);
    }
}

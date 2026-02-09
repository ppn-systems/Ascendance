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

    protected override void LoadObjects()
    {
        LoginView loginView = new();
        BackdropView background = new();
        BackButtonView backButtonView = new();

        loginView.SetZIndex(1);
        background.SetZIndex(0);
        backButtonView.SetZIndex(1);

        backButtonView.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        loginView.SubmitRequested += () =>
        {
            System.Console.WriteLine($"Login submitted with username: {loginView.Username}");

            if (loginView.Username != System.String.Empty && loginView.Password != System.String.Empty)
            {
                Credentials.Save(loginView.Username, loginView.Password);
            }
        };

        loginView.ForgetPasswordRequested += () =>
        {
            // Open the password recovery webpage
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://www.ascendancegame.com/recover-password",
                UseShellExecute = true
            });
        };

        base.AddObject(loginView);
        base.AddObject(background);
        base.AddObject(backButtonView);
    }
}

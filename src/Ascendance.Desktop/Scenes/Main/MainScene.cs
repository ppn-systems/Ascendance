// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.Main.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Banners;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.Main;

[DynamicLoad]
public sealed class MainScene : BaseScene
{
    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
    }

    protected override void LoadObjects()
    {
        Camera2D.Instance.SetCenter(new SFML.System.Vector2f(GraphicsEngine.ScreenSize.X / 2f, GraphicsEngine.ScreenSize.Y / 2f));

        ButtonView buttonView = new();
        VersionView versionView = new();
        ParallaxView parallaxLayerView = new();
        ScrollingBanner scrollingBannerView = new("⚠ Playing games for more than 180 minutes a day can negatively impact your health ⚠", null, 200f);

        buttonView.ChangeAccountRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(SceneConstants.Login);

        buttonView.LoginRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(SceneConstants.MainGame);

        buttonView.NewGameRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(SceneConstants.CharCreation);

        buttonView.ServerInfoRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(SceneConstants.ServerSelect);

        base.AddObject(buttonView);
        base.AddObject(versionView);
        base.AddObject(parallaxLayerView);
        base.AddObject(scrollingBannerView);
    }
}

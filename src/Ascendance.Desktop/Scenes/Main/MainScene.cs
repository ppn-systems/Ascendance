// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.Main.View;
using Ascendance.Rendering.Attributes;
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
        ButtonView buttonView = new()
        {
            IsLoginButtonVisible = false
        };
        buttonView.ChangeAccountRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(SceneConstants.Login);

        ParallaxLayerView parallaxLayerView = new();
        ScrollingBanner scrollingBannerView = new("⚠ Playing games for more than 180 minutes a day can negatively impact your health ⚠", null, 200f);

        base.AddObject(buttonView);
        base.AddObject(parallaxLayerView);
        base.AddObject(scrollingBannerView);
    }
}

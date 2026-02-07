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
    private readonly ParallaxLayerView _parallaxLayerView;

    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene) => _parallaxLayerView = new ParallaxLayerView(null, -10);

    protected override void LoadObjects()
    {
        base.AddObject(_parallaxLayerView);
        base.AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", null, 200f));
    }
}

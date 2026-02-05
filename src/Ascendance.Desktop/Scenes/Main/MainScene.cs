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

    public MainScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene)
    {
        ParallaxLayerView.ParallaxPreset preset = new()
        {
            Variant = 1,
            Layers =
            [
                new ParallaxLayerView.ParallaxPreset.Layer
                {
                    Speed = 00f,
                    Repeat = true,
                    TexturePath = "res/texture/wcp/1"
                },
                new ParallaxLayerView.ParallaxPreset.Layer
                {
                    Speed = 35f,
                    Repeat = true,
                    TexturePath = "res/texture/wcp/2"
                },
                new ParallaxLayerView.ParallaxPreset.Layer
                {
                    Speed = 40f,
                    Repeat = true,
                    TexturePath = "res/texture/wcp/3"
                },
                new ParallaxLayerView.ParallaxPreset.Layer
                {
                    Speed = 45f,
                    Repeat = true,
                    TexturePath = "res/texture/wcp/4"
                },
                new ParallaxLayerView.ParallaxPreset.Layer
                {
                    Speed = 50f,
                    Repeat = true,
                    TexturePath = "res/texture/wcp/5"
                },
            ]
        };

        _parallaxLayerView = new ParallaxLayerView(preset, -10);
    }

    protected override void LoadObjects()
    {
        base.AddObject(_parallaxLayerView);
        base.AddObject(new ScrollingBanner("⚠ Chơi quá 180 phút mỗi ngày sẽ ảnh hưởng xấu đến sức khỏe ⚠", null, 200f));
    }
}

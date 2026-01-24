// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Indicators;

namespace Ascendance.Sandbox.Rendering.Scenes;

[AutoLoad]
internal sealed class MainScene : BaseScene
{
    private readonly LoadingOverlay _overlay;

    public MainScene()
        : base(SceneNames.Main)
    {
        _overlay = new LoadingOverlay();
        _overlay.Show();
    }

    protected override void LoadObjects() => base.AddObject(_overlay);//AssetManager.Instance.LoadTexture(GraphicsConfig.AssetRoot + "");//Button button = new("OK", );
}

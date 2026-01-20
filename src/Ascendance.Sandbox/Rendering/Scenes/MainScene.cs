// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Scenes;
using Ascendance.Rendering.UI.Indicators;

namespace Ascendance.Sandbox.Rendering.Scenes;

[AutoLoad]
internal sealed class MainScene : BaseScene
{
    public MainScene()
        : base(SceneNames.Main)
    {
    }


    protected override void LoadObjects()
    {
        LoadingOverlay loading = new();

        base.AddObject(loading);

        loading.SetZIndex(999);
        loading.Show();
    }
}

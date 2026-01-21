// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Scenes;

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
        // Successfully loaded scene objects
        //LoadingOverlay loading = new();

        //base.AddObject(loading);
        //loading.Show();

        //AssetManager.Instance.LoadTexture(GraphicsConfig.AssetRoot + "");
        //Button button = new("OK", );
    }
}

// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.Main.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.Main;

[DynamicLoad]
public sealed class ServerSelectionScene : BaseScene
{
    public ServerSelectionScene() : base(SceneConstants.ServerInfo)
    {
    }

    protected override void LoadObjects()
    {
        ServerSelectionView serverSelectionView = new();

        serverSelectionView.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        base.AddObject(serverSelectionView);
    }
}

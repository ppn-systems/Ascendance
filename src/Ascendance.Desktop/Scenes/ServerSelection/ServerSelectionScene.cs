// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.ServerSelection.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.ServerSelection;

[DynamicLoad]
public sealed class ServerSelectionScene : BaseScene
{
    public ServerSelectionScene() : base(SceneConstants.ServerSelection)
    {
    }

    protected override void LoadObjects()
    {
        HeaderView title = new();
        BackdropView background = new();
        ServerSelectionView serverInfo = new();

        serverInfo.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        base.AddObject(title);
        base.AddObject(background);
        base.AddObject(serverInfo);
    }
}

// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.ServerInfo.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.ServerInfo;

[DynamicLoad]
public sealed class ServerInfoScene : BaseScene
{
    public ServerInfoScene() : base(SceneConstants.ServerInfo)
    {
    }

    protected override void LoadObjects()
    {
        TitleView title = new();
        BackgroundView background = new();
        ServerInfoView serverInfo = new();

        serverInfo.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        base.AddObject(title);
        base.AddObject(background);
        base.AddObject(serverInfo);
    }
}

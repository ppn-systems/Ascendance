// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Desktop.Scenes.ServerSelect.View;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;

namespace Ascendance.Desktop.Scenes.ServerSelect;

[DynamicLoad]
public sealed class ServerSelectScene : BaseScene
{
    public ServerSelectScene() : base(SceneConstants.ServerSelect)
    {
    }

    protected override void LoadObjects()
    {
        HeaderView title = new();
        BackdropView background = new();
        ServerSelectView serverInfo = new();

        serverInfo.BackRequested += () =>
            SceneManager.Instance.ScheduleSceneChange(
                ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene);

        base.AddObject(title);
        base.AddObject(background);
        base.AddObject(serverInfo);
    }
}

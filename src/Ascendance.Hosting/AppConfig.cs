// Copyright (c) 2026 Ascendance Team. All rights reserved.

using Ascendance.Contracts.Protocol;
using Nalix.Common.Diagnostics;
using Nalix.Common.Messaging.Packets.Abstractions;
using Nalix.Framework.Injection;
using Nalix.Logging;
using Nalix.Shared.Messaging.Catalog;

namespace Ascendance.Hosting;

public static class AppConfig
{
    static AppConfig()
    {
        if (InstanceManager.Instance.GetExistingInstance<ILogger>() == null)
        {
            InstanceManager.Instance.Register<ILogger>(NLogix.Host.Instance);
        }

        // 1) Build packet catalog.
        PacketCatalogFactory factory = new();

        // REGISTER packets here (single source of truth).
        _ = factory.RegisterPacket<ResponsePacket>();
        _ = factory.RegisterPacket<CredentialsPacket>();
        //_ = factory.RegisterPacket<CredsUpdatePacket>();

        IPacketCatalog catalog = factory.CreateCatalog();

        // 2) Expose catalog through your current service locator.
        InstanceManager.Instance.Register<IPacketCatalog>(catalog);
    }
}

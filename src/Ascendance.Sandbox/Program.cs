// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;

namespace Ascendance.Sandbox;

public static class Program
{
    public static void Main()
    {
        GraphicsEngine.Run();

        System.Console.WriteLine("Press Enter to exit...");
        System.Console.ReadLine();
    }
}

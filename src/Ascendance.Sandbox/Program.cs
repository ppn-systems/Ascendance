// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using SFML.Graphics;

namespace Ascendance.Sandbox;

public static class Program
{
    private const System.String IconBase64 =
        "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";

    public static void Main()
    {
        Image icon = LoadingImage();
        GraphicsEngine.SetWindowIcon(icon);
        GraphicsEngine.Run();

        System.Console.WriteLine("Press Enter to exit...");
        System.Console.ReadLine();
    }

    public static Image LoadingImage()
    {
        System.Byte[] bytes = System.Convert.FromBase64String(IconBase64);
        using System.IO.MemoryStream ms = new(bytes);
        return new Image(ms);
    }
}

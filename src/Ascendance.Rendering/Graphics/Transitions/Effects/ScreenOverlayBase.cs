using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Engine.Abstractions;
using SFML.Graphics;
using SFML.System;
using System;

namespace Ascendance.Rendering.Graphics.Transitions.Effects;

internal abstract class ScreenOverlayBase(Color color) : ITransitionDrawable
{
    protected readonly Vector2f Size = new(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);
    protected readonly Color BaseColor = color;

    public abstract void Update(Single p, Boolean closing);
    public abstract Drawable GetDrawable();

    protected static Byte LerpByte(Byte a, Byte b, Single t)
        => (Byte)(a + ((b - a) * Math.Clamp(t, 0f, 1f)));
}
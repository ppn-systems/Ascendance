// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Physics.Physics;

/// <summary>
/// Manages and resolves collisions between rigid bodies.
/// </summary>
public class CollisionManager
{
    /// <summary>
    /// Gets the list of bodies to process collision for.
    /// </summary>
    public System.Collections.Generic.List<RigidBody> Bodies { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionManager"/> class.
    /// </summary>
    /// <param name="bodies">List of rigid bodies for collision checking.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public CollisionManager(System.Collections.Generic.List<RigidBody> bodies) => Bodies = bodies;

    /// <summary>
    /// Performs collision checks and resolves all collisions.
    /// </summary>
    public void HandleCollisions()
    {
        System.Int32 count = Bodies.Count;
        for (System.Int32 i = 0; i < count - 1; i++)
        {
            for (System.Int32 j = i + 1; j < count; j++)
            {
                var a = Bodies[i];
                var b = Bodies[j];
                if (a.Collider.IsColliding(b.Collider))
                {
                    ResolveCollision(a, b);
                }
            }
        }
    }

    /// <summary>
    /// Resolves a collision event between two rigid bodies.
    /// </summary>
    /// <param name="a">The first rigid body.</param>
    /// <param name="b">The second rigid body.</param>
    private static void ResolveCollision(RigidBody a, RigidBody b)
    {
        // Skip collision response if both bodies are static
        if (a.IsStatic && b.IsStatic)
        {
            return;
        }

        // Simple velocity reversal for demonstration
        if (!a.IsStatic)
        {
            a.Velocity = -a.Velocity * 0.7f; // Some energy loss (coefficient of restitution)
        }

        if (!b.IsStatic)
        {
            b.Velocity = -b.Velocity * 0.7f;
        }
    }
}
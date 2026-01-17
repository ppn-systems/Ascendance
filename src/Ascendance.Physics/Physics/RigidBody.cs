// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Physics.Colliders;
using SFML.System;

namespace Ascendance.Physics.Physics;

/// <summary>
/// Represents a physical object with a collider and basic physical properties.
/// </summary>
public class RigidBody
{
    /// <summary>
    /// Gets the collider of this rigid body.
    /// </summary>
    public ICollider Collider { get; protected set; }

    /// <summary>
    /// Gets or sets the mass. Mass &lt;= 0 means the object is static (immovable).
    /// </summary>
    public System.Single Mass { get; set; }

    /// <summary>
    /// Gets or sets the linear velocity.
    /// </summary>
    public Vector2f Velocity { get; set; }

    /// <summary>
    /// Gets or sets the total force applied for this simulation step.
    /// </summary>
    public Vector2f Force { get; set; }

    /// <summary>
    /// Gets whether this rigid body is static (immovable).
    /// </summary>
    public System.Boolean IsStatic => Mass <= 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBody"/> class.
    /// </summary>
    /// <param name="collider">The collider.</param>
    /// <param name="mass">The mass of the object.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
    public RigidBody(ICollider collider, System.Single mass)
    {
        Collider = collider;
        Mass = mass;
        Velocity = new Vector2f(0, 0);
        Force = new Vector2f(0, 0);
    }

    /// <summary>
    /// Applies a force to the rigid body.
    /// </summary>
    /// <param name="force">The force to apply.</param>
    public void ApplyForce(Vector2f force) => Force += force;

    /// <summary>
    /// Updates the physics state via Euler integration.
    /// </summary>
    /// <param name="deltaTime">The time step, in seconds.</param>
    public void Integrate(System.Single deltaTime)
    {
        if (IsStatic)
        {
            return;
        }

        Vector2f acceleration = new(Force.X / Mass, Force.Y / Mass);
        Velocity += acceleration * deltaTime;
        Collider.Position += Velocity * deltaTime;
        Force = new Vector2f(0, 0); // Reset force after each integration step
    }
}
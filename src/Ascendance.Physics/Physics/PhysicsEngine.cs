// Copyright (c) 2025 PPN Corporation. All rights reserved.

using SFML.System;

namespace Ascendance.Physics.Physics;

/// <summary>
/// Handles the main simulation loop and physical updates for all rigid bodies.
/// </summary>
public class PhysicsEngine
{
    /// <summary>
    /// Gets the list of all rigid bodies managed by the engine.
    /// </summary>
    public System.Collections.Generic.List<RigidBody> Bodies { get; } = [];

    /// <summary>
    /// Gets or sets the gravity acceleration vector.
    /// </summary>
    public Vector2f Gravity { get; set; } = new(0, 500f);

    /// <summary>
    /// Advances the physics simulation and updates all rigid bodies.
    /// </summary>
    /// <param name="deltaTime">The time step, in seconds.</param>
    public void Update(System.Single deltaTime)
    {
        foreach (RigidBody body in Bodies)
        {
            if (!body.IsStatic)
            {
                body.ApplyForce(Gravity * body.Mass);
            }
            body.Integrate(deltaTime);
        }
    }

    /// <summary>
    /// Adds a rigid body to the simulation.
    /// </summary>
    /// <param name="body">The rigid body to add.</param>
    public void AddBody(RigidBody body) => Bodies.Add(body);
}
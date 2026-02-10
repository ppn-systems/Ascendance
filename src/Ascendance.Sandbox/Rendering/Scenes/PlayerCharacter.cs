// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Tiles;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Sandbox.Rendering.Scenes;

/// <summary>
/// Simple player character with tile collision.
/// Nhân vật player đơn giản với va chạm tile.
/// </summary>
internal sealed class PlayerCharacter : SpriteObject
{
    private const System.Single MoveSpeed = 150f;
    private readonly TileMap _tileMap;
    private Vector2f _velocity;

    public Vector2f Position
    {
        get => Sprite.Position;
        set => Sprite.Position = value;
    }

    public PlayerCharacter(TileMap tileMap) : base(CreatePlayerTexture())
    {
        _tileMap = tileMap;
        _velocity = new Vector2f(0, 0);

        // Center origin
        FloatRect bounds = Sprite.GetLocalBounds();
        Sprite.Origin = new Vector2f(bounds.Width / 2f, bounds.Height / 2f);
    }

    private static Texture CreatePlayerTexture()
    {
        // Create a simple colored rectangle as player sprite
        // Tạo hình chữ nhật màu đơn giản làm sprite player
        SFML.Graphics.RenderTexture renderTexture = new(32, 32);
        renderTexture.Clear(new Color(255, 100, 100)); // Red player

        RectangleShape rect = new(new Vector2f(28, 28))
        {
            Position = new Vector2f(2, 2),
            FillColor = new Color(200, 50, 50),
            OutlineColor = Color.White,
            OutlineThickness = 2
        };

        renderTexture.Draw(rect);
        renderTexture.Display();

        return new Texture(renderTexture.Texture);
    }

    public override void Update(System.Single deltaTime)
    {
        if (!base.IsEnabled)
        {
            return;
        }

        // Input handling
        _velocity = new Vector2f(0, 0);

        if (KeyboardManager.Instance.IsKeyDown(Keyboard.Key.W) || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Up))
        {
            _velocity.Y -= MoveSpeed;
        }
        if (KeyboardManager.Instance.IsKeyDown(Keyboard.Key.S) || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Down))
        {
            _velocity.Y += MoveSpeed;
        }
        if (KeyboardManager.Instance.IsKeyDown(Keyboard.Key.A) || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Left))
        {
            _velocity.X -= MoveSpeed;
        }
        if (KeyboardManager.Instance.IsKeyDown(Keyboard.Key.D) || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Right))
        {
            _velocity.X += MoveSpeed;
        }

        // Apply movement with collision
        Vector2f currentPos = Position;
        Vector2f targetPos = currentPos + (_velocity * deltaTime);

        if (_tileMap != null)
        {
            // Check collision with "Collision" layer
            FloatRect playerBounds = new(
                targetPos.X - 16,
                targetPos.Y - 16,
                32,
                32);

            if (!TileCollider.CheckCollision(_tileMap, "Collision", playerBounds))
            {
                Position = targetPos;
            }
            else
            {
                // Try sliding along walls
                Vector2f resolved = TileCollider.ResolveCollision(
                    _tileMap,
                    "Collision",
                    currentPos,
                    targetPos,
                    new Vector2f(32, 32));

                Position = resolved;
            }
        }
        else
        {
            Position = targetPos;
        }

        base.Update(deltaTime);
    }
}
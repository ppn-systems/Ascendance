// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Colliders;
using Ascendance.Movement;
using Ascendance.Rendering.Animation;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Input;
using Ascendance.Shared.Enums;
using Ascendance.Tiled;
using Ascendance.Tiled.Layers;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Characters;

/// <summary>
/// Represents the player character in a 2.5D top-down game (Stardew Valley style).
/// </summary>
/// <remarks>
/// <para>
/// Handles player input, movement, collision detection, and animation state management.
/// </para>
/// <para>
/// Features:
/// - 4-directional movement (WASD/Arrow keys)
/// - Walk/Run modes (Shift to run)
/// - Tile-based collision detection with TMX tile map
/// - Animated sprite with 4-directional walk cycles (15x31 pixels, 4 frames per direction)
/// - Camera following (optional)
/// - Circular collision for smooth 2.5D movement
/// </para>
/// <para>
/// Expected sprite sheet layout:
/// <code>
/// Row 0: Down  [Idle] [Walk1] [Walk2] [Walk3]
/// Row 2: Right [Idle] [Walk1] [Walk2] [Walk3]
/// Row 3: Up    [Idle] [Walk1] [Walk2] [Walk3]
/// Row 4: Left  [Idle] [Walk1] [Walk2] [Walk3]
/// </code>
/// </para>
/// </remarks>
public sealed class Character : AnimatedSprite
{
    #region Constants

    private const System.Single COLLISION_RADIUS = 8f; // Circular collision for smooth movement

    #endregion Constants

    #region Fields

    private readonly KeyboardManager _keyboard;
    private readonly MovementController _movementController;
    private readonly CharacterController _animationController;

    private PlayerState _state;
    private Direction2D _direction;

    private System.String _collisionLayerName = "collision";

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the current world position of the player.
    /// </summary>
    /// <remarks>
    /// Setting this property updates the sprite, collider, and movement controller positions.
    /// </remarks>
    public Vector2f Position
    {
        get => this.Sprite.Position;
        set
        {
            this.Sprite.Position = value;
            this.Collider.Position = value;
            _movementController.Position = value;
        }
    }

    /// <summary>
    /// Gets the current state of the player.
    /// </summary>
    public PlayerState State => _state;

    /// <summary>
    /// Gets the current facing direction of the player.
    /// </summary>
    public Direction2D Direction => _direction;

    /// <summary>
    /// Gets or sets the TMX map used for collision detection.
    /// </summary>
    /// <remarks>
    /// Set this to enable tile-based collision detection with TMX layers.
    /// If null, the player can move freely without collision.
    /// </remarks>
    public TmxMap TmxMap { get; set; }

    /// <summary>
    /// Gets or sets the name of the collision layer in the TMX map.
    /// </summary>
    /// <remarks>
    /// Default: "collision". Set this to match your TMX map's collision layer name.
    /// This layer should contain tiles marked as collidable.
    /// </remarks>
    public System.String CollisionLayerName
    {
        get => _collisionLayerName;
        set => _collisionLayerName = value?.ToLowerInvariant() ?? System.String.Empty;
    }

    /// <summary>
    /// Gets or sets the camera that follows the player.
    /// </summary>
    /// <remarks>
    /// If set, the camera will automatically center on the player's position each frame.
    /// The camera position is updated after movement and collision resolution.
    /// </remarks>
    public Camera2D Camera { get; set; }

    /// <summary>
    /// Gets the player's collision bounds as a rectangle (for debugging/rendering).
    /// </summary>
    /// <remarks>
    /// Returns a <see cref="FloatRect"/> representing the circular collider as a bounding box.
    /// Useful for debug visualization or spatial queries.
    /// </remarks>
    public FloatRect CollisionBounds => new(
        this.Collider.Position.X - this.Collider.Radius,
        this.Collider.Position.Y - this.Collider.Radius,
        this.Collider.Radius * 2f,
        this.Collider.Radius * 2f);

    /// <summary>
    /// Gets the player's circular collider component.
    /// </summary>
    /// <remarks>
    /// Exposed for advanced collision detection or physics integration.
    /// </remarks>
    public CircleCollider Collider { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Character"/> class.
    /// </summary>
    /// <param name="texture">The player sprite sheet texture (15x31 per frame, 4x4 grid).</param>
    /// <param name="startPosition">The initial spawn position in world coordinates.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="texture"/> is null.</exception>
    public Character(Texture texture, Vector2f startPosition)
        : base(texture, new IntRect(0, 0, 15, 31), startPosition, new Vector2f(1f, 1f), 0f)
    {
        // Initialize input manager (singleton)
        _keyboard = KeyboardManager.Instance;

        // Initialize movement controller
        _movementController = new MovementController(startPosition);

        // Initialize animation controller
        _animationController = new CharacterController(this.SpriteAnimator);

        // Set default state
        _state = PlayerState.Idle;
        _direction = Direction2D.Down;

        // Initialize collision (circular for smooth 2.5D movement)
        // Set sprite origin to center-bottom (for 2.5D depth sorting)
        // Origin (7.5, 31) means center-X (15/2) and bottom-Y (31)
        this.Sprite.Scale = new Vector2f(1.6f, 1.6f);
        this.Sprite.Origin = new Vector2f(7.5f, 31f);
        this.Collider = new CircleCollider(startPosition, COLLISION_RADIUS);

        // Attach animation event handlers
        this.AttachAnimatorEventHandlers();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Updates the player state, input, movement, collision, and animation.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds.</param>
    /// <remarks>
    /// <para>
    /// Update order:
    /// 1. Process keyboard input
    /// 2. Determine player state (Idle/Walking/Running)
    /// 3. Apply movement via MovementController
    /// 4. Resolve TMX tile collisions
    /// 5. Update sprite position from collider
    /// 6. Update animation based on state and direction
    /// 7. Update animator (advance animation frames)
    /// 8. Update camera to follow player
    /// </para>
    /// </remarks>
    public override void Update(System.Single deltaTime)
    {
        if (!base.IsEnabled)
        {
            return;
        }

        // 1. Process input and determine movement direction
        Vector2f inputDirection = this.PROCESS_INPUT();

        // 2. Determine state and facing direction
        this.UPDATE_STATE(inputDirection);

        // 3. Apply movement via MovementController
        this.APPLY_MOVEMENT(inputDirection, deltaTime);

        // 4. Handle TMX tile collision
        this.HANDLE_TMX_COLLISION();

        // 5. Update sprite position from collider (authoritative position)
        this.Sprite.Position = this.Collider.Position;

        // 6. Update animation based on state and direction
        _animationController.UpdateAnimation(_state, _direction);

        // 7. Update animator (advance frames)
        base.Update(deltaTime);

        // 8. Update camera to follow player
        this.UPDATE_CAMERA();
    }

    /// <summary>
    /// Sets the player's position in world space.
    /// </summary>
    /// <param name="position">The new position.</param>
    /// <remarks>
    /// Updates sprite, collider, and movement controller positions simultaneously.
    /// Use this method instead of directly setting <see cref="Position"/> property
    /// when you need to ensure all components are synchronized.
    /// </remarks>
    public void SetPosition(Vector2f position)
    {
        this.Sprite.Position = position;
        this.Collider.Position = position;
        _movementController.Position = position;
    }

    /// <summary>
    /// Teleports the player to a specific tile coordinate in the TMX map.
    /// </summary>
    /// <param name="tileX">The tile X coordinate (column index).</param>
    /// <param name="tileY">The tile Y coordinate (row index).</param>
    /// <remarks>
    /// <para>
    /// Converts tile coordinates to world coordinates using the TMX map's tile size.
    /// Centers the player in the middle of the target tile.
    /// </para>
    /// <para>
    /// Does nothing if <see cref="TmxMap"/> is null.
    /// </para>
    /// </remarks>
    public void TeleportToTile(System.Int32 tileX, System.Int32 tileY)
    {
        if (this.TmxMap is null)
        {
            return;
        }

        Vector2f worldPos = TILE_TO_WORLD_CENTER(new Vector2i(tileX, tileY));
        this.SetPosition(worldPos);
    }

    /// <summary>
    /// Gets the current tile coordinate the player is standing on.
    /// </summary>
    /// <returns>
    /// The tile coordinate as a <see cref="Vector2i"/>, or <c>(-1, -1)</c> if no TMX map is set.
    /// </returns>
    public Vector2i GetCurrentTileCoordinate() => this.TmxMap is null ? new Vector2i(-1, -1) : WORLD_TO_TILE(this.Collider.Position);

    #endregion Public Methods

    #region Event Handlers

    /// <inheritdoc/>
    /// <remarks>
    /// Called when a looping animation wraps from the last frame to the first.
    /// Override this in derived classes to add footstep sounds, particle effects, etc.
    /// </remarks>
    protected override void OnAnimationLooped()
    {
        // TODO: Add footstep sound effect here
        // Example: AudioManager.Instance.PlaySound("footstep");
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Called when a non-looping animation reaches its end and stops.
    /// Override this in derived classes to handle tool use completion, action end, etc.
    /// </remarks>
    protected override void OnAnimationCompleted()
    {
        // TODO: Handle tool use completion, return to idle state, etc.
        // Example: if (_state == PlayerState.UsingTool) { _state = PlayerState.Idle; }
    }

    #endregion Event Handlers

    #region Private Methods - Coordinate Conversion

    /// <summary>
    /// Converts world position to tile coordinates.
    /// </summary>
    /// <param name="worldPos">World position in pixels.</param>
    /// <returns>Tile coordinate.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private Vector2i WORLD_TO_TILE(Vector2f worldPos)
    {
        System.Int32 tileX = (System.Int32)System.MathF.Floor(worldPos.X / TmxMap.TileWidth);
        System.Int32 tileY = (System.Int32)System.MathF.Floor(worldPos.Y / TmxMap.TileHeight);
        return new Vector2i(tileX, tileY);
    }

    /// <summary>
    /// Converts tile coordinates to world center position.
    /// </summary>
    /// <param name="tilePos">Tile coordinate.</param>
    /// <returns>World position at the center of the tile.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private Vector2f TILE_TO_WORLD_CENTER(Vector2i tilePos)
    {
        System.Single worldX = (tilePos.X * TmxMap.TileWidth) + (TmxMap.TileWidth * 0.5f);
        System.Single worldY = (tilePos.Y * TmxMap.TileHeight) + (TmxMap.TileHeight * 0.5f);
        return new Vector2f(worldX, worldY);
    }

    #endregion Private Methods - Coordinate Conversion

    #region Private Methods - Input

    /// <summary>
    /// Processes keyboard input and returns normalized movement direction.
    /// </summary>
    /// <returns>
    /// A normalized direction vector (length = 1.0 for movement, or zero vector if no input).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Supported keys:
    /// - Horizontal: A/D or Left/Right arrows
    /// - Vertical: W/S or Up/Down arrows
    /// </para>
    /// <para>
    /// Diagonal movement is normalized to prevent faster diagonal speed.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private Vector2f PROCESS_INPUT()
    {
        System.Single x = 0f;
        System.Single y = 0f;

        if (_keyboard.IsKeyDown(Keyboard.Key.A))
        {
            x--;
        }

        if (_keyboard.IsKeyDown(Keyboard.Key.D))
        {
            x++;
        }

        if (_keyboard.IsKeyDown(Keyboard.Key.W))
        {
            y--;
        }

        if (_keyboard.IsKeyDown(Keyboard.Key.S))
        {
            y++;
        }

        // Normalize diagonal movement to prevent faster diagonal speed
        if (x != 0f && y != 0f)
        {
            System.Single length = System.MathF.Sqrt((x * x) + (y * y));
            x /= length;
            y /= length;
        }

        return new Vector2f(x, y);
    }

    #endregion Private Methods - Input

    #region Private Methods - State

    /// <summary>
    /// Updates player state and facing direction based on input.
    /// </summary>
    /// <param name="inputDirection">The normalized input direction vector.</param>
    /// <remarks>
    /// <para>
    /// State determination logic:
    /// - No movement input → <see cref="PlayerState.Idle"/>
    /// - Movement + Shift held → <see cref="PlayerState.Running"/>
    /// - Movement only → <see cref="PlayerState.Walking"/>
    /// </para>
    /// <para>
    /// Facing direction is only updated when the player is moving.
    /// This preserves the last facing direction when transitioning to idle.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void UPDATE_STATE(Vector2f inputDirection)
    {
        System.Boolean isMoving = inputDirection.X != 0f || inputDirection.Y != 0f;
        System.Boolean isRunning = _keyboard.IsKeyDown(Keyboard.Key.LShift) || _keyboard.IsKeyDown(Keyboard.Key.RShift);

        // Update state
        _state = !isMoving ? PlayerState.Idle : isRunning ? PlayerState.Running : PlayerState.Walking;

        // Update facing direction (only if moving)
        if (isMoving)
        {
            _direction = GET_DIRECTION_FROM_INPUT(inputDirection);
        }
    }

    /// <summary>
    /// Converts input vector to the closest cardinal direction.
    /// </summary>
    /// <param name="input">The input direction vector.</param>
    /// <returns>The closest <see cref="Direction2D"/> (Down, Up, Left, or Right).</returns>
    /// <remarks>
    /// <para>
    /// Prioritizes horizontal movement over vertical movement for 4-directional sprites.
    /// This is common in top-down games to avoid visual ambiguity when moving diagonally.
    /// </para>
    /// <para>
    /// Algorithm:
    /// - If |X| &gt; |Y|: Use Left or Right
    /// - Otherwise: Use Down or Up
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static Direction2D GET_DIRECTION_FROM_INPUT(Vector2f input)
    {
        // Prioritize horizontal movement for 4-directional sprite
        return System.MathF.Abs(input.X) > System.MathF.Abs(input.Y)
            ? input.X > 0 ? Direction2D.Right : Direction2D.Left
            : input.Y > 0 ? Direction2D.Down : Direction2D.Up;
    }

    #endregion Private Methods - State

    #region Private Methods - Camera

    /// <summary>
    /// Updates camera to follow player position.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Centers the camera on the player's collider position using <see cref="Camera2D.SetCenter"/>.
    /// </para>
    /// <para>
    /// Does nothing if <see cref="Camera"/> is null.
    /// </para>
    /// <para>
    /// For smooth camera following, consider using <see cref="Camera2D.Follow"/> instead
    /// with a smoothing factor (e.g., 0.1f).
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void UPDATE_CAMERA()
    {
        if (this.Camera is null)
        {
            return;
        }

        // Option 1: Hard follow (instant centering)
        this.Camera.SetCenter(this.Collider.Position);

        // Option 2: Smooth follow (uncomment if you prefer smooth camera)
        // _camera.Follow(_collider.Position, smooth: 0.1f);
    }

    #endregion Private Methods - Camera

    #region Private Methods - Movement

    /// <summary>
    /// Applies movement using MovementController.
    /// </summary>
    /// <param name="direction">The input direction vector (normalized).</param>
    /// <param name="deltaTime">Time delta in seconds.</param>
    /// <remarks>
    /// <para>
    /// Uses <see cref="MovementController"/> with <see cref="MovementType.Walk"/>.
    /// Speed is controlled internally by <see cref="WalkMovement"/> or <see cref="RunMovement"/>.
    /// </para>
    /// <para>
    /// Note: If you need different speeds for walk/run, you should modify <see cref="MovementController"/>
    /// to switch between <see cref="WalkMovement"/> and <see cref="RunMovement"/> instances,
    /// or pass speed as a parameter.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void APPLY_MOVEMENT(Vector2f direction, System.Single deltaTime)
    {
        if (_state == PlayerState.Idle)
        {
            _movementController.SetMovement(MovementType.None, new Vector2f(0, 0));
        }
        else if (_state == PlayerState.Running)
        {
            _movementController.SetMovement(MovementType.Run, direction);
        }
        else
        {
            // Use Walk movement type
            _movementController.SetMovement(MovementType.Walk, direction);
        }

        _movementController.Update(deltaTime);
    }

    #endregion Private Methods - Movement

    #region Private Methods - TMX Collision

    /// <summary>
    /// Handles TMX tile-based collision detection and resolution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses the TMX map's tile layers to check for collision tiles.
    /// Resolves collision by testing X and Y movement separately.
    /// </para>
    /// <para>
    /// If <see cref="TmxMap"/> or <see cref="CollisionLayerName"/> is not set,
    /// the player moves freely without collision detection.
    /// </para>
    /// <para>
    /// The collider position is authoritative and is used to update the sprite position
    /// after collision resolution.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void HANDLE_TMX_COLLISION()
    {
        if (this.TmxMap is null || System.String.IsNullOrEmpty(this.CollisionLayerName))
        {
            this.Collider.Position = _movementController.Position;
            return;
        }

        Vector2f currentPosition = this.Collider.Position;
        Vector2f targetPosition = _movementController.Position;
        Vector2f colliderSize = new(this.Collider.Radius * 2f, this.Collider.Radius * 2f);
        Vector2f resolvedPosition = this.RESOLVE_TMX_COLLISION_SEPARATELY(currentPosition, targetPosition, colliderSize);

        this.Collider.Position = resolvedPosition;
        _movementController.Position = resolvedPosition;
    }

    /// <summary>
    /// Resolves TMX collision by testing X and Y axes separately.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private Vector2f RESOLVE_TMX_COLLISION_SEPARATELY(
        Vector2f currentPos,
        Vector2f targetPos,
        Vector2f size)
    {
        Vector2f resolvedPos = currentPos;

        // Test X-axis movement first
        Vector2f testX = new(targetPos.X, currentPos.Y);
        FloatRect boundsX = new(testX.X - (size.X * 0.5f), testX.Y - (size.Y * 0.5f), size.X, size.Y);

        if (!CHECK_TMX_COLLISION(boundsX))
        {
            resolvedPos.X = testX.X; // X-axis is clear
        }

        // Test Y-axis movement
        Vector2f testY = new(resolvedPos.X, targetPos.Y);
        FloatRect boundsY = new(testY.X - (size.X * 0.5f), testY.Y - (size.Y * 0.5f), size.X, size.Y);

        if (!CHECK_TMX_COLLISION(boundsY))
        {
            resolvedPos.Y = testY.Y; // Y-axis is clear
        }

        return resolvedPos;
    }

    /// <summary>
    /// Checks if the given bounds collide with any tiles in the collision layer.
    /// </summary>
    /// <param name="bounds">The bounding rectangle to check.</param>
    /// <returns>True if collision detected, false otherwise.</returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private System.Boolean CHECK_TMX_COLLISION(FloatRect bounds)
    {
        // Find the collision layer by name
        TmxLayer collisionLayer = null;
        foreach (var layer in TmxMap.TileLayers)
        {
            if (System.String.Equals(layer.Name, CollisionLayerName, System.StringComparison.OrdinalIgnoreCase))
            {
                collisionLayer = layer;
                break;
            }
        }

        if (collisionLayer is null || !collisionLayer.Visible)
        {
            return false; // No collision layer found or not visible
        }

        // Calculate tile bounds that overlap with the character bounds
        System.Int32 startTileX = (System.Int32)System.MathF.Floor(bounds.Left / TmxMap.TileWidth);
        System.Int32 endTileX = (System.Int32)System.MathF.Floor((bounds.Left + bounds.Width) / TmxMap.TileWidth);
        System.Int32 startTileY = (System.Int32)System.MathF.Floor(bounds.Top / TmxMap.TileHeight);
        System.Int32 endTileY = (System.Int32)System.MathF.Floor((bounds.Top + bounds.Height) / TmxMap.TileHeight);

        // Check each overlapping tile for collision
        for (System.Int32 tileY = startTileY; tileY <= endTileY; tileY++)
        {
            for (System.Int32 tileX = startTileX; tileX <= endTileX; tileX++)
            {
                // Check if this tile position has a collision tile
                foreach (var tile in collisionLayer.Tiles)
                {
                    if (tile.X == tileX && tile.Y == tileY && tile.Gid > 0)
                    {
                        return true; // Collision detected
                    }
                }
            }
        }

        return false; // No collision
    }

    #endregion Private Methods - TMX Collision
}
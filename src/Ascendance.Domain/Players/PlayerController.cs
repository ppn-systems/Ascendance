// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Animation;
using Ascendance.Rendering.Utilities;
using Ascendance.Shared.Enums;
using SFML.Graphics;

namespace Ascendance.Domain.Players;

/// <summary>
/// Manages player animation state transitions and frame selection based on player state and direction.
/// </summary>
/// <remarks>
/// <para>
/// Handles animation switching for a 2.5D top-down player sprite with 4 directional walk cycles.
/// </para>
/// <para>
/// Expected sprite sheet layout (15x31 pixels per frame, 4 frames per direction):
/// <code>
/// Row 0: Down  [Idle] [Walk1] [Walk2] [Walk3]
/// Row 2: Right [Idle] [Walk1] [Walk2] [Walk3]
/// Row 3: Up    [Idle] [Walk1] [Walk2] [Walk3]
/// Row 4: Left  [Idle] [Walk1] [Walk2] [Walk3]
/// </code>
/// </para>
/// </remarks>
public sealed class PlayerController
{
    #region Constants

    private const System.Int32 SPRITE_WIDTH = 16;
    private const System.Int32 SPRITE_HEIGHT = 32;
    private const System.Int32 DIRECTION_COUNT = 4;
    private const System.Int32 FRAMES_PER_DIRECTION = 4;

    private const System.Single RUN_FRAME_TIME = 0.10f;   // 100ms per frame (faster animation)
    private const System.Single WALK_FRAME_TIME = 0.15f;  // 150ms per frame
    private const System.Single IDLE_FRAME_TIME = 0.25f;  // 250ms per frame (slower for idle bob)

    #endregion Constants

    #region Fields

    private readonly Animator _animator;
    private readonly System.Collections.Generic.Dictionary<Direction2D, IntRect> _idleFrames;
    private readonly System.Collections.Generic.Dictionary<Direction2D, System.Collections.Generic.List<IntRect>> _walkFrames;

    private PlayerState _currentState;
    private Direction2D _currentDirection;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the current animation state of the player.
    /// </summary>
    public PlayerState CurrentState => _currentState;

    /// <summary>
    /// Gets the current facing direction of the player.
    /// </summary>
    public Direction2D CurrentDirection => _currentDirection;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerController"/> class.
    /// </summary>
    /// <param name="animator">The animator component to control.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="animator"/> is null.</exception>
    public PlayerController(Animator animator)
    {
        _animator = animator ?? throw new System.ArgumentNullException(nameof(animator));

        _currentState = PlayerState.Idle;
        _currentDirection = Direction2D.Down;
        _idleFrames = new System.Collections.Generic.Dictionary<Direction2D, IntRect>(DIRECTION_COUNT);
        _walkFrames = new System.Collections.Generic.Dictionary<Direction2D, System.Collections.Generic.List<IntRect>>(DIRECTION_COUNT);

        this.INITIALIZE_FRAMES();
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Updates the animation state based on player state and direction.
    /// </summary>
    /// <param name="state">The desired player state.</param>
    /// <param name="direction">The desired facing direction.</param>
    /// <remarks>
    /// Automatically switches animation frames and playback speed based on state changes.
    /// Idle state plays only the first frame of the walk cycle.
    /// </remarks>
    public void UpdateAnimation(PlayerState state, Direction2D direction)
    {
        System.Boolean stateChanged = state != _currentState;
        System.Boolean directionChanged = direction != _currentDirection;

        if (!stateChanged && !directionChanged)
        {
            return; // No change needed
        }

        _currentState = state;
        _currentDirection = direction;

        switch (state)
        {
            case PlayerState.Idle:
                this.PLAY_IDLE_ANIMATION(direction);
                break;

            case PlayerState.Walking:
                this.PLAY_WALK_ANIMATION(direction, WALK_FRAME_TIME);
                break;

            case PlayerState.Running:
                this.PLAY_WALK_ANIMATION(direction, RUN_FRAME_TIME);
                break;

            case PlayerState.UsingTool:
            case PlayerState.Interacting:
                // Future: Implement tool/interaction animations
                this.PLAY_IDLE_ANIMATION(direction);
                break;

            default:
                this.PLAY_IDLE_ANIMATION(direction);
                break;
        }
    }

    /// <summary>
    /// Forces the animator to stop and reset to idle state.
    /// </summary>
    public void Stop()
    {
        _animator.Stop();
        _currentState = PlayerState.Idle;
    }

    /// <summary>
    /// Gets the current animation frame index.
    /// </summary>
    /// <returns>The zero-based frame index, or -1 if no animation is loaded.</returns>
    public System.Int32 GetCurrentFrameIndex() => _animator.CurrentFrameIndex;

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Initializes all animation frames by cutting the sprite sheet.
    /// </summary>
    private void INITIALIZE_FRAMES()
    {
        // Cut entire sprite sheet: 4 rows x 4 columns
        System.Collections.Generic.List<IntRect> allFrames = SpriteSheetCutter.CutGrid(
            SPRITE_WIDTH,
            SPRITE_HEIGHT,
            FRAMES_PER_DIRECTION,
            DIRECTION_COUNT,
            spacing: 0,
            margin: 0);

        // Map frames to directions
        for (System.Int32 dir = 0; dir < DIRECTION_COUNT; dir++)
        {
            Direction2D direction = (Direction2D)dir;
            System.Int32 rowStartIndex = dir * FRAMES_PER_DIRECTION;

            // Extract 4 frames for this direction
            System.Collections.Generic.List<IntRect> directionFrames = allFrames.GetRange(rowStartIndex, FRAMES_PER_DIRECTION);

            _walkFrames[direction] = directionFrames;
            _idleFrames[direction] = directionFrames[0]; // First frame is idle pose
        }
    }

    /// <summary>
    /// Plays the idle animation for the specified direction.
    /// </summary>
    /// <param name="direction">The facing direction.</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void PLAY_IDLE_ANIMATION(Direction2D direction)
    {
        // Idle uses only the first frame (no animation)
        IntRect idleFrame = _idleFrames[direction];
        _animator.SetFrames([idleFrame]);
        _animator.SetFrameTime(IDLE_FRAME_TIME);
        _animator.Loop = true;
        _animator.Play();
    }

    /// <summary>
    /// Plays the walk/run animation for the specified direction.
    /// </summary>
    /// <param name="direction">The facing direction.</param>
    /// <param name="frameTime">The duration per frame (shorter = faster animation).</param>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void PLAY_WALK_ANIMATION(Direction2D direction, System.Single frameTime)
    {
        System.Collections.Generic.List<IntRect> frames = _walkFrames[direction];
        _animator.SetFrames(frames);
        _animator.SetFrameTime(frameTime);
        _animator.Loop = true;
        _animator.Play();
    }

    #endregion Private Methods
}
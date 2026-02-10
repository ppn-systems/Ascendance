// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Domain.Tiles;
using Ascendance.Rendering.Attributes;
using Ascendance.Rendering.Camera;
using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Scenes;
using Nalix.Framework.Configuration;
using Nalix.Logging.Extensions;
using SFML.System;

namespace Ascendance.Sandbox.Scenes;

/// <summary>
/// Test scene for player character with tile map collision and camera following.
/// Scene test cho nhân vật người chơi với va chạm tile map và camera theo dõi.
/// </summary>
/// <remarks>
/// <para>
/// This scene demonstrates:
/// Scene này minh họa:
/// </para>
/// <list type="bullet">
/// <item>Loading a tile map from TMX format (Tải tile map từ định dạng TMX)</item>
/// <item>Creating a player with physics-based movement (Tạo người chơi với chuyển động dựa trên vật lý)</item>
/// <item>Camera following the player within map bounds (Camera theo dõi người chơi trong giới hạn map)</item>
/// <item>Collision detection between player and tiles (Phát hiện va chạm giữa người chơi và tile)</item>
/// </list>
/// </remarks>
[DynamicLoad]
internal sealed class PlayerTestScene : BaseScene
{
    #region Fields

    /// <summary>
    /// The tile map containing level geometry and collision data.
    /// Tile map chứa hình học cấp độ và dữ liệu va chạm.
    /// </summary>
    private TileMap _tileMap;

    /// <summary>
    /// The camera that follows the player character.
    /// Camera theo dõi nhân vật người chơi.
    /// </summary>
    private Camera2D _camera;

    /// <summary>
    /// The player character controlled by keyboard input.
    /// Nhân vật người chơi được điều khiển bởi input bàn phím.
    /// </summary>
    private Player _player;

    /// <summary>
    /// Camera follow smoothing factor (0 = instant, 1 = very smooth).
    /// Hệ số làm mượt theo dõi của camera (0 = tức thì, 1 = rất mượt).
    /// </summary>
    private const System.Single CameraFollowSpeed = 0.1f;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerTestScene"/> class.
    /// Khởi tạo một thể hiện mới của lớp <see cref="PlayerTestScene"/>.
    /// </summary>
    public PlayerTestScene() : base(ConfigurationManager.Instance.Get<GraphicsConfig>().MainScene) => GraphicsEngine.Instance.FrameUpdate += Update;

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Loads all scene objects including tile map, camera, and player.
    /// Tải tất cả các đối tượng scene bao gồm tile map, camera và người chơi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Loading order:
    /// Thứ tự tải:
    /// </para>
    /// <list type="number">
    /// <item>Setup camera with screen-sized viewport (Thiết lập camera với viewport kích thước màn hình)</item>
    /// <item>Load tile map from TMX file (Tải tile map từ file TMX)</item>
    /// <item>Configure camera bounds to match map size (Cấu hình giới hạn camera khớp với kích thước map)</item>
    /// <item>Create player at spawn position (Tạo người chơi tại vị trí spawn)</item>
    /// <item>Add all objects to scene rendering pipeline (Thêm tất cả đối tượng vào pipeline rendering của scene)</item>
    /// </list>
    /// </remarks>
    protected override void LoadObjects()
    {
        "Loading PlayerTestScene...".Info(source: "PlayerTestScene");

        // Setup camera (Thiết lập camera)
        Vector2f screenSize = new(GraphicsEngine.ScreenSize.X, GraphicsEngine.ScreenSize.Y);
        _camera = new Camera2D(new Vector2f(0, 0), screenSize / 8);

        // Load tile map from TMX file (Tải tile map từ file TMX)
        _tileMap = TmxMapLoader.Load("res/maps/test_map.tmx");

        if (_tileMap is null)
        {
            "Failed to load tile map!".Error(source: "PlayerTestScene");
            return;
        }

        _tileMap.Camera = _camera;
        _tileMap.SetZIndex(-10); // Draw map behind everything (Vẽ map phía sau mọi thứ)
        base.AddObject(_tileMap);

        // Set camera bounds to map size (Đặt giới hạn camera theo kích thước map)
        _camera.Bounds = new SFML.Graphics.FloatRect(0, 0, _tileMap.PixelWidth, _tileMap.PixelHeight);

        $"Tile map loaded: {_tileMap.PixelWidth}x{_tileMap.PixelHeight} pixels".Info(source: "PlayerTestScene");

        // Create player at spawn position (Tạo người chơi tại vị trí spawn)
        Vector2f spawnPosition = FindSpawnPosition();
        _player = new Player(_tileMap)
        {
            Position = spawnPosition
        };
        _player.SetZIndex(0);
        base.AddObject(_player);

        $"Player spawned at: {spawnPosition.X}, {spawnPosition.Y}".Info(source: "PlayerTestScene");

        "PlayerTestScene loaded successfully!".Info(source: "PlayerTestScene");
    }

    /// <summary>
    /// Updates the scene, including camera following logic.
    /// Cập nhật scene, bao gồm logic theo dõi camera.
    /// </summary>
    /// <param name="deltaTime">
    /// The time elapsed since the last frame in seconds.
    /// Thời gian trôi qua kể từ khung hình cuối cùng tính bằng giây.
    /// </param>
    public void Update(System.Single deltaTime)
    {
        if (_player is not null && _camera is not null)
        {
            UpdateCameraFollow();
        }
    }

    /// <summary>
    /// Updates camera position to smoothly follow the player.
    /// Cập nhật vị trí camera để theo dõi người chơi một cách mượt mà.
    /// </summary>
    /// <remarks>
    /// Uses linear interpolation (lerp) for smooth camera movement.
    /// The camera is constrained within the map bounds to prevent showing areas outside the map.
    /// Sử dụng nội suy tuyến tính (lerp) để chuyển động camera mượt mà.
    /// Camera bị hạn chế trong giới hạn map để ngăn hiển thị các khu vực bên ngoài map.
    /// </remarks>
    private void UpdateCameraFollow()
    {
        Vector2f playerPos = _player.Position;
        Vector2f currentCameraPos = _camera.SFMLView.Center;

        // Smooth camera follow using lerp (Theo dõi camera mượt mà bằng lerp)
        Vector2f targetPos = new(
            Lerp(currentCameraPos.X, playerPos.X, CameraFollowSpeed),
            Lerp(currentCameraPos.Y, playerPos.Y, CameraFollowSpeed)
        );

        _camera.SFMLView.Center = targetPos;
    }

    /// <summary>
    /// Finds a valid spawn position for the player in the tile map.
    /// Tìm vị trí spawn hợp lệ cho người chơi trong tile map.
    /// </summary>
    /// <returns>
    /// A spawn position vector. Returns (100, 100) if no spawn tile is found.
    /// Vector vị trí spawn. Trả về (100, 100) nếu không tìm thấy tile spawn.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method searches for a tile with the custom property "spawn" set to "true" or "player".
    /// If found, it returns the center position of that tile.
    /// Phương thức này tìm kiếm một tile với thuộc tính tùy chỉnh "spawn" được đặt thành "true" hoặc "player".
    /// Nếu tìm thấy, nó trả về vị trí trung tâm của tile đó.
    /// </para>
    /// <para>
    /// To set a spawn point in Tiled:
    /// Để đặt điểm spawn trong Tiled:
    /// </para>
    /// <list type="number">
    /// <item>Select a tile in the "Objects" or "Spawn" layer (Chọn một tile trong lớp "Objects" hoặc "Spawn")</item>
    /// <item>Add a custom property: name="spawn", value="true" (Thêm thuộc tính tùy chỉnh: name="spawn", value="true")</item>
    /// </list>
    /// </remarks>
    private Vector2f FindSpawnPosition()
    {
        // Try to find a spawn point in the tile map (Cố gắng tìm điểm spawn trong tile map)
        // Look for a tile with custom property "spawn" = "true"
        // (Tìm tile với thuộc tính tùy chỉnh "spawn" = "true")

        foreach (TileLayer layer in _tileMap.Layers)
        {
            for (System.Int32 y = 0; y < layer.Height; y++)
            {
                for (System.Int32 x = 0; x < layer.Width; x++)
                {
                    Tile tile = layer.GetTile(x, y);
                    if (tile.Properties is not null &&
                        tile.Properties.TryGetValue("spawn", out System.String spawnValue) &&
                        (spawnValue is "true" or "player"))
                    {
                        // Found spawn tile, return its center position
                        // (Tìm thấy tile spawn, trả về vị trí trung tâm của nó)
                        return _tileMap.TileToWorldCenter(new Vector2i(x, y));
                    }
                }
            }
        }

        // Default spawn position if no spawn tile found
        // (Vị trí spawn mặc định nếu không tìm thấy tile spawn)
        "No spawn point found in tile map, using default position (100, 100)".Warn(source: "PlayerTestScene");
        return new Vector2f(100, 100);
    }

    /// <summary>
    /// Performs linear interpolation between two values.
    /// Thực hiện nội suy tuyến tính giữa hai giá trị.
    /// </summary>
    /// <param name="start">The starting value (Giá trị bắt đầu).</param>
    /// <param name="end">The ending value (Giá trị kết thúc).</param>
    /// <param name="t">The interpolation factor (0 to 1) (Hệ số nội suy (0 đến 1)).</param>
    /// <returns>
    /// The interpolated value between <paramref name="start"/> and <paramref name="end"/>.
    /// Giá trị nội suy giữa <paramref name="start"/> và <paramref name="end"/>.
    /// </returns>
    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static System.Single Lerp(System.Single start, System.Single end, System.Single t) => start + ((end - start) * t);

    #endregion Methods
}
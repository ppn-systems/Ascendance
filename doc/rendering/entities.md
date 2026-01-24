# Tài liệu các lớp cơ bản trong `Ascendance.Rendering.Entities`

> Đây là nhóm các lớp nền quản lý đối tượng (object) trong scene/game của framework Ascendance, bao gồm các đối tượng render, sprite, animation và batch xử lý sprite.

---

## 1. `SceneObject`

**Lớp nền cho mọi đối tượng trong scene của game.**  
Quản lý vòng đời, pause/resume, enable/disable, tag, và việc spawn/destroy object thông qua scene manager.

### Thuộc tính chính

- **IsPaused**: Có bị tạm dừng hay không.
- **IsEnabled**: Có đang hoạt động hay không.
- **IsInitialized**: Đã được khởi tạo hay chưa.
- **IsPersistent**: Có giữ lại khi đổi scene không.

### Phương thức tiêu biểu

- **Initialize()**: Gọi khi khởi tạo, có thể ghi đè để setup logic riêng.
- **OnBeforeDestroy()**: Gọi trước khi bị huỷ, có thể xử lý dọn dẹp.
- **Update(deltaTime)**: Gọi mỗi frame, ghi đè cho logic cập nhật đối tượng.
- **AddTag(tag)** / **HasTag(tag)**: Gắn tag và kiểm tra tag cho object (thường để gắn định danh, nhóm).
- **Pause() / Resume() / Enable() / Disable()**: Quản lý trạng thái hoạt động.
- **Spawn() / Destroy()**: Đưa object vào hàng đợi spawn/destroy trong scene.
- **IsQueuedForSpawn()/IsQueuedForDestroy()**: Kiểm tra trạng thái hàng đợi.

---

## 2. `RenderObject` (Kế thừa từ `SceneObject`)

**Lớp nền cho các đối tượng có khả năng vẽ lên màn hình.**  
Quản lý Z-Index, hiển thị hoặc không, và phương thức để vẽ.

### Thuộc tính chính

- **IsVisible**: Đối tượng có được hiển thị không.

### Phương thức tiêu biểu

- **Draw(RenderTarget)**: Vẽ object lên đích nếu IsVisible=true.
- **Show() / Hide()**: Hiển thị hoặc ẩn object khỏi render.
- **SetZIndex(index)**: Thiết lập thứ tự vẽ (nhỏ vẽ trước, lớn vẽ sau).
- **CompareZIndex(r1, r2)**: So sánh hai object dựa trên Z-Index (để sort).

---

## 3. `SpriteObject` (Kế thừa từ `RenderObject`)

**Lớp nền cho các object render ra Sprite.**  
Cung cấp các constructor tiện lợi để khởi tạo Sprite với texture, vị trí, scale, rotation...

### Thuộc tính chính

- **Sprite**: Đối tượng Sprite liên kết với object.
- **GlobalBounds**: Bounding box của Sprite.

### Phương thức tiêu biểu

- Phương thức dựng với đầy đủ tuỳ chọn (texture, vị trí, scale, vùng cắt, rotation,...).
- **GetDrawable()** (sealed): Trả về Sprite để render.

---

## 4. `AnimatorObject` (Kế thừa từ `SpriteObject`)

**Lớp nền cho các sprite scene object có hoạt ảnh (animation).**  
Quản lý bằng SpriteAnimator, hỗ trợ setup animation từ danh sách frame hoặc grid.

### Thuộc tính chính

- **SpriteAnimator**: Quản lý hoạt ảnh cho Sprite.
- **IsAnimationPlaying**: Trạng thái đang chạy animation không.
- **FrameCount**: Tổng số frame animation hiện tại.
- **CurrentFrameIndex**: Frame animation hiện tại.

### Phương thức tiêu biểu

- **PlayAnimationFrames(frames, frameTime, loop)**: Chạy animation từ danh sách frame.
- **PlayAnimationFromGrid(cellW, cellH, cols, rows, frameTime, loop, startCol, startRow, count)**:  Khởi tạo frame từ grid chia đều trên spritesheet.
- **PlayAnimation() / PauseAnimation() / StopAnimation()**: Điều khiển trạng thái hoạt ảnh.
- **Update(deltaTime)**: Cập nhật hoạt ảnh mỗi frame.
- **Dispose()**: Giải phóng tài nguyên khi xong.
- **OnAnimationLooped() / OnAnimationCompleted()**: Có thể ghi đè để xử lý sự kiện animation.

---

## 5. `SpriteBatch<T>`

**Lớp hỗ trợ vẽ hàng loạt nhiều sprite trong 1 lần gọi draw (batch) hiệu suất cao.**  
Mỗi item trong batch có thể cấu hình vị trí, scale, rotation, màu, khung animation, và dữ liệu custom (T).

### Đặc điểm

- Dùng singleton cho mỗi kiểu T.
- Quản lý texture atlas và danh sách các sprite cần vẽ.

### Phương thức tiêu biểu

- **Add(...)**: Thêm 1 sprite vào batch với mọi tuỳ chọn transform, màu, extra data.
- **Clear()**: Xoá các sprite trong batch để bắt đầu frame mới.
- **SetTexture(texture)**: Thiết lập texture atlas dùng cho batch.
- **Draw(RenderTarget)**: Vẽ toàn bộ batch lên render target (window) chỉ bằng 1 draw call.

---

## Mối quan hệ kế thừa

```text
SceneObject
  └─ RenderObject
      └─ SpriteObject
          └─ AnimatorObject
```

- `SpriteBatch<T>` không nằm trong chuỗi kế thừa này, mà là một công cụ quản lý và tối ưu hoá vẽ sprite số lượng lớn.

---

## Lưu ý kỹ thuật

- Các class đều dùng các thuộc tính và method inline hiệu năng cao, phù hợp game engine.
- Các class có thể tuỳ biến cho từng loại entity, animation, hiệu ứng render khác nhau.
- Quản lý lifecycle, enable/disable, tag giúp tối ưu cập nhật scene và hiệu năng.
- Không quên giải phóng tài nguyên khi cần (`Dispose()`), xử lý sự kiện được override tuỳ ý cho các subclass.

---

## Ví dụ sử dụng

```csharp
// Khởi tạo object có animation
var texture = LoadTexture("player.png");
var player = new MyAnimatedPlayer(texture);
player.PlayAnimationFromGrid(32, 32, 4, 2, 0.1f);

// Vẽ nhiều sprite với SpriteBatch
var batch = SpriteBatch<MyExtraData>.Instance;
batch.SetTexture(textureAtlas);
for (int i = 0; i < 1000; ++i)
    batch.Add(new Vector2f(i * 10, 0), frameRect, extra: new MyExtraData());
batch.Draw(renderWindow);
```

---

## Đóng góp

Copyright (c) 2025 PPN Corporation. All rights reserved.

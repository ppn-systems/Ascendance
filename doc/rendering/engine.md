# Tài liệu lớp `GraphicsEngine` (Ascendance.Rendering.Engine)

## Giới thiệu

`GraphicsEngine` là lớp tĩnh (static) trung tâm quản lý cửa sổ trò chơi chính, vòng lặp render (vẽ màn hình) và các sự kiện cốt lõi của engine đồ họa. Lớp này cung cấp các phương thức và thuộc tính để khởi tạo, vận hành, cập nhật và đóng hệ thống đồ họa cũng như xử lý các sự kiện đầu vào (input) và chuyển cảnh.

---

## Các trường & thuộc tính chính

### Trường dữ liệu (Fields)

- **_window**: Đối tượng cửa sổ đồ họa của trò chơi.
- **_foregroundFps**: Giới hạn FPS khi ứng dụng đang được focus (đang chạy foreground).
- **_backgroundFps**: Giới hạn FPS khi ứng dụng mất focus (background).
- **_isFocused**: Trạng thái focus của cửa sổ (đang được click/chỉnh hay không).
- **_renderObjectCache**: Danh sách bộ nhớ đệm các đối tượng vẽ được.
- **_renderCacheDirty**: Đánh dấu bộ cache cần được làm mới.

### Thuộc tính (Properties)

- **IsDebugMode**: Kiểm tra ứng dụng có đang bật chế độ debug hay không.
- **ScreenSize**: Kích thước hiện tại của cửa sổ (chiều rộng x chiều cao).
- **GraphicsConfig**: Cấu hình đồ họa cho toàn bộ ứng dụng.
- **FrameUpdate**: Hàm người dùng truyền vào, được gọi mỗi frame để xử lý logic tự định nghĩa.

---

## Khởi tạo lớp (`static GraphicsEngine()`)

- Lấy cấu hình đồ họa từ `ConfigurationManager`.
- Thiết lập cửa sổ game bằng `RenderWindow` với các tùy chọn cấu hình (VSync, Framerate, v.v).
- Đăng ký các sự kiện cửa sổ: đóng, mất focus, được focus, thay đổi kích thước.
- Khởi tạo các giá trị mặc định cho FPS và bộ lưu cache.

---

## Các phương thức chính

### Khởi chạy vòng lặp game (`Run`)

- Khởi tạo hệ thống scene.
- Bắt đầu vòng lặp chính: xử lý event, cập nhật thời gian, thực hiện các cập nhật (`UPDATE_FRAME`), vẽ cảnh (`DRAW`), refresh màn hình.
- Điều chỉnh framerate tùy theo trạng thái focus.
- Xử lý exception và giải phóng tài nguyên khi kết thúc.

### Thay đổi chế độ debug (`DebugMode`)

- Bật/tắt chế độ debug của engine.

### Đặt icon cho cửa sổ (`SetWindowIcon`)

- Cho phép người dùng đặt biểu tượng (icon) cho cửa sổ ứng dụng.

### Đóng cửa sổ & giải phóng tài nguyên (`Shutdown`)

- Đóng cửa sổ và gọi giải phóng các hệ thống có liên quan như nhạc nền.

---

## Các phương thức nội bộ

### Cập nhật mỗi frame (`UPDATE_FRAME`)

- Gọi hàm cập nhật user định nghĩa qua `FrameUpdate`.
- Cập nhật input (chuột, bàn phím).
- Chuyển đổi, tạo, huỷ cảnh, cập nhật đối tượng cảnh.

### Vẽ màn hình (`DRAW`)

- Làm mới bộ cache nếu cần.
- Sắp xếp danh sách đối tượng theo thứ tự Z-index.
- Vẽ các đối tượng hiện lên màn hình.

### Xử lý thay đổi trạng thái focus (`HANDLE_FOCUS_CHANGED`)

- Điều chỉnh giới hạn FPS khi cửa sổ bị mất focus hoặc được focus lại.

---

## Lưu ý kỹ thuật & Best Practices

- Nên kiểm tra trạng thái Focus để điều chỉnh hiệu năng/tiết kiệm tài nguyên.
- Khi sử dụng `FrameUpdate`, đảm bảo logic tự định nghĩa không làm tắc nghẽn vòng lặp render.
- Luôn giải phóng tài nguyên các hệ thống phụ trợ (âm nhạc, scene, v.v) khi tắt engine để tránh rò rỉ bộ nhớ.

---

## Ví dụ sử dụng

```csharp
// Đăng ký hàm cập nhật frame
GraphicsEngine.FrameUpdate = dt => {
    // Cập nhật logic game mỗi frame, ví dụ:
    if (player != null) player.Update(dt);
};

// Khởi chạy game
GraphicsEngine.Run();
```

---

## Tác giả & bản quyền

Copyright (c) 2025 PPN Corporation. All rights reserved.

# Tài liệu nhóm lớp UI Controls (`Button`, `TextInputField`, `PasswordField`) trong Ascendance.Rendering.UI.Controls

> Bộ này cung cấp các control UI cơ bản cho game/app framework Ascendance: nút bấm, input text và trường nhập password ẩn/mask.  
> Tích hợp với hệ thống rendering, layout, input quản lý bởi engine.

---

## 1. `Button`

**Nút bấm tuỳ chỉnh, co giãn, có hiệu ứng hover/disabled, thích hợp cho UI menu/game.**
- Kế thừa từ `RenderObject`, tích hợp layout 9-slice qua `NineSlicePanel`.
- Quản lý label text, màu, padding, trạng thái enabled/hovered.
- Hỗ trợ sự kiện click (mouse/keyboard), callback tuỳ biến.

### Thuộc tính & API tiêu biểu

- **SetWidth(width)**, **SetHeight(height)**, **SetSize(w,h)**: Tuỳ chỉnh kích thước.
- **SetText(text)**: Đổi nội dung label.
- **SetFontSize(size)**: Đổi size chữ.
- **SetPadding(padding)**: Đổi khoảng cách hai bên label.
- **SetPosition(position)**: Đặt vị trí top-left button.
- **SetColors(panelNormal, panelHover, panelDisabled)**: Tuỳ chỉnh màu panel cho trạng thái.
- **SetTextColors(textNormal, textHover, textDisabled)**: Tuỳ chỉnh màu text cho trạng thái.
- **SetEnabled(enabled)**: Bật/tắt trạng thái nút.
- **SetTextOutline(color, thickness)**: Viền outline cho chữ.
- **RegisterClickHandler(handler)**: Đăng ký callback khi bấm.
- **UnregisterClickHandler(handler)**: Hủy callback.
- **GetGlobalBounds()**: Lấy vùng bao (bounding box) màn hình của button.

### Vòng lặp chính

- **Update(dt)**: Cập nhật trạng thái hover, pressed, sự kiện click chuột/phím.
- **Draw(target)**: Vẽ button và label lên target.
---
## 2. `TextInputField`

**Ô nhập liệu đơn dòng cho username/search/...**

- Render bằng `NineSlicePanel`, có caret nhấp nháy, tự động scroll hiển thị caret/tail cho text quá d��i.
- Hỗ trợ mouse click để focus, nhận phím A-Z, 0-9, một số ký tự đặc biệt; Backspace/Delete với repeat.
- Hỗ trợ placeholder khi text rỗng.
- Tích hợp validation rule cho input (VD: username/password).

### Thuộc tính & API tiêu biểu

- **Text**: Giá trị nhập hiện tại (get/set).
- **MaxLength**: Giới hạn số ký tự nhập.
- **Placeholder**: Nội dung hiển thị khi rỗng/unfocused.
- **Focused**: Có đang focus không.
- **Position, Size, Padding**: Điều chỉnh layout.

### Sự kiện

- **TextChanged**: Kích hoạt khi text thay đổi.
- **TextSubmitted**: Kích hoạt khi bấm Enter lúc focused.

### Main loop

- **Update(dt)**: Quản lý focus, key input, caret blink, cursor repeat.
- **Draw(target)**: Vẽ panel, text, caret lên target.
- **GetRenderText()**: Quyết định text hiển thị: placeholder/true text/hoặc masked khi làm password.

---

## 3. `PasswordField` (kế thừa `TextInputField`)

**Ô nhập liệu password – ẩn ký tự khi nhập, có nút show/hide.**

- Mặc định mask bằng ký tự "•" (U+2022).
- Cho phép toggle hiện/ẩn password bằng thuộc tính `IsPasswordVisible`.

### Thuộc tính/API tiêu biểu

- **IsPasswordVisible**: Có hiện text thật không; mặc định false.
- **MaskCharacter**: Ký tự dùng để che (mặc định: •).
- **ToggleVisibility()**: Đảo trạng thái show/hide password.
- **GetRenderText()**: Nếu đang show thì text thật, còn không thì chuỗi mask bằng `MaskCharacter`.

---

## Mô tả tổng quan

- Các control có tính năng cập nhật trạng thái, tương tác cả chuột + bàn phím, hỗ trợ layout động, và gọi đúng event (callback) khi thao tác.
- Thích hợp UI cho menu, đăng nhập, cài đặt, nhập dữ liệu, popup nhỏ cho game/app .NET.

---

## Ví dụ sử dụng

```csharp
// Tạo button
var btn = new Button("Login", texButton, fontMain)
    .SetPosition(new Vector2f(100, 250))
    .SetWidth(240)
    .RegisterClickHandler(() => LoginUser());

// Ô nhập username
var usernameField = new TextInputField(texInput, border, srcRect, fontMain, 24, new Vector2f(300, 40), new Vector2f(100, 300));
usernameField.Placeholder = "Enter username...";

// Ô nhập password với icon show/hide
var passwordField = new PasswordField(texInput, border, srcRect, fontMain, 24, new Vector2f(300, 40), new Vector2f(100, 350));
// Khi bấm nút "eye": passwordField.ToggleVisibility();
```

---

## Lưu ý

- Nên kết hợp các control cho UI chuẩn hóa.
- Có thể custom màu, theme qua các API, dùng sự kiện để xử lý logic riêng cho game/app.

---

## Bản quyền

Copyright (c) 2025 PPN Corporation. All rights reserved.

# Tài liệu nhóm lớp Notification, Banner trong Ascendance.Rendering.UI.Notifications

> Cung cấp các UI thông báo, popup, banner cuộn cho framework Ascendance: gồm hộp thông báo có nút, rolling/scolling banner, và notification cơ bản. Tất cả đều tương thích với hệ thống panel/text/engine.

---

## 1. `Notification`

**Popup thông báo nhẹ, chỉ hiện text trên 9-slice panel, auto word-wrap.**

- Vị trí có thể gán: phía trên hoặc dưới màn hình.
- Tự động tính layout, kích thước phù hợp nội dung.
- API cho thay đổi message động.

### APIs

- **UpdateMessage(newMessage)**: Đổi nội dung thông báo, tự động wrap lại và cập nhật layout.
- **Update(deltaTime)**: Override cho logic động nếu cần.
- **Draw(target)**: Vẽ background panel và text ra target.

---

## 2. `ButtonNotification` (kế thừa Notification)

**Hộp thông báo có nút hành động (OK/Xác nhận) dùng lại UI.Controls.Button.**

- Khi bấm sẽ tự động đóng notification.
- Có thể đăng ký callback cho OnClicked khi nút được bấm.
- Tuỳ chỉnh layout nút, padding, label, màu, ...

### APIs tiêu biểu

- **RegisterAction/UnregisterAction(handler)**: Đăng ký/hủy callback khi nhấn nút.
- **Update(deltaTime)**: Cập nhật trạng thái notification & button.
- **Draw(target)**: Vẽ notification và nút ra target.
- **UpdateMessage(newMessage)**: Đổi text, đồng thời reposition lại nút.

---

## 3. `RollingBanner`

**Thanh banner liên tục cuộn nhiều thông điệp từ phải qua trái (giống news ticker).**

- Hiển thị nhiều message tuần tự, tự động lặp lại khi ra khỏi màn hình trái.
- Tự động xây dựng danh sách text, hỗ trợ thay đổi danh sách message thời gian thực.

### APIs tiêu biểu

- **SetMessages(List<string>)**: Đổi message, tự động reset lại vị trí các text.
- **Update(deltaTime)**: Cập nhật cuộn cho tất cả message.
- **Render(target)**: Vẽ background và tất cả dòng text.

---

## 4. `ScrollingBanner`

**Banner cuộn 1 thông báo đơn liên tục từ phải sang trái, khi hết thì reset lại từ phải.**

- Chỉ hiển thị 1 message, speed có thể cấu hình.
- Có thể thay đổi message động.

### APIs tiêu biểu

- **SetMessage(message)**: Đặt lại thông báo, reset vị trí cuộn.
- **Update(deltaTime)**: Cập nhật vị trí text, reset khi trượt hết qua trái.
- **Render(target)**: Vẽ background và dòng text.

---

## Ví dụ sử dụng UI Notification

```csharp
// Notification đơn
var notif = new Notification(font, panelTex, "Login successful!", Direction2D.Up);
// Notification với nút OK/cancel
var btnNotif = new ButtonNotification(font, panelTex, "Update available!", Direction2D.Down, "Update");
btnNotif.RegisterAction(() => UpdateApp());

// Banner cuộn nhiều dòng
var rollingBanner = new RollingBanner(new List<string> { "Server online", "Update soon!" }, 100, fontMain);

// Banner cuộn single
var scrollingBanner = new ScrollingBanner("Welcome to the game!", fontHeader);
```

---

## Lưu ý

- Các loại notification/banner đều có thể custom layout, màu, padding, font, tốc độ,... bằng cách dùng các API.
- Đều sử dụng `RenderObject` nên có thể quản lý bởi hệ thống Engine và Scene của framework.

---

## Bản quyền

Copyright (c) 2025 PPN Corporation. All rights reserved.

# Tài liệu nhóm lớp Scene trong Ascendance.Rendering.Scenes

> Đây là các lớp quản lý scene (cảnh), chuyển scene, và truyền dữ liệu khi chuyển cảnh cho engine Ascendance.  
> Scene là khái niệm trung tâm trong quản lý flow của game/app, quản lý các đối tượng (object) và hiệu ứng khi chuyển đổi.
---

## 1. `BaseScene`

**Lớp nền trừu tượng cho một cảnh (scene) trong game.**

- Quản lý danh sách các `SceneObject` ban đầu trong cảnh.
- Đảm bảo lớp dẫn xuất phải implement phương thức `LoadObjects()` để khởi tạo các object đặc trưng cho cảnh.

### Thuộc tính & phương thức tiêu biểu

- **Name**: Tên cảnh.
- **GetObjects()**: Trả về danh sách các object ban đầu c���a cảnh.
- **AddObject(SceneObject)**: Thêm object vào cảnh.
- **InitializeScene()**: Xoá object cũ, gọi tải object mới qua hàm `LoadObjects()`.

---

## 2. `SceneManager`

**Quản lý chuyển đổi, khởi tạo, spawn/destroy các scene và object.**

- Lưu scene đang chạy, scene kế tiếp sẽ chuyển sang, danh sách các scene đã load.
- Quản lý `SceneObject` theo các trạng thái: đang hoạt động, đợi spawn/destroy.

### APIs quan trọng

- **RequestSceneChange(name)**: Đặt cảnh cần chuyển đổi ở frame tiếp theo.
- **EnqueueSpawn(o)**, **EnqueueDestroy(o)**: Đưa object vào hàng đợi spawn/destroy.
- **ProcessSceneChange()**: Chuyển cảnh nếu có yêu cầu.
- **ProcessPendingSpawn(), ProcessPendingDestroy()**: Thực thi hàng đợi khởi tạo và huỷ object.
- **UpdateSceneObjects(deltaTime)**: Gọi update cho toàn bộ object đang hoạt động.
- **FindFirstObjectOfType`<`T`>`**: Tìm object đầu tiên theo kiểu.
- Sự kiện **SceneChanged** dùng để theo dõi quá trình chuyển đổi cảnh.

---

## 3. `SceneTransition`

**Hiệu ứng chuyển đổi cảnh với overlay 2 pha (đóng–mở).**

- Quản lý trạng thái chuyển cảnh thông qua overlay và tự động chuyển cảnh ở giữa quá trình.
- Hỗ trợ nhiều hiệu ứng chuyển cảnh thông qua interface `ITransitionDrawable`.
- Instance này tồn tại xuyên cảnh, tự hủy khi hoàn tất hiệu ứng chuyển.

### Constructor & phương thức

- **SceneTransition(nextSceneName, style, duration, color)**: Khởi tạo chuyển cảnh với tên cảnh mới, kiểu hiệu ứng, thời lượng và màu overlay.
- **Update(deltaTime)**: Tăng tiến trạng thái chuyển cảnh, tự chuyển cảnh ở giữa và hủy đối tượng khi hết hiệu ứng.
- **GetDrawable()**: Lấy overlay để vẽ lên màn hình.

---

## 4. `SceneTransitionData<T>`

**Lưu thông tin truyền qua chuyển cảnh và tồn tại xuyên cảnh đổi.**

- Dùng để truyền dữ liệu giữa hai cảnh / màn hình (ví dụ: điểm số, trạng thái,..).
- Tự động huỷ sau 1 frame chuyển cảnh thông qua sự kiện SceneChanged.

### Thuộc tính & phương thức

- **Name**: Tên dữ liệu cảnh.
- **GetData()**: Lấy data đang lưu.
- **FindByName(name, defaultValue)**: Tìm instance có tên, trả về data hoặc mặc định nếu không có.
- **Update(deltaTime)**: Tự động huỷ sau khi scene thay đổi.
- **Initialize()/OnBeforeDestroy()**: Tự động đăng ký/hủy đăng ký với sự kiện SceneChanged.

---

## Ví dụ luồng chuyển cảnh

```csharp
// Khởi tạo hiệu ứng chuyển đổi cảnh
var transition = new SceneTransition("GameplayScene", SceneTransitionEffect.Fade, 1.5f);
SceneManager.EnqueueSpawn(transition);

// Lưu dữ liệu truyền qua cảnh
var data = new SceneTransitionData<GameScore>(scoreValue, "ScoreInfo");
SceneManager.EnqueueSpawn(data);

// Lấy lại dữ liệu sau khi chuyển cảnh ở scene mới
var score = SceneTransitionData<GameScore>.FindByName("ScoreInfo", defaultValue);
```

---

## Hệ thống vận hành

- Mỗi cảnh sẽ kế thừa từ `BaseScene`, tự tạo các đối tượng cần thiết bên trong.
- Quản lý trạng thái object bằng các hàng đợi spawn/destroy giúp tối ưu và kiểm soát logic update/lifecycle.
- Chuyển đổi cảnh mượt mà nhờ hiệu ứng overlay và sự kiện chuyển cảnh cho phép truyền thông tin hoặc cleanup hợp lý.

---

## Bản quyền

Copyright (c) 2025 PPN Corporation. All rights reserved.

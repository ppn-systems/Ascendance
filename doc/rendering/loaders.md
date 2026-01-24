# Tài liệu nhóm lớp Asset Loader trong Ascendance.Rendering.Loaders

> Bộ lớp này quản lý việc tải, giải phóng (dispose/unload) các tài nguyên (assets) như texture, font, âm thanh cho engine Ascendance. Nó giúp kiểm soát hiệu quả bộ nhớ và log quá trình tải các file tài nguyên.

---

## 1. `AssetLoader<T>`

**Lớp nền trừu tượng cho việc quản lý tài nguyên theo kiểu T**  
Quản lý việc load, cache, và dispose cho các asset theo tên hoặc data thô (raw data).

### Các thuộc tính chính

- **RootFolder**: Thư mục gốc chứa asset khi load từ file.
- **EnableLogging**: Có ghi log quá trình tải/giải phóng asset không.
- **Disposed**: Đã bị dispose chưa.
- **FileEndings**: Các kiểu định dạng file hỗ trợ (đuôi file .png, .jpg,...).
- **_assets**: Dictionary chứa các asset đã được load (key theo tên).

### Các phương thức tiêu biểu

- **Load(name, rawData?)**: Load tài nguyên theo tên (hoặc từ data thô nếu có). Trả về instance T hoặc lấy lại từ cache nếu đã load.
- **LoadAllAssetsInDirectory(logErrors?)**: Tải toàn bộ asset trong thư mục gốc.
- **Unload(name)**: Giải phóng asset theo tên và xoá khỏi cache.
- **Dispose()**: Giải phóng toàn bộ tài nguyên.
- **Load(byte[])**: (Có thể được override) Load từ mảng byte raw data.
- **CreateInstanceFromPath(path)**: (Có thể được override) Tạo instance T từ path file.
- **ResolveFileExtension(name)**: Kiểm tra các đuôi file hợp lệ để tìm asset.

**Lưu ý:** Khi muốn custom cho từng loại asset, cần kế thừa và override Load/CreateInstanceFromPath phù hợp.

---

## 2. `FontLoader`

**Lớp quản lý việc tải font (`Font`)**  
Kế thừa từ `AssetLoader<Font>`.  
Hỗ trợ các đuôi file font: `.ttf`, `.cff`, `.fnt`, `.otf`, `.eot`.

### Phương thức override

- **Load(byte[])**: Tạo đối tượng Font từ raw data thông qua MemoryStream.
- **CreateInstanceFromPath(path)**: Tạo Font từ path file cụ thể.

---

## 3. `SoundEffectLoader`

**Lớp quản lý hiệu ứng âm thanh (`SoundBuffer`)**  
Kế thừa từ `AssetLoader<SoundBuffer>`.  
Hỗ trợ nhiều định dạng âm thanh như `.ogg`, `.wav`, `.flac`, `.mp3`,...

### Phương thức tiêu biểu

- **Load(name, Stream)**: Tạo SoundBuffer từ stream raw data của âm thanh.
- **Load(byte[])**: Tạo SoundBuffer từ raw data.
- **CreateInstanceFromPath(path)**: Tạo SoundBuffer từ path file.

---

## 4. `TextureLoader`

**Lớp quản lý texture hình ảnh (`Texture`)**  
Kế thừa từ `AssetLoader<Texture>`.  
Hỗ trợ các định dạng phổ biến như `.bmp`, `.png`, `.jpg`, `.gif`, `.psd`,...

### Các thuộc tính riêng

- **Repeat**: Texture lặp lại khi vẽ ngoài khuôn hình.
- **Smoothing**: Dùng smooth cho texture khi hiển thị (làm mịn).

### Phương thức tiêu biểu

- **Load(name, rawData?)**: Lấy texture theo tên (hoặc data thô). Có thể cấu hình repeat/smooth ở mỗi lần load.
- **Load(name, repeat, smoothing, rawData)**: Load với kiểm soát repeat/smoothing từng lần.
- **Load(byte[])**: Tạo Texture từ raw data thông qua MemoryStream.
- **CreateInstanceFromPath(path)**: Tạo Texture từ file thông qua FileStream.

---

## Hướng dẫn sử dụng cơ bản

```csharp
// Font
var fontLoader = new FontLoader("Assets/Fonts");
var myFont = fontLoader.Load("OpenSans"); // Tên không cần đuôi

// Texture
var texLoader = new TextureLoader("Assets/Textures", repeat: true, smoothing: true);
var playerTex = texLoader.Load("PlayerSprite");

// Âm thanh
var sfxLoader = new SoundEffectLoader("Assets/SFX");
var jumpSound = sfxLoader.Load("Jump");

// Đóng gói/tối ưu, giải phóng bộ nhớ khi không dùng:
// texLoader.Unload("PlayerSprite");
// sfxLoader.Dispose();
```

---

## Best Practices

- Cấu hình `RootFolder` để dễ kiểm soát nguồn tài nguyên.
- Nên dispose loader hoặc unload asset sau khi dùng xong để tránh rò rỉ bộ nhớ.
- Có thể ghi log để debug quá trình tải asset.
- Khi override: Đảm bảo phương thức Load/CreateInstanceFromPath phù hợp với loại asset của bạn.

---

## Bản quyền

Copyright (c) 2025 PPN Corporation. All rights reserved.

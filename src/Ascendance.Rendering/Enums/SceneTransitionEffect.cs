// Copyright (c) 2025 PPN Corporation. All rights reserved.

namespace Ascendance.Rendering.Enums;

/// <summary>
/// Định nghĩa các hiệu ứng hình ảnh được sử dụng khi chuyển đổi giữa các cảnh.
/// </summary>
public enum SceneTransitionEffect : System.Byte
{
    /// <summary>
    /// Hiệu ứng mờ dần vào hoặc mờ dần ra.
    /// </summary>
    Fade,

    /// <summary>
    /// Hiệu ứng phủ màn hình theo chiều ngang từ trái sang phải, sau đó mở ra.
    /// </summary>
    WipeHorizontal,

    /// <summary>
    /// Hiệu ứng phủ màn hình theo chiều dọc từ trên xuống dưới, sau đó mở ra.
    /// </summary>
    WipeVertical,

    /// <summary>
    /// Hiệu ứng tấm phủ trượt từ bên trái vào màn hình, sau đó trượt ra.
    /// </summary>
    SlideCoverLeft,

    /// <summary>
    /// Hiệu ứng tấm phủ trượt từ bên phải vào màn hình, sau đó trượt ra.
    /// </summary>
    SlideCoverRight,

    /// <summary>
    /// Hiệu ứng phóng to khung hình từ kích thước nhỏ đến toàn màn hình, sau đó thu lại.
    /// </summary>
    ZoomIn,

    /// <summary>
    /// Hiệu ứng thu nhỏ khung hình từ kích thước lớn về 0, sau đó phóng lại.
    /// </summary>
    ZoomOut
}

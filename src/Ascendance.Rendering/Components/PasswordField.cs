// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Components.Rules;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.Components;

/// <summary>
/// Single-line password input built on top of <see cref="TextInputField"/>.
/// </summary>
/// <remarks>
/// - Masks user input with <see cref="MaskChar"/> by default.<br/>
/// - Set <see cref="IsShowField"/> = <c>true</c> to reveal raw text (useful for an "eye" toggle).<br/>
/// </remarks>
public sealed class PasswordField : TextInputField
{
    #region Properties

    /// <summary>
    /// Whether to reveal the raw text (i.e., “show password”). Default: <c>false</c>.
    /// </summary>
    public System.Boolean IsShowField { get; set; } = false;

    /// <summary>
    /// Mask character used when <see cref="IsShowField"/> is <c>false</c>. Default: • (U+2022).
    /// </summary>
    public System.Char MaskChar { get; set; } = '\u2022';

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Creates a new password field.
    /// </summary>
    public PasswordField(
        Texture panelTexture,
        Thickness border,
        IntRect sourceRect,
        Font font,
        System.UInt32 fontSize,
        Vector2f size,
        Vector2f position)
        : base(panelTexture, border, sourceRect, font, fontSize, size, position) => base.ValidationRule = new PasswordValidationRule();

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Toggle <see cref="IsShowField"/> state. (VN) Đổi trạng thái hiện/ẩn mật khẩu.
    /// </summary>
    public void Toggle() => IsShowField = !IsShowField;

    /// <summary>
    /// Returns what should be displayed: raw text when <see cref="IsShowField"/> is true,
    /// otherwise masked with <see cref="MaskChar"/>.
    /// </summary>
    protected override System.String GetDisplayText()
    {
        // Nếu đang “show”, hiển thị text thường
        if (IsShowField)
        {
            return Text;
        }

        // Khi ẩn, trả về chuỗi mask có độ dài bằng số ký tự thực
        System.Int32 len = Text?.Length ?? 0;
        return len == 0 ? System.String.Empty : new System.String(MaskChar, len);
    }

    #endregion APIs
}

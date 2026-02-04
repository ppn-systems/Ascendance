// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Internal.Input;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Rendering.UI.Controls;

/// <summary>
/// Represents a single-line password input control built on top of
/// <see cref="TextInputField"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item>
///     <description>
///     User input is masked using <see cref="MaskCharacter"/> by default.
///     </description>
///   </item>
///   <item>
///     <description>
///     Set <see cref="IsPasswordVisible"/> to <c>true</c> to reveal the raw text
///     (e.g., for a "show password" toggle).
///     </description>
///   </item>
/// </list>
/// </remarks>
public sealed class PasswordField : TextInputField
{
    #region Properties

    /// <summary>
    /// Gets or sets whether the raw password text is visible.
    /// </summary>
    /// <remarks>
    /// When set to <c>false</c>, the displayed text is masked using
    /// <see cref="MaskCharacter"/>.
    /// </remarks>
    public System.Boolean IsPasswordVisible { get; set; } = false;

    /// <summary>
    /// Gets or sets the character used to mask the password when
    /// <see cref="IsPasswordVisible"/> is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Default value is the bullet character (•, U+2022).
    /// </remarks>
    public System.Char MaskCharacter { get; set; } = '\u2022';

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordField"/> class
    /// with explicit border thickness.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="border">Panel border thickness.</param>
    /// <param name="sourceRect">Rectangle region from the texture.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public PasswordField(
        Texture panelTexture,
        Thickness border,
        IntRect sourceRect,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
        : base(panelTexture, border, sourceRect, size, position, font, fontSize) => base.ValidationRule = new PasswordValidationRule();

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordField"/> class
    /// with default border thickness.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="sourceRect">Rectangle region from the texture.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public PasswordField(
        Texture panelTexture,
        IntRect sourceRect,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
        : base(panelTexture, new Thickness(32), sourceRect, size, position, font, fontSize)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordField"/> class
    /// with the default border thickness and default source rectangle.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    public PasswordField(
        Texture panelTexture,
        Vector2f size,
        Vector2f position,
        Font font = null,
        System.UInt32 fontSize = 16)
        : base(panelTexture, new Thickness(32), default, size, position, font, fontSize)
    { }

    #endregion Constructor

    #region APIs

    /// <summary>
    /// Toggle <see cref="IsPasswordVisible"/> state. (VN) Đổi trạng thái hiện/ẩn mật khẩu.
    /// </summary>
    public void ToggleVisibility() => IsPasswordVisible = !IsPasswordVisible;

    /// <summary>
    /// Returns what should be displayed: raw text when <see cref="IsPasswordVisible"/> is true,
    /// otherwise masked with <see cref="MaskCharacter"/>.
    /// </summary>
    protected override System.String GetRenderText()
    {
        // Nếu đang “show”, hiển thị text thường
        if (this.IsPasswordVisible)
        {
            return base.Text;
        }

        // Khi ẩn, trả về chuỗi mask có độ dài bằng số ký tự thực
        System.Int32 len = base.Text?.Length ?? 0;
        return len == 0 ? System.String.Empty : new System.String(this.MaskCharacter, len);
    }

    #endregion APIs
}

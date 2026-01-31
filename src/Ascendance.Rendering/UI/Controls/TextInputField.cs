// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Abstractions;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Internal.Input;
using Ascendance.Rendering.Internal.Input.Rules;
using Ascendance.Rendering.Layout;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.UI.Controls;

/// <summary>
/// Lightweight single-line text input built atop your <c>NineSlicePanel</c>.
/// </summary>
/// <remarks>
/// <para>
/// - Click to focus; caret blinks when focused.<br/>
/// - Typing supports A–Z, 0–9, a few punctuation keys (space, '.' ',' '-' '\''), Backspace/Delete with key-repeat.<br/>
/// - Text scrolls to ensure the caret (at end) is always visible inside the box width.<br/>
/// - Rendering order: panel → text → caret (if focused &amp; visible).<br/>
/// </para>
/// </remarks>
public class TextInputField : RenderObject, IFocusable
{
    #region Constants

    private const System.Single DefaultPaddingX = 16f;
    private const System.Single DefaultPaddingY = 6f;

    private const System.Single CaretBlinkPeriod = 0.5f; // (VN) Chu kỳ nháy caret
    private const System.Single KeyRepeatNextDelay = 0.05f;
    private const System.Single KeyRepeatFirstDelay = 0.35f;

    #endregion Constants

    #region Fields

    private readonly Text _text;        // used for drawing
    private readonly Text _measure;     // used for measuring width/pos exactly
    private readonly RectangleShape _caret;
    private readonly NineSlicePanel _panel;
    private readonly System.UInt32 _fontSize;
    private readonly KeyRepeatController _deleteRepeat;
    private readonly KeyRepeatController _backspaceRepeat;
    private readonly System.Text.StringBuilder _buffer = new();

    private Vector2f _padding;
    private FloatRect _hitBox;          // cached for mouse hit-test
    private System.Int32 _caretIndex;
    private System.Int32 _scrollStart; // start index of visible window (inclusive)
    private System.Single _caretTimer;
    private System.Single _caretWidth;
    private System.Boolean _caretVisible;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Maximum number of characters allowed; <c>null</c> means unlimited.
    /// </summary>
    public System.Int32? MaxLength { get; set; }

    /// <summary>
    /// Optional placeholder (shown when <see cref="Text"/> is empty and unfocused).
    /// </summary>
    public System.String Placeholder { get; set; } = System.String.Empty;

    /// <summary>
    /// Raised whenever <see cref="Text"/> changes.
    /// </summary>
    public event System.Action<System.String> TextChanged;

    /// <summary>
    /// Raised when user presses Enter while focused.
    /// </summary>
    public event System.Action<System.String> TextSubmitted;

    /// <summary>
    /// Validation rule for input text; can be <c>null</c> for no validation.
    /// </summary>
    public ITextValidationRule ValidationRule { get; set; }

    /// <summary>
    /// Gets or sets the current text content.
    /// </summary>
    public System.String Text
    {
        get => _buffer.ToString();
        set
        {
            _ = _buffer.Clear().Append(value ?? System.String.Empty);
            _caretIndex = _buffer.Length;
            this.CLAMP_TO_MAX_LENGTH();
            this.RESET_SCROLL_AND_CARET();
            this.TextChanged?.Invoke(_buffer.ToString());
        }
    }

    /// <summary>
    /// Gets or sets whether the field is focused.
    /// </summary>
    public System.Boolean Focused { get; private set; }

    /// <summary>
    /// Text position is derived from panel position + padding.
    /// </summary>
    public Vector2f Position
    {
        get => _panel.Position;
        set
        {
            _ = _panel.SetPosition(value);
            this.RELAYOUT_TEXT();
            this.UPDATE_HIT_BOX();
            this.UPDATE_CARET_IMMEDIATE();
        }
    }

    /// <summary>
    /// Panel size; text area is inner size minus padding.
    /// </summary>
    public Vector2f Size
    {
        get => _panel.Size;
        set
        {
            _ = _panel.SetSize(ENSURE_MIN_SIZE(value, _panel.Border));
            this.RELAYOUT_TEXT();
            this.UPDATE_HIT_BOX();
            this.RESET_SCROLL_AND_CARET();
        }
    }

    /// <summary>
    /// Padding (x,y) inside the panel.
    /// </summary>
    public Vector2f Padding
    {
        get => _padding;
        set
        {
            _padding = value;
            this.RELAYOUT_TEXT();
            this.RESET_SCROLL_AND_CARET();
        }
    }

    /// <summary>
    /// Width of the caret in pixels.
    /// </summary>
    public System.Single CaretWidth
    {
        get => _caretWidth;
        set
        {
            _caretWidth = System.MathF.Max(0.5f, value);
            this.UPDATE_CARET_IMMEDIATE();
        }
    }

    #endregion Properties

    #region Construction

    /// <summary>
    /// Creates a new <see cref="TextInputField"/>.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="border">9-slice borders.</param>
    /// <param name="sourceRect">Texture rect.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    public TextInputField(Texture panelTexture, Thickness border, IntRect sourceRect, Font font, System.UInt32 fontSize, Vector2f size, Vector2f position)
    {
        _caretWidth = 1f;
        _fontSize = fontSize;
        _deleteRepeat = new();
        _backspaceRepeat = new();
        _padding = new(DefaultPaddingX, DefaultPaddingY);
        _panel = new NineSlicePanel(panelTexture, border, sourceRect);
        _ = _panel.SetPosition(position).SetSize(ENSURE_MIN_SIZE(size, border));

        // (VN) _measure chỉ dùng đo kích thước/khoảng cách glyph → tránh xê dịch do bearings
        _measure = new Text(System.String.Empty, font, _fontSize)
        {
            FillColor = new Color(30, 30, 30)
        };

        _text = new Text(System.String.Empty, font, _fontSize)
        {
            FillColor = new Color(30, 30, 30)
        };

        this.RELAYOUT_TEXT();

        _caret = new RectangleShape(new Vector2f(_caretWidth, _fontSize))
        {
            FillColor = _text.FillColor
        };

        this.ValidationRule = new UsernameValidationRule();

        this.UPDATE_HIT_BOX();
        this.UPDATE_CARET_IMMEDIATE();

        // (VN) Cho UI nổi lên một chút; tùy engine của bạn
        base.SetZIndex(800);
    }

    /// <summary>
    /// Creates a new <see cref="TextInputField"/>.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="sourceRect">Texture rect.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    public TextInputField(Texture panelTexture, IntRect sourceRect, Font font, System.UInt32 fontSize, Vector2f size, Vector2f position)
        : this(panelTexture, new Thickness(32), sourceRect, font, fontSize, size, position)
    { }

    /// <summary>
    /// Creates a new <see cref="TextInputField"/>.
    /// </summary>
    /// <param name="panelTexture">9-slice texture.</param>
    /// <param name="font">SFML font to render text.</param>
    /// <param name="fontSize">Font size in points.</param>
    /// <param name="size">Panel size (will be clamped to minimal size by borders).</param>
    /// <param name="position">Top-left position.</param>
    public TextInputField(Texture panelTexture, Font font, System.UInt32 fontSize, Vector2f size, Vector2f position)
        : this(panelTexture, new Thickness(32), default, font, fontSize, size, position)
    { }

    #endregion Construction

    #region APIs

    /// <inheritdoc/>
    public override void Update(System.Single dt)
    {
        if (MouseManager.Instance.IsMouseButtonPressed(Mouse.Button.Left))
        {
            Vector2i mp = MouseManager.Instance.GetMousePosition();

            if (_hitBox.Contains(mp.X, mp.Y))
            {
                FocusManager.Instance.RequestFocus(this);
            }
            else
            {
                FocusManager.Instance.ClearFocus(this);
            }
        }

        if (this.Focused)
        {
            // Caret blink
            _caretTimer += dt;

            if (_caretTimer >= CaretBlinkPeriod)
            {
                _caretTimer = 0f;
                _caretVisible = !_caretVisible;
            }

            this.HANDLE_KEY_INPUT(dt);
        }

        this.UPDATE_VISIBLE_TEXT();
        this.UPDATE_CARET_IMMEDIATE();
    }

    /// <inheritdoc/>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_text);

        if (Focused && _caretVisible)
        {
            target.Draw(_caret);
        }
    }

    /// <summary>
    /// Not used by engine (we render explicitly in <see cref="Draw"/>), but must be provided.
    /// </summary>
    protected override Drawable GetDrawable() => _text;

    /// <summary>
    /// Set the panel's tint color.
    /// </summary>
    public void SetPanelColor(Color color) => _panel.SetTintColor(color);

    /// <summary>
    /// Set the text/caret color together.
    /// </summary>
    public void SetTextColor(Color color)
    {
        _text.FillColor = color;
        _caret.FillColor = color;
    }

    /// <summary>
    /// Returns what should be displayed: placeholder, masked password, or raw text.
    /// </summary>
    protected virtual System.String GetRenderText()
        => _buffer.Length == 0 && !this.Focused && !System.String.IsNullOrEmpty(this.Placeholder) ? this.Placeholder : _buffer.ToString();

    /// <inheritdoc/>
    void IFocusable.OnFocusGained()
    {
        this.Focused = true;
        _caretVisible = true;
        _caretTimer = 0f;
    }

    /// <inheritdoc/>
    void IFocusable.OnFocusLost()
    {
        this.Focused = false;
        _caretVisible = false;
    }

    #endregion APIs

    #region Private Methods

    /// <summary>
    /// Handles key presses and key repeats for Backspace/Delete.
    /// </summary>
    private void HANDLE_KEY_INPUT(System.Single dt)
    {
        System.Boolean shift = KeyboardManager.Instance.IsKeyDown(Keyboard.Key.LShift) || KeyboardManager.Instance.IsKeyDown(Keyboard.Key.RShift);

        // Submit: Enter
        if (KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter))
        {
            TextSubmitted?.Invoke(_buffer.ToString());
        }

        // Letters A..Z
        if (_buffer.Length < (MaxLength ?? System.Int32.MaxValue) && KeyboardCharMapper.Instance.TryMapKeyToChar(out System.Char ch, shift))
        {
            System.String preview = _buffer.ToString();
            preview = preview.Insert(_caretIndex, ch.ToString());

            if (this.ValidationRule?.IsValid(preview) == false)
            {
                return;
            }

            this.APPEND_CHAR(ch);
        }

        // Backspace/Delete: edge + repeat
        if (_backspaceRepeat.Update(
                KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Backspace),
                dt,
                KeyRepeatFirstDelay,
                KeyRepeatNextDelay))
        {
            this.BACKSPACE();
        }

        // Delete key (same as Backspace here)
        if (_deleteRepeat.Update(
                KeyboardManager.Instance.IsKeyDown(Keyboard.Key.Delete),
                dt,
                KeyRepeatFirstDelay,
                KeyRepeatNextDelay))
        {
            this.DELETE();
        }
    }

    /// <summary>
    /// Updates caret position/size immediately to the end of visible text.
    /// </summary>
    private void UPDATE_CARET_IMMEDIATE()
    {
        System.String visible = _text.DisplayedString;
        System.Int32 visibleCaret = System.Math.Clamp(_caretIndex - _scrollStart, 0, visible.Length);
        Vector2f caretPos = _measure.FindCharacterPos((System.UInt32)visibleCaret);

        _caret.Size = new Vector2f(_caretWidth, _fontSize);
        _caret.Position = new Vector2f(_text.Position.X + caretPos.X - _measure.Position.X, _text.Position.Y + 2f);
    }

    /// <summary>
    /// Computes the portion of <see cref="GetRenderText"/> that fits into the inner width,
    /// ensuring the tail (caret at end) remains visible. Then assigns to <see cref="_text"/>.
    /// </summary>
    private void UPDATE_VISIBLE_TEXT()
    {
        System.String full = GetRenderText();
        _measure.DisplayedString = full;

        System.Single innerWidth = _panel.Size.X - (_padding.X * 2f) - _caretWidth; // leave space for caret

        // Helper: width of substring [i..n)
        System.Single Width(System.UInt32 i, System.UInt32 n)
            => _measure.FindCharacterPos(n).X - _measure.FindCharacterPos(i).X;

        System.UInt32 n = (System.UInt32)full.Length;
        if (n == 0)
        {
            _scrollStart = 0;
            _text.DisplayedString = System.String.Empty;
            this.APPLY_TEXT_POSITION();
            return;
        }

        // If full fits, reset scroll
        if (Width(0, n) <= innerWidth)
        {
            _scrollStart = 0;
            _text.DisplayedString = full;
            this.APPLY_TEXT_POSITION();
            return;
        }

        // Ensure tail is visible: advance start until [start..n) fits
        while (Width((System.UInt32)_scrollStart, n) > innerWidth && _scrollStart < full.Length)
        {
            _scrollStart++;
        }

        // After deletions, try reveal more head if space allows
        while (_scrollStart > 0 && Width((System.UInt32)(_scrollStart - 1), n) <= innerWidth)
        {
            _scrollStart--;
        }

        _text.DisplayedString = full[_scrollStart..];
        this.APPLY_TEXT_POSITION();
    }

    /// <summary>
    /// Append a char with MaxLength enforcement and change notification.
    /// </summary>
    private void APPEND_CHAR(System.Char c)
    {
        if (this.MaxLength.HasValue && _buffer.Length >= this.MaxLength.Value)
        {
            return;
        }

        _buffer.Insert(_caretIndex, c);
        _caretIndex++;

        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>Remove char before caret position, if any; raises <see cref="TextChanged"/>.</summary>
    private void BACKSPACE()
    {
        if (_caretIndex <= 0)
        {
            return;
        }

        _buffer.Remove(_caretIndex - 1, 1);
        _caretIndex--;

        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>Delete char at caret position, if any; raises <see cref="TextChanged"/>.</summary>
    private void DELETE()
    {
        if (_caretIndex >= _buffer.Length)
        {
            return;
        }

        _buffer.Remove(_caretIndex, 1);
        this.TextChanged?.Invoke(_buffer.ToString());
    }

    /// <summary>Clamp current text to <see cref="MaxLength"/> if needed.</summary>
    private void CLAMP_TO_MAX_LENGTH()
    {
        if (this.MaxLength.HasValue && _buffer.Length > this.MaxLength.Value)
        {
            _buffer.Length = MaxLength.Value;
        }
    }

    /// <summary>Recompute hit-box based on panel position &amp; size.</summary>
    private void UPDATE_HIT_BOX()
    {
        Vector2f s = _panel.Size;
        Vector2f p = _panel.Position;
        _hitBox = new FloatRect(p.X, p.Y, s.X, s.Y);
    }

    /// <summary>Ensure panel size never violates border minimums.</summary>
    private static Vector2f ENSURE_MIN_SIZE(Vector2f size, Thickness b)
    {
        System.Single minW = b.Left + b.Right + 1f;
        System.Single minH = b.Top + b.Bottom + 1f;
        return new Vector2f(System.MathF.Max(size.X, minW), System.MathF.Max(size.Y, minH));
    }

    /// <summary>Reposition both measure/draw texts from panel position and padding.</summary>
    private void RELAYOUT_TEXT() => this.APPLY_TEXT_POSITION();

    private void APPLY_TEXT_POSITION()
    {
        System.Single textY = _panel.Position.Y + ((_panel.Size.Y - _fontSize) / 2f) - 2f;
        System.Single textX = _panel.Position.X + _padding.X;

        _text.Position = new Vector2f(textX, textY);
        _measure.Position = _text.Position; // measure should share same anchor
    }

    /// <summary>Reset scrolling window and caret visibility after large layout changes.</summary>
    private void RESET_SCROLL_AND_CARET()
    {
        _caretTimer = 0f;
        _scrollStart = 0;
        _caretVisible = true;
        this.UPDATE_VISIBLE_TEXT();
        this.UPDATE_CARET_IMMEDIATE();
    }

    #endregion Private Methods

    #region Class

    /// <summary>
    /// Controls key repeat timing for keyboard input, supporting initial and repeated activation intervals.
    /// </summary>
    private sealed class KeyRepeatController
    {
        private System.Single _timer;
        private System.Boolean _repeating;

        /// <summary>
        /// Updates the state of the key repeat controller.
        /// </summary>
        /// <param name="isKeyDown">Indicates whether the key is currently pressed down.</param>
        /// <param name="dt">Elapsed time since the last update, in seconds.</param>
        /// <param name="firstDelay">Delay before the first repeat fires, in seconds.</param>
        /// <param name="repeatDelay">Delay between subsequent repeats, in seconds.</param>
        /// <returns>
        /// <c>true</c> if the key should be considered activated (either initially or as a repeat); otherwise, <c>false</c>.
        /// </returns>
        public System.Boolean Update(System.Boolean isKeyDown, System.Single dt, System.Single firstDelay, System.Single repeatDelay)
        {
            if (!isKeyDown)
            {
                _repeating = false;
                _timer = 0f;
                return false;
            }

            if (!_repeating)
            {
                _repeating = true;
                _timer = firstDelay;
                return true; // First key press
            }

            _timer -= dt;
            if (_timer <= 0f)
            {
                _timer = repeatDelay;
                return true;
            }

            return false;
        }
    }

    #endregion Class
}

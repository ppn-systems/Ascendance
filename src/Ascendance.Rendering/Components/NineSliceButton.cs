// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Layout;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.Components;

/// <summary>
/// A resizable button based on NineSlicePanel (single image), with text color changing on hover using tint.
/// Supports mouse and keyboard interactions, custom colors, and fluent configuration.
/// </summary>
public class NineSliceButton : RenderObject, IUpdatable
{
    #region Constants

    private const System.Single DefaultHeight = 50f;
    private const System.Single DefaultWidth = 100f;
    private const System.UInt32 DefaultFontSize = 20;
    private const System.Single HorizontalPaddingDefault = 16f;

    private static readonly IntRect DefaultSrc = default;
    private static readonly Thickness DefaultSlice = new(32);

    #endregion Constants

    #region Fields

    private readonly Text _label;
    private readonly NineSlicePanel _panel;

    // States
    private System.Boolean _isHovered, _isPressed, _wasMousePressed;
    private System.Boolean _keyboardPressed;
    private System.Boolean _isEnabled = true;

    // Layout
    private System.Single _buttonWidth;
    private System.Single _buttonHeight = DefaultHeight;
    private System.Single _horizontalPadding = HorizontalPaddingDefault;
    private FloatRect _totalBounds;
    private Vector2f _position = new(0, 0);

    // Color Themes
    private Color _panelNormal = new(30, 30, 30);
    private Color _panelHover = new(60, 60, 60);
    private Color _panelDisabled = new(40, 40, 40, 180);

    private Color _textNormal = new(200, 200, 200);
    private Color _textHover = new(255, 255, 255);
    private Color _textDisabled = new(160, 160, 160, 200);

    private event System.Action OnClick;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Creates a new stretchable button instance.
    /// </summary>
    /// <param name="text">Button label text.</param>
    /// <param name="texture">Texture for button panel.</param>
    /// <param name="font">Text font.</param>
    /// <param name="width">Initial button width.</param>
    /// <param name="sourceRect">Source rect on texture (optional).</param>
    public NineSliceButton(
        System.String text, Texture texture, Font font,
        System.Single width = 240f, IntRect sourceRect = default)
    {
        _buttonWidth = System.Math.Max(DefaultWidth, width);
        _label = new Text(text, font, DefaultFontSize) { FillColor = Color.Black };
        _panel = new NineSlicePanel(texture, DefaultSlice, sourceRect == default ? DefaultSrc : sourceRect);

        UPDATE_LAYOUT();
        APPLY_TINT();
    }

    #endregion Constructor

    #region Public API (Fluent-Friendly)

    /// <summary>
    /// Sets button width (in pixels).
    /// </summary>
    public NineSliceButton SetWidth(System.Single width)
    {
        _buttonWidth = width;
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets button height (in pixels).
    /// </summary>
    public NineSliceButton SetHeight(System.Single height)
    {
        _buttonHeight = height;
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets both width and height of the button.
    /// </summary>
    public NineSliceButton SetSize(System.Single width, System.Single height)
    {
        _buttonWidth = width;
        _buttonHeight = height;
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets button label text.
    /// </summary>
    public NineSliceButton SetText(System.String text)
    {
        _label.DisplayedString = text;
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets label font size.
    /// </summary>
    public NineSliceButton SetFontSize(System.UInt32 size)
    {
        _label.CharacterSize = size;
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets horizontal padding inside button (pixels).
    /// </summary>
    public NineSliceButton SetPadding(System.Single horizontalPadding)
    {
        _horizontalPadding = System.MathF.Max(0f, horizontalPadding);
        UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets screen-space position for button's top-left.
    /// </summary>
    public void SetPosition(Vector2f position)
    {
        _position = position;
        UPDATE_LAYOUT();
    }

    /// <summary>
    /// Sets button panel colors (normal/hover/disabled).
    /// </summary>
    public NineSliceButton SetColors(Color? panelNormal = null, Color? panelHover = null, Color? panelDisabled = null)
    {
        if (panelNormal.HasValue)
        {
            _panelNormal = panelNormal.Value;
        }

        if (panelHover.HasValue)
        {
            _panelHover = panelHover.Value;
        }

        if (panelDisabled.HasValue)
        {
            _panelDisabled = panelDisabled.Value;
        }

        APPLY_TINT();
        return this;
    }

    /// <summary>
    /// Sets text color theme (normal/hover/disabled).
    /// </summary>
    public NineSliceButton SetTextColors(Color? textNormal = null, Color? textHover = null, Color? textDisabled = null)
    {
        if (textNormal.HasValue)
        {
            _textNormal = textNormal.Value;
        }

        if (textHover.HasValue)
        {
            _textHover = textHover.Value;
        }

        if (textDisabled.HasValue)
        {
            _textDisabled = textDisabled.Value;
        }

        APPLY_TINT();
        return this;
    }

    /// <summary>
    /// Enables or disables the button interactively and visually.
    /// </summary>
    public NineSliceButton SetEnabled(System.Boolean enabled)
    {
        _isEnabled = enabled;
        APPLY_TINT();
        return this;
    }

    /// <summary>
    /// Sets outline for the text with explicit color and thickness.
    /// </summary>
    public void SetTextOutline(Color outlineColor, System.Single thickness)
    {
        _label.OutlineColor = outlineColor;
        _label.OutlineThickness = thickness;
    }

    /// <summary>
    /// Register a callback for click event.
    /// </summary>
    public void RegisterClickHandler(System.Action handler) => OnClick += handler;

    /// <summary>
    /// Unregister a previously registered click handler.
    /// </summary>
    public void UnregisterClickHandler(System.Action handler) => OnClick -= handler;

    /// <summary>
    /// Returns the button's bounds in screen space.
    /// </summary>
    public FloatRect GetGlobalBounds() => _totalBounds;

    #endregion

    #region Main Loop

    /// <summary>
    /// Updates the interactive state, mouse/keyboard events, and visual highlights.
    /// </summary>
    public override void Update(System.Single dt)
    {
        if (!IsVisible)
        {
            return;
        }

        var mousePos = MouseManager.Instance.GetMousePosition();
        System.Boolean isOver = _totalBounds.Contains(mousePos.X, mousePos.Y);
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        // Hover
        if (_isHovered != (isOver && _isEnabled))
        {
            _isHovered = isOver && _isEnabled;
            APPLY_TINT();
        }

        // Mouse click logic
        if (_isEnabled)
        {
            if (isOver && isDown && !_wasMousePressed)
            {
                _isPressed = true;
            }
            else if (_isPressed && !isDown && isOver)
            {
                FIRE_CLICK();
                _isPressed = false;
            }
            else if (!isDown)
            {
                _isPressed = false;
            }
        }
        _wasMousePressed = isDown;

        // Keyboard (Enter/Space) when hovered for gamepad/keyboard navigation
        System.Boolean keyDown = KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Enter) ||
                       KeyboardManager.Instance.IsKeyPressed(Keyboard.Key.Space);

        if (_isEnabled && _isHovered)
        {
            if (keyDown && !_keyboardPressed) { _keyboardPressed = true; }
            else if (!keyDown && _keyboardPressed) { _keyboardPressed = false; FIRE_CLICK(); }
        }
        else
        {
            _keyboardPressed = false;
        }
    }

    /// <summary>
    /// Renders the button and its label.
    /// </summary>
    public void Render(RenderTarget target)
    {
        if (!IsVisible)
        {
            return;
        }

        target.Draw(_panel);
        target.Draw(_label);
    }

    /// <summary>
    /// This button does not support GetDrawable (use Render).
    /// </summary>
    protected override Drawable GetDrawable() =>
        throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout

    /// <summary>
    /// Updates panel and text geometry based on current layout/padding/text.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        // Ensure enough room for text + padding
        var tb = _label.GetLocalBounds();
        System.Single minTextWidth = tb.Width + (_horizontalPadding * 2f);

        System.Single totalWidth = System.Math.Max(_buttonWidth, System.Math.Max(DefaultWidth, minTextWidth));
        System.Single totalHeight = System.Math.Max(_buttonHeight, DefaultHeight);

        _panel.SetPosition(_position).SetSize(new Vector2f(totalWidth, totalHeight));
        _totalBounds = new FloatRect(_position.X, _position.Y, totalWidth, totalHeight);
        CENTER_LABEL(totalWidth, totalHeight);
    }

    /// <summary>
    /// Centers the label text within the button area.
    /// </summary>
    private void CENTER_LABEL(System.Single totalWidth, System.Single totalHeight)
    {
        var tb = _label.GetLocalBounds();
        System.Single x = _position.X + ((totalWidth - tb.Width) * 0.5f) - tb.Left;
        System.Single y = _position.Y + ((totalHeight - tb.Height) * 0.5f) - tb.Top + 8f;
        _label.Position = new Vector2f(x, y);
    }

    #endregion Layout

    #region Visual Helpers

    /// <summary>
    /// Updates the panel and text color/tint based on state (normal/hover/disabled).
    /// </summary>
    private void APPLY_TINT()
    {
        if (!_isEnabled)
        {
            _panel.SetTintColor(_panelDisabled);
            _label.FillColor = _textDisabled;
            return;
        }
        _panel.SetTintColor(_isHovered ? _panelHover : _panelNormal);
        _label.FillColor = _isHovered ? _textHover : _textNormal;
    }

    /// <summary>
    /// Triggers registered click callbacks.
    /// </summary>
    private void FIRE_CLICK() => OnClick?.Invoke();

    #endregion Visual Helpers
}
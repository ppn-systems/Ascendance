// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Assets;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.Extensions;
using Ascendance.Rendering.Input;
using Ascendance.Rendering.Layout;
using Ascendance.Rendering.UI.Theme;
using Ascendance.Shared.Abstractions;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Ascendance.Rendering.UI.Controls;

/// <summary>
/// A resizable button based on NineSlicePanel (single image), with text color changing on hover using tint.
/// Supports mouse and keyboard interactions, custom colors, and fluent configuration.
/// </summary>
public class Button : RenderObject, IUpdatable
{
    #region Constants

    private const System.Single DefaultHeight = 64f;
    private const System.Single DefaultWidth = 200f;
    private const System.UInt32 DefaultFontSize = 20;
    private const System.Single HorizontalPaddingDefault = 16f;

    private static readonly IntRect DefaultSrc = default;
    private static readonly Thickness DefaultSlice = new(32);

    #endregion Constants

    #region Fields

    private readonly Text _label;
    private readonly NineSlicePanel _panel;

    // States
    private System.Boolean _isHovered;
    private System.Boolean _isPressed;
    private System.Boolean _wasMousePressed;
    private System.Boolean _keyboardPressed;
    private System.Boolean _isEnabled = true;
    private System.Boolean _needsLayout = false;

    // Layout
    private FloatRect _totalBounds;
    private System.Single _buttonWidth;
    private Vector2f _position = new(0, 0);
    private System.Single _buttonHeight = DefaultHeight;
    private System.Single _horizontalPadding = HorizontalPaddingDefault;

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
    public Button(
        System.String text, Texture texture, Font font = null,
        System.Single width = 240f, IntRect sourceRect = default)
    {
        _buttonWidth = System.Math.Max(DefaultWidth, width);
        _label = new Text(text, font ?? EmbeddedAssets.JetBrainsMono.ToFont(), DefaultFontSize) { FillColor = Color.Black };
        _panel = new NineSlicePanel(texture, DefaultSlice, sourceRect == default ? DefaultSrc : sourceRect);

        this.UPDATE_LAYOUT();
        this.APPLY_TINT();
    }

    #endregion Constructor

    #region Public API (Fluent-Friendly)

    /// <summary>
    /// Sets button width (in pixels).
    /// </summary>
    public Button SetWidth(System.Single width)
    {
        _buttonWidth = width;
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets button height (in pixels).
    /// </summary>
    public Button SetHeight(System.Single height)
    {
        _buttonHeight = height;
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets both width and height of the button.
    /// </summary>
    public Button SetSize(System.Single width, System.Single height)
    {
        _buttonWidth = width;
        _buttonHeight = height;
        _needsLayout = true;
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets button label text.
    /// </summary>
    public Button SetText(System.String text)
    {
        _label.DisplayedString = text;
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets label font size.
    /// </summary>
    public Button SetFontSize(System.UInt32 size)
    {
        _label.CharacterSize = size;
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets horizontal padding inside button (pixels).
    /// </summary>
    public Button SetPadding(System.Single horizontalPadding)
    {
        _horizontalPadding = System.MathF.Max(0f, horizontalPadding);
        this.UPDATE_LAYOUT();
        return this;
    }

    /// <summary>
    /// Sets screen-space position for button's top-left.
    /// </summary>
    public void SetPosition(Vector2f position)
    {
        _position = position;
        this.UPDATE_LAYOUT();
    }

    /// <summary>
    /// Enables or disables the button interactively and visually.
    /// </summary>
    public Button SetEnabled(System.Boolean enabled)
    {
        _isEnabled = enabled;
        this.APPLY_TINT();
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
        if (_needsLayout)
        {
            this.UPDATE_LAYOUT();
            _needsLayout = false;
        }

        if (!this.IsVisible)
        {
            return;
        }

        Vector2i mousePos = MouseManager.Instance.GetMousePosition();
        System.Boolean isOver = _totalBounds.Contains(mousePos.X, mousePos.Y);
        System.Boolean isDown = Mouse.IsButtonPressed(Mouse.Button.Left);

        // Hover
        if (_isHovered != (isOver && _isEnabled))
        {
            _isHovered = isOver && _isEnabled;
            this.APPLY_TINT();
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
                this.FIRE_CLICK();
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
            if (keyDown && !_keyboardPressed)
            {
                _keyboardPressed = true;
            }
            else if (!keyDown && _keyboardPressed)
            {
                _keyboardPressed = false; this.FIRE_CLICK();
            }
        }
        else
        {
            _keyboardPressed = false;
        }
    }

    /// <summary>
    /// Renders the button and its label.
    /// </summary>
    public override void Draw(RenderTarget target)
    {
        if (!this.IsVisible)
        {
            return;
        }

        _panel.Draw(target);
        target.Draw(_label);
    }

    /// <summary>
    /// This button does not support GetDrawable (use Render).
    /// </summary>
    protected override Drawable GetDrawable() => throw new System.NotSupportedException("Use Render() instead.");

    #endregion

    #region Layout

    /// <summary>
    /// Updates panel and text geometry based on current layout/padding/text.
    /// </summary>
    private void UPDATE_LAYOUT()
    {
        // Ensure enough room for text + padding
        FloatRect tb = _label.GetLocalBounds();

        System.Single minTextWidth = tb.Width + (_horizontalPadding * 2f);
        System.Single minWidth = _panel.Border.Left + _panel.Border.Right;
        System.Single minHeight = _panel.Border.Top + _panel.Border.Bottom;

        System.Single totalWidth = System.Math.Max(_buttonWidth, System.Math.Max(DefaultWidth, minTextWidth));
        totalWidth = System.Math.Max(totalWidth, minWidth);

        System.Single totalHeight = System.Math.Max(_buttonHeight, DefaultHeight);
        totalHeight = System.Math.Max(totalHeight, minHeight);

        System.Single x = _position.X + ((totalWidth - tb.Width) * 0.5f) - tb.Left;
        System.Single y = _position.Y + ((totalHeight - tb.Height) * 0.5f) - tb.Top;


        _label.Position = new Vector2f(x, y);
        _panel.SetPosition(_position).SetSize(new Vector2f(totalWidth, totalHeight));
        _totalBounds = new FloatRect(_position.X, _position.Y, totalWidth, totalHeight);
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
            _label.FillColor = Themes.TextTheme.Disabled;
            _panel.SetTintColor(Themes.PanelTheme.Disabled);

            return;
        }

        _label.FillColor = _isHovered ? Themes.TextTheme.Hover : Themes.TextTheme.Normal;
        _panel.SetTintColor(_isHovered ? Themes.PanelTheme.Hover : Themes.PanelTheme.Normal);
    }

    /// <summary>
    /// Triggers registered click callbacks.
    /// </summary>
    private void FIRE_CLICK() => this.OnClick?.Invoke();

    #endregion Visual Helpers
}
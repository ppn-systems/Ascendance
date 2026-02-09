// Copyright (c) 2025 PPN Corporation. All rights reserved.

using Ascendance.Rendering.Engine;
using Ascendance.Rendering.Entities;
using Ascendance.Rendering.UI.Controls;
using SFML.Graphics;
using SFML.System;

namespace Ascendance.Desktop.Scenes.Login.View;

/// <summary>
/// Represents a view component that displays a back button in the bottom-left corner of the screen.
/// Provides navigation functionality to return to the previous scene.
/// </summary>
public sealed class BackButtonView : RenderObject
{
    #region Constants

    private const System.Single PaddingLeft = 30f;
    private const System.Single PaddingBottom = 30f;
    private const System.Single ButtonWidth = 140f;
    private const System.UInt32 ButtonFontSize = 16;

    #endregion Constants

    #region Fields

    private readonly Button _backButton;

    #endregion Fields

    #region Events

    /// <summary>
    /// Raised when the back button is clicked.
    /// </summary>
    public event System.Action BackRequested;

    #endregion Events

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="BackButtonView"/> class.
    /// Creates and positions the back button in the bottom-left corner.
    /// </summary>
    /// <param name="buttonText">The text to display on the button. Default is "Back".</param>
    /// <param name="zIndex">Z-index for sorting the render order. Default is 100.</param>
    public BackButtonView(System.String buttonText = "Back", System.Int32 zIndex = 100)
    {
        _backButton = new Button(buttonText, null, ButtonWidth)
        {
            FontSize = ButtonFontSize
        };

        _backButton.SetZIndex(zIndex);
        _backButton.RegisterClickHandler(this.ON_BACK_CLICKED);

        this.POSITION_BUTTON();
        base.SetZIndex(zIndex);
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Gets or sets whether the back button is enabled for interaction.
    /// </summary>
    public System.Boolean IsBackButtonEnabled
    {
        get => _backButton.IsEnabled;
        set => _backButton.IsEnabled = value;
    }

    /// <summary>
    /// Updates the back button state.
    /// </summary>
    /// <param name="dt">Delta time (thời gian đã trôi qua kể từ lần cập nhật trước).</param>
    public override void Update(System.Single dt)
    {
        if (!base.IsVisible)
        {
            return;
        }

        _backButton.Update(dt);
    }

    /// <summary>
    /// Renders the back button to the specified target if visible.
    /// </summary>
    /// <param name="target">The render target (đối tượng cùng loại với màn hình cần vẽ).</param>
    public override void Draw(RenderTarget target)
    {
        if (!base.IsVisible)
        {
            return;
        }

        _backButton.Draw(target);
    }

    /// <summary>
    /// Throws NotSupportedException. Use <see cref="Draw(RenderTarget)"/> for custom drawing logic.
    /// </summary>
    /// <returns>Never returns.</returns>
    protected override Drawable GetDrawable() => throw new System.NotSupportedException("Use Draw() instead of GetDrawable().");

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Positions the back button in the bottom-left corner of the screen.
    /// </summary>
    private void POSITION_BUTTON()
    {
        System.Single x = PaddingLeft;
        System.Single y = GraphicsEngine.ScreenSize.Y - _backButton.GlobalBounds.Height - PaddingBottom;

        _backButton.Position = new Vector2f(x, y);
    }

    /// <summary>
    /// Handles the back button click event and raises the BackRequested event.
    /// </summary>
    private void ON_BACK_CLICKED() => this.BackRequested?.Invoke();

    #endregion Private Methods
}
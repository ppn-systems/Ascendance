using Ascendance.Rendering.UI.Controls;

namespace Ascendance.Desktop.Scenes.Main.View;

public class ButtonView
{
    #region Fields

    private readonly Button _news;
    private readonly Button _login;
    private readonly Button _register;

    private readonly Button[] _buttons;

    #endregion Fields

    #region Constructor

    public ButtonView()
    {
        _news = new Button("News")
        {
            Position = new(50, 50),
            Size = new(200, 50)
        };
        _login = new Button("Login")
        {
            Position = new(50, 120),
            Size = new(200, 50)
        };
        _register = new Button("Register")
        {
            Position = new(50, 190),
            Size = new(200, 50)
        };

        _buttons = [_news, _login, _register];
    }

    #endregion Constructor
}

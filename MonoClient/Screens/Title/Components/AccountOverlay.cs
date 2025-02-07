using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.Title.Components.Panels;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components;

public class AccountOverlay : Sprite {
    private readonly bool _isTitle;

    private Container _currentAccount;
    private Container _newAccount;

    public AccountOverlay(bool title) {
        _isTitle = title;
        
        CreateAccountInfo();
        
        AddChild(Account.LoggedIn ? _currentAccount : _newAccount);
    }

    private void CreateAccountInfo() {
        _currentAccount = new Container();

        var nameConfig = new TextConfig { Text = $"logged in as {Account.Username} - ", FontSize = 24 };
        var nameText = new SimpleText(nameConfig);
        _currentAccount.AddChild(nameText);
        
        var logoutConfig = new TextButtonConfig { Text = "logout", FontSize = 24, OnClicked = OnLogout, X = nameText.Width };
        var logoutButton = new TextButton(logoutConfig);
        _currentAccount.AddChild(logoutButton);

        _newAccount = new Container();

        var newConfig = new TextConfig { Text = "new account - ", FontSize = 24, Bold = false, Color = 0xB3B3B3 };
        var newText = new SimpleText(newConfig);
        _newAccount.AddChild(newText);

        var registerConfig = new TextButtonConfig { Text = "register", FontSize = 24, OnClicked = () => OverlayManager.Enqueue(new RegisterContainer()), Bold = true, X = newText.Width };
        var registerButton = new TextButton(registerConfig);
        _newAccount.AddChild(registerButton);

        var dashConfig = new TextConfig { Text = " - ", FontSize = 24, X = registerButton.X + registerButton.Width, Color = 0xB3B3B3 };
        var dashText = new SimpleText(dashConfig);
        _newAccount.AddChild(dashText);

        var loginConfig = new TextButtonConfig { Text = "login", FontSize = 24, OnClicked = () => OverlayManager.Enqueue(new LoginContainer()), Bold = true, X = dashText.X + dashText.Width };
        var loginButton = new TextButton(loginConfig);
        _newAccount.AddChild(loginButton);
    }
    
    private void OnLogout() {
        Account.LogOut();

        if (_isTitle) {
            GTween.Add(Tween.New(_currentAccount, Easing.SineInOut, 150, 0f, EaseType.Alpha, onFinish: () => RemoveChild(_currentAccount)));
            _newAccount.Alpha = 0f;
            AddChild(_newAccount);
            GTween.Add(Tween.New(_newAccount, Easing.SineInOut, 150, 1f, EaseType.Alpha, 150));
        } else {
            ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 500, 0x0);
        }
    }
    
}
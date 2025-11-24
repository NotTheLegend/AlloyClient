using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Screens.Components.Containers;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.BuiltIn.Buttons;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Extra;
using AlloyClient.Models;

namespace AlloyClient.Screens.Components;

public class AccountOverlay : Sprite {
    
    private readonly bool _isTitle;
    
    private readonly Container _container = new();

    public AccountOverlay(bool title) {
        _isTitle = title;
        
        AddChild(_container);
        
        if (GlobalData.Contains<LoginData>()) {
            CreateLogout();
        } else {
            CreateLogin();
        }
    }
    
    public void OnLogin() {
        GTween.Add(Tween.New(_container, Easing.SineInOut, 150, 0f, EaseType.Alpha, onFinish: () => {
            CreateLogout();
            GTween.Add(Tween.New(_container, Easing.SineInOut, 150, 1f, EaseType.Alpha));
        }));
    }

    private void CreateLogin() {
        _container.RemoveChildren();

        var newConfig = new TextConfig { Text = "new account - ", FontSize = 24, FontType = FontType.Normal, Color = 0xB3B3B3 };
        var newText = new SimpleText(newConfig);
        _container.AddChild(newText);

        var registerConfig = new TextButtonConfig { Text = "register", FontSize = 24, OnClicked = () => OverlayManager.Set(new RegisterContainer()), FontType = FontType.Bold, X = newText.Width };
        var registerButton = new TextButton(registerConfig);
        _container.AddChild(registerButton);

        var dashConfig = new TextConfig { Text = " - ", FontSize = 24, X = registerButton.X + registerButton.Width, Color = 0xB3B3B3 };
        var dashText = new SimpleText(dashConfig);
        _container.AddChild(dashText);

        var loginConfig = new TextButtonConfig { Text = "login", FontSize = 24, OnClicked = () => {
            var login = new LoginContainer();
            login.AddEventListener("loginSuccess", OnLogin);
            OverlayManager.Set(login);
        }, FontType = FontType.Bold, X = dashText.X + dashText.Width };
        var loginButton = new TextButton(loginConfig);
        _container.AddChild(loginButton);
    }

    private void CreateLogout() {
        _container.RemoveChildren();

        var account = GlobalData.Get<AccountData>();
        
        var nameConfig = new TextConfig { Text = $"logged in as {account.Name} - ", FontSize = 24 };
        var nameText = new SimpleText(nameConfig);
        _container.AddChild(nameText);
        
        var logoutConfig = new TextButtonConfig { Text = "logout", FontSize = 24, OnClicked = OnLogout, X = nameText.Width };
        var logoutButton = new TextButton(logoutConfig);
        _container.AddChild(logoutButton);
    }
    
    private void OnLogout() {
        GlobalData.Logout();

        if (_isTitle) {
            GTween.Add(Tween.New(_container, Easing.SineInOut, 150, 0f, EaseType.Alpha, onFinish: () => {
                CreateLogin();
                GTween.Add(Tween.New(_container, Easing.SineInOut, 150, 1f, EaseType.Alpha));
            }));
        } else {
            ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 500, 0x0);
        }
    }
    
}
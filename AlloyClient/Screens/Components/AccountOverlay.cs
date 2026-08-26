using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Screens.Components.Containers.Account;
using AlloyClient.Ui.Components.Buttons;

namespace AlloyClient.Screens.Components;

public class AccountOverlay : Sprite {

    public readonly static EventType<Event> AccountChangedEvent = "accountChanged";

    private const int LinkSpacing = 8;

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
            DispatchEvent(new Event(AccountChangedEvent));
            GTween.Add(Tween.New(_container, Easing.SineInOut, 150, 1f, EaseType.Alpha));
        }));
    }

    private void CreateLogin() {
        _container.RemoveChildren();

        var newConfig = new TextConfig { Text = "new account", FontSize = 24, FontType = FontType.Normal, Color = 0xB3B3B3 };
        var newText = new SimpleText(newConfig);
        _container.AddChild(newText);

        var newDashConfig = new TextConfig { Text = "-", FontSize = 24, X = newText.Width + LinkSpacing, Color = 0xB3B3B3 };
        var newDashText = new SimpleText(newDashConfig);
        _container.AddChild(newDashText);

        var registerConfig = new TextButtonConfig {
            Text = "register", FontSize = 24, OnClicked = () => OverlayManager.Set(new RegisterContainer()), FontType = FontType.Bold,
            X = newDashText.X + newDashText.Width + LinkSpacing
        };

        var registerButton = new TextButton(registerConfig);
        _container.AddChild(registerButton);

        var dashConfig = new TextConfig
            { Text = "-", FontSize = 24, X = registerButton.X + registerButton.Width + LinkSpacing, Color = 0xB3B3B3 };

        var dashText = new SimpleText(dashConfig);
        _container.AddChild(dashText);

        var loginConfig = new TextButtonConfig {
            Text = "login", FontSize = 24, OnClicked = () => {
                var login = new LoginContainer();
                login.AddEventListener(LoginContainer.LoginEvent, OnLogin);
                OverlayManager.Set(login);
            },
            FontType = FontType.Bold, X = dashText.X + dashText.Width + LinkSpacing
        };

        var loginButton = new TextButton(loginConfig);
        _container.AddChild(loginButton);
    }

    private void CreateLogout() {
        _container.RemoveChildren();

        var account = GlobalData.Get<AccountData>();
        if (account == null) {
            // We're not properly logged in
            CreateLogin();
            return;
        }

        var nameConfig = new TextConfig {
            Text = $"logged in as {account.Name}",
            FontSize = 24,
            FontType = FontType.Normal,
            Color = 0xB3B3B3
        };

        var nameText = new SimpleText(nameConfig);
        _container.AddChild(nameText);

        var dashConfig = new TextConfig {
            Text = "-",
            FontSize = 24,
            X = nameText.Width + LinkSpacing,
            Color = 0xB3B3B3
        };

        var dashText = new SimpleText(dashConfig);
        _container.AddChild(dashText);

        var logoutConfig = new TextButtonConfig {
            Text = "logout",
            FontSize = 24,
            FontType = FontType.Bold,
            OnClicked = OnLogout,
            X = dashText.X + dashText.Width + LinkSpacing
        };

        var logoutButton = new TextButton(logoutConfig);
        _container.AddChild(logoutButton);
    }

    private void OnLogout() {
        GlobalData.Logout();
        DispatchEvent(new Event(AccountChangedEvent));

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
using AlloyClient.AppEngine;
using AlloyClient.Display;
using AlloyClient.State;
using AlloyClient.Ui.Components.Dialogs;
using AlloyClient.Ui.Components.Panels;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.BuiltIn.Buttons;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Extra;
using AlloyClient.Models;
using AlloyClient.UiLib.Core;

namespace AlloyClient.Screens.Components.Containers;

public class LoginContainer : Overlay {
    private readonly TextInput _emailInput;
    private readonly TextInput _passwordInput;

    public LoginContainer() {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);
        
        var background = new ColorRect(new ColorRectConfig { Width = 475, Height = 350, Color = 0x363636 });
        AddChild(background);
        
        var titleBackground = new ColorRect(new ColorRectConfig { Width = 475, Height = 50, Color = 0x4d4d4d });
        AddChild(titleBackground);

        var title = new SimpleText(new TextConfig { Text = "Log in", FontSize = 22, FontType = FontType.Bold, X = Width / 2, Y = titleBackground.Height / 2, Color = 0xFFFFFF, Anchor = UiAnchor.Middle });
        AddChild(title);

        var emailConfig = new InputConfig { X = Width / 2, Y = 100, FontSize = 24, FontType = FontType.Bold, Color = 0xFFFFFF, Width = 350, DefaultText = "Username", Anchor = UiAnchor.Middle };
        _emailInput = new TextInput(emailConfig);
        AddChild(_emailInput);

        var passwordConfig = new InputConfig { X = Width / 2, Y = 160, FontSize = 24, FontType = FontType.Bold, Color = 0xFFFFFF, Width = 350, DefaultText = "Password", Password = true, Anchor = UiAnchor.Middle };
        _passwordInput = new TextInput(passwordConfig);
        AddChild(_passwordInput);

        var registerConfig = new TextButtonConfig { Text = "New user? Click here to Register!", FontSize = 16, OnClicked = () => { OverlayManager.Set(new RegisterContainer()); }, FontType = FontType.Bold, X = Width / 2, Y = _passwordInput.Y + 40, Anchor = UiAnchor.Middle };
        var registerButton = new TextButton(registerConfig);
        AddChild(registerButton);
        
        var loginConfig = new TextButtonConfig { Text = "Log in", FontSize = 28, OnClicked = OnLogin, FontType = FontType.Normal, X = Width - 25, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var loginButton = new TextButton(loginConfig);
        AddChild(loginButton);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 28, OnClicked = CloseOverlay, FontType = FontType.Normal, X = loginButton.X - loginButton.Width - 35, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
    }

    private void OnLogin() {
        AddEventListener(AppRequests.VerifyAsync(_emailInput.Text, _passwordInput.Text, true), OnLoginResponse);
    }
    
    private void OnLoginResponse(AppResponse response) {
        if (!response.Success) {
            var dialog = new Dialog("Login Error", response.Message, new DialogOption("Ok"));
            DialogManager.Enqueue(dialog);
            return;
        }
        CloseOverlay();
        DispatchEvent(new Event("loginSuccess"));
    }
}
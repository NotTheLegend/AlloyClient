using MonoClient.Data;
using MonoClient.Display;
using MonoClient.State;
using MonoClient.Ui.Components.Dialogs;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components.Panels;

public class LoginContainer : Overlay {
    private readonly TextInput _emailInput;
    private readonly TextInput _passwordInput;

    public LoginContainer() {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetBaseDimensions(475, 350);
        SetAnchor(UiAnchor.Middle);
        
        var background = new ColorRect(new ColorRectConfig { Width = 475, Height = 350, Color = 0x363636 });
        AddChild(background);
        
        var titleBackground = new ColorRect(new ColorRectConfig { Width = 475, Height = 50, Color = 0x4d4d4d });
        AddChild(titleBackground);

        var title = new SimpleText(new TextConfig { Text = "Log in", FontSize = 22, Bold = true, X = Width / 2, Y = titleBackground.Height / 2, Color = 0xFFFFFF, Anchor = UiAnchor.Middle });
        AddChild(title);

        var emailConfig = new InputConfig { X = Width / 2, Y = 100, FontSize = 24, Bold = true, Color = 0xFFFFFF, Width = 350, DefaultText = "Email", Anchor = UiAnchor.Middle };
        _emailInput = new TextInput(emailConfig);
        AddChild(_emailInput);

        var passwordConfig = new InputConfig { X = Width / 2, Y = 160, FontSize = 24, Bold = true, Color = 0xFFFFFF, Width = 350, DefaultText = "Password", Password = true, Anchor = UiAnchor.Middle };
        _passwordInput = new TextInput(passwordConfig);
        AddChild(_passwordInput);

        var registerConfig = new TextButtonConfig { Text = "New user? Click here to Register!", FontSize = 16, OnClicked = () => { CloseOverlay(); OverlayManager.Enqueue(new RegisterContainer()); }, Bold = true, X = Width / 2, Y = _passwordInput.Y + 40, Anchor = UiAnchor.Middle };
        var registerButton = new TextButton(registerConfig);
        AddChild(registerButton);
        
        var forgotConfig = new TextButtonConfig { Text = "Forgot Password?", FontSize = 16, OnClicked = () => { CloseOverlay(); OverlayManager.Enqueue(new ForgotContainer()); }, Bold = true, X = Width / 2, Y = registerButton.Y + 30, Anchor = UiAnchor.Middle };
        var forgotButton = new TextButton(forgotConfig);
        AddChild(forgotButton);
        
        var loginConfig = new TextButtonConfig { Text = "Log in", FontSize = 28, OnClicked = OnLogin, Bold = false, X = Width - 25, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var loginButton = new TextButton(loginConfig);
        AddChild(loginButton);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 28, OnClicked = CloseOverlay, Bold = false, X = loginButton.X - loginButton.Width - 35, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
    }

    private void OnLogin() {
        AddEventListener(TaskEvent.Completed, Account.LoginAsync(_emailInput.Text, _passwordInput.Text), OnLoginResponse);
    }
    
    private void OnLoginResponse(LoginResponse response) {
        if (!response.Success) {
            var dialog = new Dialog("Login Error", response.Message, new DialogOption("Ok"));
            DialogManager.Enqueue(dialog);
            return;
        }
        CloseOverlay();
        ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 500, 0x0);
    }
}
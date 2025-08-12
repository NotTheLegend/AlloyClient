using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.BuiltIn.Buttons;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;
using RealmClient.Data;
using RealmClient.Display;
using RealmClient.State;
using RealmClient.Ui.Components.Dialogs;
using RealmClient.Ui.Components.Panels;

namespace RealmClient.Screens.Title.Components.Panels;

public class RegisterContainer : Overlay {
    
    private readonly TextInput _emailInput;
    private readonly TextInput _passwordInput;
    
    public RegisterContainer() {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);
        
        var background = new ColorRect(new ColorRectConfig { Width = 475, Height = 350, Color = 0x363636 });
        AddChild(background);
        
        var titleBackground = new ColorRect(new ColorRectConfig { Width = 475, Height = 50, Color = 0x4d4d4d });
        AddChild(titleBackground);

        var title = new SimpleText(new TextConfig { Text = "Register", FontSize = 22, FontType = FontType.Bold, X = Width / 2, Y = titleBackground.Height / 2, Color = 0xFFFFFF, Anchor = UiAnchor.Middle });
        AddChild(title);
        
        
        var emailConfig = new InputConfig { X = Width / 2, Y = 100, FontSize = 24, FontType = FontType.Bold, Color = 0xFFFFFF, Width = 350, DefaultText = "Email", Anchor = UiAnchor.Middle };
        _emailInput = new TextInput(emailConfig);
        AddChild(_emailInput);

        var passwordConfig = new InputConfig { X = Width / 2, Y = 160, FontSize = 24, FontType = FontType.Bold, Color = 0xFFFFFF, Width = 350, DefaultText = "Password", Password = true, Anchor = UiAnchor.Middle };
        _passwordInput = new TextInput(passwordConfig);
        AddChild(_passwordInput);
        
        //todo register fields
        
        var loginConfig = new TextButtonConfig { Text = "Create", FontSize = 28, OnClicked = OnRegister, FontType = FontType.Normal, X = 475 - 25, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var loginButton = new TextButton(loginConfig);
        AddChild(loginButton);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 28, OnClicked = CloseOverlay, FontType = FontType.Normal, X = loginButton.X - loginButton.Width - 35, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
    }
    
    private void OnRegister() {
        AddEventListener(Account.Register(_emailInput.Text, _passwordInput.Text), OnLoginResponse);
    }
    
    private void OnLoginResponse(LoginResponse response) {
        if (!response.Success) {
            var dialog = new Dialog("Register Error", response.Message, new DialogOption("Ok"));
            DialogManager.Enqueue(dialog);
            return;
        }
        
        
        CloseOverlay();
        ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 500, 0x0);
    }
}
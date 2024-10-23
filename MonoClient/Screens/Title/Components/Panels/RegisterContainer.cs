using MonoClient.Display;
using MonoClient.State;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components.Panels;

public class RegisterContainer : Panel {
    public RegisterContainer() {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);
        
        var background = new ColorRect(new ColorRectConfig { Width = 475, Height = 350, Color = 0x363636 });
        AddChild(background);
        
        var titleBackground = new ColorRect(new ColorRectConfig { Width = 475, Height = 50, Color = 0x4d4d4d });
        AddChild(titleBackground);

        var title = new SimpleText(new TextConfig { Text = "Register", FontSize = 22, Bold = true, X = Width / 2, Y = titleBackground.Height / 2, Color = 0xFFFFFF, Anchor = UiAnchor.Middle });
        AddChild(title);
        
        //todo register fields
        
        var loginConfig = new TextButtonConfig { Text = "Create", FontSize = 28, OnClicked = OnRegister, Bold = false, X = 475 - 25, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var loginButton = new TextButton(loginConfig);
        AddChild(loginButton);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 28, OnClicked = () => State = PanelState.Closed, Bold = false, X = loginButton.X - loginButton.Width - 35, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
    }

    private void OnRegister() {
        PanelManager.Enqueue(new ForgotContainer());
        State = PanelState.Closed;
    }
    
}
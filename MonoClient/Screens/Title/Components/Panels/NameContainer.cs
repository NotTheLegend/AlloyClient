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

public class NameContainer : Overlay {

    private readonly TextInput _nameInput;
    
    public NameContainer() {
        X = Settings.DefaultScreenWidth / 2;
        Y = Settings.DefaultScreenHeight / 2;
        SetAnchor(UiAnchor.Middle);
        
        var background = new ColorRect(new ColorRectConfig { Width = 475, Height = 350, Color = 0x363636 });
        AddChild(background);
        
        var titleBackground = new ColorRect(new ColorRectConfig { Width = 475, Height = 50, Color = 0x4d4d4d });
        AddChild(titleBackground);

        var title = new SimpleText(new TextConfig { Text = "Set Name", FontSize = 22, Bold = true, X = Width / 2, Y = titleBackground.Height / 2, Color = 0xFFFFFF, Anchor = UiAnchor.Middle });
        AddChild(title);
        
        var nameConfig = new InputConfig { X = Width / 2, Y = 100, FontSize = 24, Bold = true, Color = 0xFFFFFF, Width = 350, DefaultText = "Name", Anchor = UiAnchor.Middle };
        _nameInput = new TextInput(nameConfig);
        AddChild(_nameInput);
        
        var loginConfig = new TextButtonConfig { Text = "Name", FontSize = 28, OnClicked = OnSetName, Bold = false, X = 475 - 25, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var loginButton = new TextButton(loginConfig);
        AddChild(loginButton);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 28, OnClicked = CloseOverlay, Bold = false, X = loginButton.X - loginButton.Width - 35, Y = Height - 25, Anchor = UiAnchor.RightBottom };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
    }
    
    private void OnSetName() {
        AddEventListener(TaskEvent.Completed, Account.SetName(_nameInput.Text), OnLoginResponse);
    }
    
    private void OnLoginResponse(LoginResponse response) {
        if (!response.Success) {
            var dialog = new Dialog("Name Error", response.Message, new DialogOption("Ok"));
            DialogManager.Enqueue(dialog);
            return;
        }
        
        CloseOverlay();
        ScreenManager.FadeToScreen(new CharacterListScreen(), Easing.SineInOut, 500, 0x0);
    }
    
}
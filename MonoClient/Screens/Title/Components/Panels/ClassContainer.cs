using Microsoft.Xna.Framework;
using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.Game;
using MonoClient.Screens.Title.Components.CharacterList;
using MonoClient.State;
using MonoClient.Ui.Components.Dialogs;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components.Panels;

public class ClassContainer : Panel {

    public CharacterWheel CharacterWheel;
    public ClassInfo ClassInfo;
    public ushort ClassType { get; set; }
    
    public ClassContainer() {
        CharacterWheel = new CharacterWheel();
        ClassInfo = new ClassInfo();
        
        AddChild(ClassInfo);
        AddChild(CharacterWheel);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 50, OnClicked = ClosePanel, Bold = false, X = 75, Y = 650 };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
        
        var slotConfig = new TextButtonConfig { Text = "Play", FontSize = 50, OnClicked = () => {
            ClassType = CharacterWheel.SelectedClass.Type;
            AddEventListener(TaskEvent.Completed, Account.PurchaseClassUnlock(ClassType), OnCreateResponse);
        }, Bold = false, X = 1000, Y = 360 };
        var slotButton = new TextButton(slotConfig);
        
        AddChild(slotButton);
    }

    private void OnCreateResponse(LoginResponse response) {
        if (!response.Success) {
            var dialog = new Dialog("You can't afford this Class.", response.Message, new DialogOption("Ok"));
            DialogManager.Enqueue(dialog);
            return;
        }
        
        Account.CharacterType = ClassType;
        ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
        
        ClosePanel();
    }
    
    protected override void OnUpdate(GameTime gameTime) {
        ClassInfo.Update(gameTime, CharacterWheel.SelectedClass);
    }
}
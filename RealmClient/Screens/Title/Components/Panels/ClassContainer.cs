using Common;
using RealmClient.Screens.Title.Components.CharacterList;
using RealmClient.UiLib.BuiltIn.Buttons;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;
using RealmClient.Data;
using RealmClient.Display;
using RealmClient.Screens.Game;
using RealmClient.Screens.Title.Components.CharacterSelection;
using RealmClient.Ui.Components.Dialogs;
using RealmClient.Ui.Components.Panels;

namespace RealmClient.Screens.Title.Components.Panels;

public class ClassContainer : Overlay {

    public CharacterWheel CharacterWheel;
    public ClassInfo ClassInfo;
    public ushort ClassType { get; set; }
    
    public ClassContainer() {
        CharacterWheel = new CharacterWheel();
        ClassInfo = new ClassInfo();
        
        AddChild(ClassInfo);
        AddChild(CharacterWheel);
        
        var cancelConfig = new TextButtonConfig { Text = "Cancel", FontSize = 50, OnClicked = CloseOverlay, FontType = FontType.Normal, X = 75, Y = 650 };
        var cancelButton = new TextButton(cancelConfig);
        AddChild(cancelButton);
        
        var slotConfig = new TextButtonConfig { Text = "Play", FontSize = 50, OnClicked = () => {
            ClassType = CharacterWheel.SelectedClass.Type;
            AddEventListener(Account.PurchaseClassUnlock(ClassType), OnCreateResponse);
        }, FontType = FontType.Normal, X = 1000, Y = 360 };
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
        
        CloseOverlay();
    }
    
    protected override void OnUpdate(GameTime gameTime) {
        ClassInfo.Update(gameTime, CharacterWheel.SelectedClass);
    }
}
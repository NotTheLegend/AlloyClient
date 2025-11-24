using RealmClient.Data;
using RealmClient.Display;
using RealmClient.Game;
using RealmClient.Screens.Components.CharacterSelection;
using RealmClient.Ui.Components.Panels;
using RealmClient.UiLib.BuiltIn.Buttons;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Extra;

namespace RealmClient.Screens.Components.Containers;

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
            GlobalData.CharacterType = ClassType;
            ScreenManager.FadeToScreen(new GameScreen(), Easing.SineInOut, 1000, 0x0);
        
            CloseOverlay();
        }, FontType = FontType.Normal, X = 1000, Y = 360 };
        var slotButton = new TextButton(slotConfig);
        
        AddChild(slotButton);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }
    
    private void OnFrameEnter() {
        ClassInfo.Update(Stage.GameTime, CharacterWheel.SelectedClass);
    }
}
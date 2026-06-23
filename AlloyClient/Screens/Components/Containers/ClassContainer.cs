using AlloyClient.Data;
using AlloyClient.Display;
using AlloyClient.Game;
using AlloyClient.Screens.Components.CharacterSelection;
using AlloyClient.Ui.Components.Panels;
using Alloy.UiLib.Core;
using Alloy.UiLib.Extra;
using AlloyClient.Ui.Components.Buttons;

namespace AlloyClient.Screens.Components.Containers;

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
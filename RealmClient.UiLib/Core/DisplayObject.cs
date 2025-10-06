namespace RealmClient.UiLib.Core;

public abstract class DisplayObject : EventManager {
    
    internal DisplayObject() { }
    
    public Stage Stage { get; private set; }
    
    public Sprite Parent { get; internal set; }

    internal float SelfContentWidth;

    internal float SelfContentHeight;
    
    internal float ContentWidth;

    internal float ContentHeight;

    internal void SetStageReference(Stage stage) {
        Stage = stage;
    }
}
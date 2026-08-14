using Alloy.UiLib.Core;

namespace AlloyClient.Ui.Components;

public abstract class UiElement : Sprite {
    
    protected UiElement() {
        AddEventListener(Event.AddedToStage, InternalOnAddedToStage);
        AddEventListener(Event.RemovedFromStage, InternalOnRemovedFromStage);
    }

    private void InternalOnAddedToStage() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnAddedToStage();
        OnResize(new ResizeEvent(ResizeEvent.Resize, Stage.StageWidth, Stage.StageHeight));
    }

    private void InternalOnRemovedFromStage() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
        OnRemovedFromStage();
    }

    protected virtual void OnAddedToStage() {}
    protected virtual void OnRemovedFromStage() {}
    protected virtual void OnResize(ResizeEvent args) {}
}
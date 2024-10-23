using MonoClient.Ui;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class StatusBarButton : Sprite {
    private Sprite _skillButton;
    private Sprite _fameButton;

    public StatusBarButton(int w, int h) {
        SetBaseDimensions(w, h);
        MouseEnabled = true;
        _skillButton = new NineSliceRect(new NineSliceConfig {SliceData = SliceConfig.StatusBar, Padding = false, CutX = 10, CutY = 10, Width = w, Height = h });
        _skillButton.SetColor(0x1A9BE0);
        AddChild(_skillButton);
        //todo add icons
        _fameButton = new NineSliceRect(new NineSliceConfig {SliceData = SliceConfig.StatusBar, Padding = false, CutX = 10, CutY = 10, Width = w, Height = h });
        _fameButton.SetColor(14835456);
        AddChild(_fameButton);

        _fameButton.Visible = false;

        MouseEnabled = true;
        AddEventListener(MouseEventId.LeftClick, OnClick);
        AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        AddEventListener(MouseEventId.MouseOut, OnMouseOut);
    }

    private void OnClick() {
        _skillButton.Visible = !_skillButton.Visible;
        _fameButton.Visible= !_fameButton.Visible;
    }

    private void OnMouseOver() {
        _skillButton.ColorTransformation = ColorTransform.Bright;
        _fameButton.ColorTransformation = ColorTransform.Bright;
    }

    private void OnMouseOut() {
        _skillButton.ColorTransformation = ColorTransform.Default;
        _fameButton.ColorTransformation = ColorTransform.Default;
    }
}
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Screens.Game.Components.Hud;

public class CharacterBars : Sprite {
    private readonly StatusBarButton _expSwitch;
    private readonly StatusBar _expBar;
    private readonly StatusBar _fameBar;
    private readonly StatusBar _skillExpBar;
    private readonly StatusBar _hpBar;
    private readonly StatusBar _shieldBar;
    private readonly StatusBar _mpBar;

    private bool _showExpBar = true;

    public CharacterBars() {
        _expSwitch = new StatusBarButton(30, 20);
        _expBar = new StatusBar(172, 20, 5931045, 5526612, 0xFFFFFF, "Lvl X");
        _fameBar = new StatusBar(172, 20, 14835456, 5526612, 0xFFFFFF, "Fame");
        _skillExpBar = new StatusBar(172, 20, 0x1A9BE0, 5526612, 0xFFFFFF, "SXP");
        _hpBar = new StatusBar(212, 20, 14693428, 5526612, 0xFFFFFF, "HP");
        _shieldBar = new StatusBar(212, 20, 0xC0C0C0, 5526612, 0xFFFFFF, "Shield");
        _mpBar = new StatusBar(212, 20, 6325472, 5526612, 0xFFFFFF, "MP");
        _expSwitch.Visible = false;
        _expBar.Visible = true;
        _fameBar.Visible = false;
        _skillExpBar.Visible = false;
        AddChild(_expSwitch);
        AddChild(_expBar);
        AddChild(_fameBar);
        AddChild(_skillExpBar);
        AddChild(_hpBar);
        AddChild(_shieldBar);
        AddChild(_mpBar);

        SetPositions(true);

        _expSwitch.AddEventListener(MouseEventId.LeftClick, SwitchBars);
    }

    private void SetPositions(bool show) {
        if (show) {
            _expSwitch.X = 176;
            _expSwitch.Y = -4;
            _expBar.Y = -4;
            _fameBar.Y = -4;
            _skillExpBar.Y = -4;
            _hpBar.Y = 15;
            _shieldBar.Y = 34;
            _mpBar.Y = 53;
        } else {
            _expSwitch.X = 146;
            _expSwitch.Y = 0;
            _expBar.Y = 0;
            _fameBar.Y = 0;
            _skillExpBar.Y = 0;
            _hpBar.Y = 24;
            _shieldBar.Y = 24;
            _mpBar.Y = 48;
            _shieldBar.Visible = false;
        }
    }

    public void Update() {
        if (Map.LocalPlayer == null) return;

        var player = Map.LocalPlayer;

        if (_showExpBar && player.Level == 20)
            DisableExpBar();

        if (_expBar.Visible) {
            _expBar.Update(player.Experience, 0);
        }

        _hpBar.Update(player.Hp, player.MaxHp, player.MaxHpBoost, 250, player.Level);
        _mpBar.Update(player.Mp, player.MaxMp, player.MaxMpBoost, 250, player.Level);
    }

    private void SwitchBars() {
        _fameBar.Visible = !_fameBar.Visible;
        _skillExpBar.Visible = !_skillExpBar.Visible;
    }

    private void DisableExpBar() {
        _expSwitch.Visible = true;
        _expBar.Visible = false;
        _fameBar.Visible = true;
    }
}
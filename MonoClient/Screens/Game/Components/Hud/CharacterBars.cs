using MonoClient.Objects;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;

namespace MonoClient.Screens.Game.Components.Hud;

public class CharacterBars : Sprite {

    public int height = 20;
    public int offset = 7;

    private readonly StatusBar _expBar;
    private readonly StatusBar _fameBar;
    private readonly StatusBar _hpBar;
    private readonly StatusBar _mpBar;

    public CharacterBars() {
        _expBar = new StatusBar(210, height, 5931045, 5526612, 0xFFFFFF, "Lvl X");
        _fameBar = new StatusBar(210, height, 14835456, 5526612, 0xFFFFFF, "Fame");
        _hpBar = new StatusBar(210, height, 14693428, 5526612, 0xFFFFFF, "HP");
        _mpBar = new StatusBar(210, height, 6325472, 5526612, 0xFFFFFF, "MP");
        _fameBar.Visible = false;
        AddChild(_hpBar);
        AddChild(_mpBar);
        AddChild(_fameBar);
        AddChild(_expBar);
        SetPositions();
    }

    private void SetPositions() {
        _fameBar.Y = height  * 0 + offset * 0; 
        _expBar.Y = height  * 0 + offset * 0; 
        _hpBar.Y = height * 1 + offset * 1; 
        _mpBar.Y = height * 2 + offset * 2;
    }

    public void Update() {

        if (Map.LocalPlayer == null) return;

        var player = Map.LocalPlayer;
        string lvlText = "Lvl " + player.Level;

        if( lvlText != _expBar.labelString)
        {
            _expBar.UpdateLabel(lvlText);
        }

        if (!_fameBar.Visible && player.Level == 20)
            DisableExpBar();

        if (_expBar.Visible) {
            _expBar.Update(player.Experience, player.NextLevelExp);
        }

        if(_fameBar.Visible) {
            _expBar.Update(player.CurrentFame, player.FameGoal);
        }

        _hpBar.Update(player.Hp, player.MaxHp, player.MaxHpBoost, 250, player.Level);
        _mpBar.Update(player.Mp, player.MaxMp, player.MaxMpBoost, 250, player.Level);
    }
    private void DisableExpBar()
    {
        _expBar.Visible = false;
        _fameBar.Visible = true;
    }
}
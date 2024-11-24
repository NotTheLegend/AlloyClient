using System;
using System.Collections.Generic;
using MonoClient.Screens.Game.Components.Options.OptionTypes;
using MonoClient.State;
using MonoClient.State.SettingTypes;
using MonoClient.Ui.Components.Scrollbars;
using MonoClient.UiLib.BuiltIn;

namespace MonoClient.Screens.Game.Components.Options;

public class OptionTabView : Container {
    private static readonly string[] OnOffLabels = ["On", "Off"];
    private static readonly object[] OnOffValues = [true, false];
    
    private readonly OptionsView _optionsView;

    private readonly Container _container;
    private readonly List<Option> _options = [];
    private readonly VerticalScrollBar _scrollbar;
    
    public OptionTabView(string name) : base(new ContainerConfig { Width = 1280, Height = 507, EnableClip = true }) {
        _container = new Container();
        AddChild(_container);

        switch (name) {
            case OptionsView.ControlsTab:
                AddControlsOptions();
                break;
            case OptionsView.HotkeysTab:
                AddHotkeysOptions();
                break;
            case OptionsView.ChatTab:
                AddChatOptions();
                break;
            case OptionsView.GraphicsTab:
                AddGraphicsOptions();
                break;
            case OptionsView.SoundTab:
                AddSoundOptions();
                break;
            case OptionsView.ExtraTab:
                AddExtraOptions();
                break;
        }

        PositionChildren();

        if (_container.Height >= Height) {
            _scrollbar = new VerticalScrollBar(this, new VerticalScrollBarConfig {
                X = Settings.DefaultScreenWidth - 15,
                Width = 15,
                Height = Height,
                TotalContentHeight = _container.Height,
                VisibleContentHeight = Height,
                OnValueChanged = val => _container.Y = -val
            });
            AddChild(_scrollbar);
        }
    }

    private void PositionChildren() {
        var i = 0f;
        foreach (var option in _options) {
            if (option == null) {
                continue;
            }

            option.X += i % 2 == 0 ? 32 : 664;
            option.Y += (int) (i / 2) * 70 + 22;

            _container.AddChild(option);

            i++;
        }
    }

    public void Refresh() {
        foreach (var option in _options) {
            option?.Refresh();
        }
    }

    private void AddControlsOptions() {
        _options.Add(new KeyMapperOption(Settings.MoveUp, "Move Up", "Key to will move character up"));
        _options.Add(new KeyMapperOption(Settings.MoveLeft, "Move Left", "Key to will move character to the left"));
        _options.Add(new KeyMapperOption(Settings.MoveDown, "Move Down", "Key to will move character down"));
        _options.Add(new KeyMapperOption(Settings.MoveRight, "Move Right", "Key to will move character to the right"));
        _options.Add(new ChoiceOption<bool>(Settings.AllowRotation, OnOffLabels, OnOffValues, "Allow Camera Rotation", "Toggles whether to allow for camera rotation"));
        _options.Add(null);
        _options.Add(new KeyMapperOption(Settings.RotateLeft, "Rotate Left", "Key to will rotate the camera to the left"));
        _options.Add(new KeyMapperOption(Settings.RotateRight, "Rotate Right", "Key to will rotate the camera to the right"));
        _options.Add(new KeyMapperOption(Settings.Special, "Use Special Ability", "This key will activate your special ability"));
        _options.Add(new KeyMapperOption(Settings.AutoFire, "Autofire Toggle", "This key will toggle autofire"));
        _options.Add(new KeyMapperOption(Settings.ResetCameraAngle, "Reset To Default Camera Angle", "This key will reset the camera angle to the default position"));
        _options.Add(new KeyMapperOption(Settings.PerformanceStats, "Toggle Performance Stats", "This key will toggle a display of fps and memory usage"));
        _options.Add(new KeyMapperOption(Settings.CenterPlayerKey, "Toggle Centering of Player", "This key will toggle the position between centered and offset"));
        _options.Add(new KeyMapperOption(Settings.Interact, "Interact/Buy", "This key will allow you to enter a portal or buy an item without using your mouse."));
        _options.Add(new KeyMapperOption(Settings.Focus, "Focus/Crouch", "Toggles your speed to 15. To use held focus find the option in the Extra tab."));
    }

    private void AddHotkeysOptions() {
        _options.Add(new KeyMapperOption(Settings.HealthPotion, "Use Health Potion", "This key will use health potions if available"));
        _options.Add(new KeyMapperOption(Settings.MagicPotion, "Use Magic Potion", "This key will use magic potions if available"));
        _options.Add(new KeyMapperOption(Settings.InvOne, "Use Inventory Slot 1", "Use item in inventory slot 1"));
        _options.Add(new KeyMapperOption(Settings.InvTwo, "Use Inventory Slot 2", "Use item in inventory slot 2"));
        _options.Add(new KeyMapperOption(Settings.InvThree, "Use Inventory Slot 3", "Use item in inventory slot 3"));
        _options.Add(new KeyMapperOption(Settings.InvFour, "Use Inventory Slot 4", "Use item in inventory slot 4"));
        _options.Add(new KeyMapperOption(Settings.InvFive, "Use Inventory Slot 5", "Use item in inventory slot 5"));
        _options.Add(new KeyMapperOption(Settings.InvSix, "Use Inventory Slot 6", "Use item in inventory slot 6"));
        _options.Add(new KeyMapperOption(Settings.InvSeven, "Use Inventory Slot 7", "Use item in inventory slot 7"));
        _options.Add(new KeyMapperOption(Settings.InvEight, "Use Inventory Slot 8", "Use item in inventory slot 8"));
        _options.Add(new KeyMapperOption(Settings.Escape, "Escape To Nexus", "This key will instantly escape you to the Nexus"));
        _options.Add(new KeyMapperOption(Settings.MenuButton, "Show Options", "This key will bring up the options screen"));
        _options.Add(new KeyMapperOption(Settings.SwitchTabs, "Switch Tabs", "This key will switch from available tabs"));
        _options.Add(new ChoiceOption<bool>(Settings.InventorySwap, OnOffLabels, OnOffValues, "Switch item to/from backpack",
            "Hold the Ctrl key and click on an item to swap it between your inventory and your backpack."));
        _options.Add(new KeyMapperOption(Settings.ReconVault, "Vault", "Connects you to Vault."));
        _options.Add(new KeyMapperOption(Settings.ReconGuild, "Guild Hall", "Connects you to Guild Hall."));
        _options.Add(new KeyMapperOption(Settings.ReconRealm, "Realm", "Connects you to Realm."));
        _options.Add(new KeyMapperOption(Settings.ReconGod, "God Lands", "Teleports you to God Lands."));
        _options.Add(new KeyMapperOption(Settings.ReconDarkMarket, "Black Market", "Connects you to the Black Market."));
        _options.Add(new KeyMapperOption(Settings.ReconMarket, "Marketplace", "Connects you to the Marketplace."));
        _options.Add(new KeyMapperOption(Settings.PartySummon, "Party Summon", "Hotkey to automatically summon your party."));
        _options.Add(new KeyMapperOption(Settings.PartyAccept, "Party Accept", "Hotkey to automatically accept a party summon."));
        _options.Add(new KeyMapperOption(Settings.ResetMScale, "Reset Map Scale", "Resets your map scale to default."));
        _options.Add(new KeyMapperOption(Settings.SetBagPriority, "Bag priority", "Toggle whether to make bags easier to interact with or not."));
        _options.Add(new KeyMapperOption(Settings.FullscreenKey, "Fullscreen", "Toggles fullscreen mode."));
        _options.Add(new KeyMapperOption(Settings.ItemScreenshotKey, "Item Screenshot", "Makes a screenshot of the currently displayed item tooltip and saves it as a .png"));
        _options.Add(new KeyMapperOption(Settings.ScreenshotModeKey, "Item Screenshot",
            "Removes the HUD and hides all popups which allows you to take a better screenshot. (Do a quick Shift+Scroll after enabling to fix the camera angle)"));
        _options.Add(new KeyMapperOption(Settings.FreeRoamKey, "Free Roam", "Your movement keys will control the position of the camera instead of your character."));
    }

    private void AddChatOptions() {
        _options.Add(new KeyMapperOption(Settings.Chat, "Activate Chat", "This key will bring up the chat input box"));
        _options.Add(new KeyMapperOption(Settings.ChatCommand, "Start Chat Command",
            "This key will bring up the chat with a \\'/\\' prepended to \" + \"allow for commands such as /who, /ignore, etc."));
        _options.Add(new KeyMapperOption(Settings.TellKey, "Start Chat Command", "This key will bring up a tell in the chat input box"));
        _options.Add(new ChoiceOption<int>(Settings.ChatInclude, ["None", "Guild", "Party", "GP"], [0, 1, 2, 3], "Include In Begin Tell",
            "This key will include the chat in the chat input box"));
        _options.Add(new KeyMapperOption(Settings.GuildChat, "Begin Guild Chat", "This key will bring up a guild chat in the chat input box"));
        _options.Add(new KeyMapperOption(Settings.PartyChat, "Begin Party Chat", "This key will bring up a party chat in the chat input box"));
        _options.Add(new ChoiceOption<bool>(Settings.ChatVisible, OnOffLabels, OnOffValues, "Chat visible", "Turn chat visibility ON/OFF.", OnChatVisible));
        _options.Add(new ChoiceOption<float>(Settings.ChatScaling, ["100%", "90%", "80%", "70%", "60%", "50%"], [1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f], "Chat Scale",
            "Click here to change the scale of the chat", OnChatBoxScale));
        _options.Add(new KeyMapperOption(Settings.ChatHistoryUp, "Navigate Chat History Up", "Navigate previous messages"));
        _options.Add(new KeyMapperOption(Settings.ChatHistoryDown, "Navigate Chat History Down", "Navigate next messages"));
        _options.Add(new ChoiceOption<int>(Settings.ChatHideList, ["All", "None", "Locked", "Guild", "Party", "GPL"], [0, 1, 2, 3, 4, 5], "Show Player Messages",
            "Choose which players messages should be shown to you. This includes whispers and global chat."));
    }

    private void AddGraphicsOptions() {
        _options.Add(new ChoiceOption<float>(Settings.CameraAngle, ["45", "0"], [7 * MathF.PI, 0f], "Default Camera Angle", "This toggles the default camera angle",
            OnDefautCameraAngleChange));
        _options.Add(new ChoiceOption<bool>(Settings.CenterPlayer, OnOffLabels, OnOffValues, "Center On Player", "This toggles whether the player is centered or offset"));
        _options.Add(new ChoiceOption<bool>(Settings.EyeCandyParticles, OnOffLabels, OnOffValues, "Eye Candy Particles",
            "This toggles whether to show eye candy particles, disabling this will improve performance."));
        _options.Add(new ChoiceOption<bool>(Settings.ReducedParticles, OnOffLabels, OnOffValues, "Reduced Particles",
            "This toggles whether to show reduced particles, enabling this will improve performance."));
        _options.Add(new ChoiceOption<bool>(Settings.Fullscreen, OnOffLabels, OnOffValues, "Fullscreen Mode", "This toggles whether to have fullscreen mode enabled or not.",
            OnFullscreenToggle));
        _options.Add(new ChoiceOption<int>(Settings.FpsCap, ["30", "60", "90", "120", "144", "165", "240", "300", "360", "None"], [30, 60, 90, 120, 144, 165, 240, 300, 360, -1],
            "FPS Cap", "This allows you to choose a frame rate cap", OnFPSChange));
        _options.Add(new ChoiceOption<int>(Settings.MaxRenderDistance, ["Low", "Medium", "High", "Max"], [15, 20, 25, 60], "Max Render Distance",
            "Pick the maximum render distance of your client. Can improve performance greatly.", OnRenderDistanceChange));
        _options.Add(new ChoiceOption<float>(Settings.MScale, ["100%", "90%", "80%", "70%", "60%", "50%", "200%", "150%"], [1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 2f, 1.5f], "Zoom",
            "Zooms your game in and out so you can see more or less things at once. You can also use the /mscale command or use Shift + Scroll. (Available options: 100%, 90%, 80%, 70%, 60%, 50%, 200%, 150%)",
            OnMScaleChange));
        _options.Add(new ChoiceOption<bool>(Settings.VSync, OnOffLabels, OnOffValues, "VSync", "This toggles whether to have VSync enabled or not.", OnVSyncToggle));
    }

    private void AddSoundOptions() {
        _options.Add(new ChoiceOption<bool>(Settings.PlayMusic, OnOffLabels, OnOffValues, "Play Music", "This toggles whether music is played", OnPlayMusicChange));
        _options.Add(new SliderOption(Settings.MusicVolume, "Vol:", OnMusicVolumeChange));
        _options.Add(new ChoiceOption<bool>(Settings.PlaySfx, OnOffLabels, OnOffValues, "Play Sound Effects", "This toggles whether sound effects are played",
            OnPlaySoundEffectsChange));
        _options.Add(new SliderOption(Settings.SfxVolume, "Vol:", OnSfxVolumeChange));
        _options.Add(new ChoiceOption<bool>(Settings.PlayWeaponSfx, OnOffLabels, OnOffValues, "Play Weapon Sounds", "This toggles whether weapon sounds are played",
            OnPlayWeaponSoundsChange));
        _options.Add(new SliderOption(Settings.WeaponSfxVolume, "Vol:", OnWeaponSfxVolumeChange));
        _options.Add(new ChoiceOption<bool>(Settings.PlayLootSfx, OnOffLabels, OnOffValues, "Play Loot Sounds", "This toggles whether LG/DC/AC/CS loot drop sounds are played"));
        _options.Add(new SliderOption(Settings.LootSfxVolume, "Vol:", OnLootSfxVolumeChange));
    }

    private void AddExtraOptions() {

    }

    private void OnLootSfxVolumeChange(float obj) {
        throw new NotImplementedException();
    }

    private void OnWeaponSfxVolumeChange(float obj) {
        throw new NotImplementedException();
    }

    private void OnSfxVolumeChange(float obj) {
        throw new NotImplementedException();
    }

    private void OnMusicVolumeChange(float obj) {
        throw new NotImplementedException();
    }

    private void OnPlayWeaponSoundsChange() {
        throw new NotImplementedException();
    }

    private void OnPlaySoundEffectsChange() {
        throw new NotImplementedException();
    }

    private void OnPlayMusicChange() {
        throw new NotImplementedException();
    }

    private void OnChatVisible() {
        throw new NotImplementedException();
        //GameSprite.Instance.ChatBox.Enabled = !GameSprite.Instance.ChatBox.Enabled;
    }

    private void OnChatBoxScale() {
        throw new NotImplementedException();
        //GameSprite.Instance.ChatBox.Scale = new Vector2(Settings.ChatScaling);
    }

    private void OnDefautCameraAngleChange() {
        throw new NotImplementedException();
    }

    private void OnVSyncToggle() {
        Main.GraphicsMode.Dispatch(GraphicsOptions.InGame);
        var option = GetOption(Settings.FpsCap);
        option.SetDisabled(Settings.VSync);
    }

    private void OnRenderDistanceChange() {
        throw new NotImplementedException();
    }

    private void OnFPSChange() {
        Main.GraphicsMode.Dispatch(GraphicsOptions.InGame);
    }

    private void OnMScaleChange() {
        throw new NotImplementedException();
    }

    private void OnFullscreenToggle() {
        throw new NotImplementedException();
    }
    
    private Option GetOption(ISettingType setting) {
        return _options.Find(option => option.Setting == setting);
    }
}
using System.Collections.Generic;
using AlloyClient.Networking;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Ui.Components.Graphics;
using AlloyClient.Ui.Components.Panels;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using Alloy.UiLib.Signals;

namespace AlloyClient.Game.Components.Options;

public sealed class OptionsView : Overlay {

    public const int PanelWidth = Settings.DefaultScreenWidth;
    public const int PanelHeight = Settings.DefaultScreenHeight;
    public const int HeaderHeight = 120;
    public const int OptionsHeight = 507;
    
    public const string ControlsTab = "Controls";
    public const string HotkeysTab = "Hot Keys";
    public const string ChatTab = "Chat";
    public const string GraphicsTab = "Graphics";
    public const string SoundTab = "Sound";
    public const string ExtraTab = "Extra";

    private readonly static string[] Tabs = [ControlsTab, HotkeysTab, ChatTab, GraphicsTab, SoundTab, ExtraTab];
    
    private readonly Dictionary<string, OptionTabView> _tabViews = [];

    public readonly static SingleSignal RefreshOptions = new ();//TODO: holds refs, redo

    private TextButton _selectedTab;

    public OptionsView() {
        RefreshOptions.Set(Refresh);

        SetAnchor(UiAnchor.Middle);

        var background = new ColorRect(new ColorRectConfig {
            Width = PanelWidth,
            Height = PanelHeight,
            Color = 0x2B2B2B,
            Alpha = 0
        });
        AddChild(background);

        var titleText = new SimpleText(new TextConfig {
            Text = "Options",
            FontSize = 43,
            FontType = FontType.Bold,
            X = PanelWidth / 2,
            Y = 10,
            OutlineThickness = 2,
            Anchor = UiAnchor.MiddleTop
        });
        AddChild(titleText);

        var header = new ColorRect(new ColorRectConfig {
            Y = HeaderHeight - 1,
            Width = PanelWidth,
            Height = 1,
            Color = 0x5E5E5E
        });
        AddChild(header);

        var menuRibbon = new TitleMenuRibbon(PanelWidth) {
            Y = TitleMenuRibbon.TopY
        };
        AddChild(menuRibbon);

        var continueButton = new MenuBarButton(new TextButtonConfig {
            Text = "continue",
            FontSize = 43,
            OnClicked = OnContinue,
            X = PanelWidth / 2,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.Middle
        });
        AddChild(continueButton);

        var resetButton = new MenuBarButton(new TextButtonConfig {
            Text = "reset to defaults",
            FontSize = 26,
            OnClicked = OnResetToDefaults,
            X = 20,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.MiddleLeft
        });
        AddChild(resetButton);

        var homeButton = new MenuBarButton(new TextButtonConfig {
            Text = "back to home",
            FontSize = 26,
            OnClicked = OnHome,
            X = PanelWidth - 20,
            Y = TitleMenuRibbon.MenuCenterY,
            Anchor = UiAnchor.MiddleRight
        });
        AddChild(homeButton);

        AddTabs();
        Refresh();
    }

    private void AddTabs() {
        var first = true;
        var xOffset = 22;
        for (var i = 0; i < Tabs.Length; i++) {
            var tabName = Tabs[i];
            var tab = new TextButton(new TextButtonConfig {
                Text = tabName,
                FontSize = 19,
                FontType = FontType.Bold,
                ActiveColor = 0xB3B3B3,
                HoverColor = 0xFFFFFF,
                InactiveColor = 0xFFC800,
                X = xOffset,
                Y = 84,
                Anchor = UiAnchor.LeftTop
            });
            tab.AddEventListener(MouseEvent.LeftClick, OnSelectTab);
            AddChild(tab);

            var view = new OptionTabView(tabName) {
                Visible = false,
                Y = HeaderHeight
            };
            _tabViews[tabName] = view;
            AddChild(view);

            if (first) {
                first = false;

                view.Visible = true;
                SelectTab(tab);
            }

            xOffset += 172;
        }
    }

    private void OnSelectTab(MouseEvent args) {
        SelectTab(args.CurrentTarget as TextButton);
    }

    private void SelectTab(TextButton tab) {
        if (tab is null || tab == _selectedTab) {
            return;
        }

        if (_selectedTab != null) {
            _selectedTab.Activate();
            _tabViews[_selectedTab.Name].Visible = false;
        }

        _selectedTab = tab;
        _selectedTab!.Deactivate();
        _tabViews[_selectedTab.Name].Visible = true;
    }
    
    public void Refresh() {
        foreach (var tabView in _tabViews.Values) {
            tabView.Refresh();
        }
    }

    private void OnResetToDefaults() {
        Settings.ResetToDefault();
        ApplyLiveSettings();
        Refresh();
    }

    private void OnContinue() {
        Settings.SaveSettings();
        CloseOverlay();
        UserInput.SetManualFocus(true);
    }

    private void OnHome() {
        OnContinue();
        Client.Disconnect();
        Map.Reset();
    }

    private static void ApplyLiveSettings() {
        Audio.SetMasterVolume(Settings.GetMasterVolume());
        Audio.MusicChannel.SetVolume(Settings.GetMusicVolume());
        Audio.SfxChannel.SetVolume(Settings.GetSfxVolume());
        GameScreen.RefreshChatOptions();
        Main.OnScreenChange.Dispatch(ScreenType.Game);

        if (Settings.FullscreenState) {
            Main.OnFullscreenToggle.Dispatch();
        }
    }
}

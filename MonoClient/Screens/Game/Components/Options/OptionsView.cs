using System.Collections.Generic;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Screens.Title;
using MonoClient.State;
using MonoClient.State.Input;
using MonoClient.Ui.Components.Buttons;
using MonoClient.Ui.Components.Panels;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Utils.Signals;

namespace MonoClient.Screens.Game.Components.Options;

public sealed class OptionsView : Overlay {

    public static OptionsView Panel
    {
        get
        {
            if (_panel == null)
                _panel = new OptionsView();
            return _panel;
        }
    }
    private static OptionsView _panel;
    public const string ControlsTab = "Controls";
    public const string HotkeysTab = "Hot Keys";
    public const string ChatTab = "Chat";
    public const string GraphicsTab = "Graphics";
    public const string SoundTab = "Sound";
    public const string ExtraTab = "Extra";

    private static readonly string[] Tabs = [ControlsTab, HotkeysTab, ChatTab, GraphicsTab, SoundTab, ExtraTab];
    
    private readonly Dictionary<string, OptionTabView> _tabViews = [];

    public static readonly SingleSignal RefreshOptions = new ();

    private TextButton _selectedTab;

    public OptionsView() {
        RefreshOptions.Set(Refresh);
        
        SetBaseDimensions(Settings.DefaultScreenWidth, Settings.DefaultScreenHeight);

        var titleText = new SimpleText(new TextConfig {
            Text = "Options",
            FontSize = 57,
            FontType = FontType.Bold,
            X = Settings.DefaultScreenWidth / 2,
            Y = 10,
            OutlineThickness = 2,
            Anchor = UiAnchor.MiddleTop
        });
        AddChild(titleText);

        var header = new ColorRect(new ColorRectConfig { X = 0, Y = 120, Width = Settings.DefaultScreenWidth, Height = 2, Color = 0x5E5E5E });
        AddChild(header);

        var continueButton = new MenuBarButton(new TextButtonConfig {
            Text = "continue",
            FontSize = 57,
            OnClicked = CloseOverlay,
            X = Settings.DefaultScreenWidth / 2,
            Y = Settings.DefaultScreenHeight - 40,
            Anchor = UiAnchor.Middle
        }, true);
        AddChild(continueButton);

        var resetButton = new MenuBarButton(new TextButtonConfig {
            Text = "reset to defaults",
            FontSize = 35,
            OnClicked = OnResetToDefaults,
            Y = Settings.DefaultScreenHeight - 40,
            Anchor = UiAnchor.MiddleLeft
        });
        resetButton.X = 20;
        AddChild(resetButton);

        var homeButton = new MenuBarButton(new TextButtonConfig {
            Text = "home",
            FontSize = 35,
            OnClicked = OnHome,
            X = Settings.DefaultScreenWidth - 100,
            Y = Settings.DefaultScreenHeight - 40,
            Anchor = UiAnchor.MiddleRight
        });
        homeButton.X = Settings.DefaultScreenWidth - homeButton.Width - 20;
        AddChild(homeButton);

        AddTabs();
    }

    private void AddTabs() {
        var first = true;
        var xOffset = 22;
        foreach (var tabName in Tabs) {
            var tab = new TextButton(new TextButtonConfig {
                Text = tabName,
                FontSize = 25,
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
                Y = 120
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
        Refresh();
    }

    private void OnHome() {
        CloseOverlay();
        Client.Disconnect();
        ScreenManager.FadeToScreen(new CharacterListScreen(), Easing.SineInOut, 500, 0, () => Client.Disconnect());
    }
}
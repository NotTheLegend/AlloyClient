using System.Linq;
using MonoClient.Data;
using MonoClient.Data.XmlModels;
using MonoClient.Display;
using MonoClient.Screens.Title.Components;
using MonoClient.Screens.Title.ServersListScreen.Components;
using MonoClient.State;
using MonoClient.Ui.Components.Buttons;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core.Events;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.ServerListScreen;

public class ServersTitleScreen : TitleScreenBase {

    #region Config

    /// <summary>
    /// Don't think it's of any use for now because CharacterList should already be loaded
    /// by the previous (title) screen but added it in case we need it for some reason
    /// </summary>
    private const bool RequestNewCharList = false;

    #endregion
    
    private readonly Container _selectedServerContainer;
    private readonly Container _serverListContainer;
    
    private ServerRect _selectedServerRect;

    public ServersTitleScreen() {
        #region Title

        var serverTitle = new SimpleText(new TextConfig() {
            Text = "Server Selection",
            FontSize = 32,
            FontType = FontType.Bold,
            X = Settings.DefaultScreenWidth / 2,
            Y = 50,
            Anchor = UiAnchor.Middle,
        });
        AddChild(serverTitle);
        
        var lineDivider = new ColorRect(new ColorRectConfig {
            Y = 100,
            Width = Settings.DefaultScreenWidth,
            Height = 5,
            Color = 0x404040,
        });
        AddChild(lineDivider);
        
        #endregion

        #region Selected Server
        
        var selectedServerText = new SimpleText(new TextConfig() {
            Anchor = UiAnchor.MiddleTop,
            Y = lineDivider.Y + lineDivider.Height + 10,
            X = Settings.DefaultScreenWidth / 2,
            Text = "Selected Server:",
            FontSize = 22,
        });
        AddChild(selectedServerText);

        _selectedServerContainer = new Container(new ContainerConfig() {
            Anchor = UiAnchor.MiddleTop,
            Width = Settings.DefaultScreenWidth,
            Height = 84,
            X = Settings.DefaultScreenWidth / 2,
            Y = selectedServerText.Y + selectedServerText.Height,
        });
        AddChild(_selectedServerContainer);
        
        #endregion

        #region Server List
        
        _serverListContainer = new Container(new ContainerConfig() {
            Anchor = UiAnchor.MiddleTop,
            Width = Settings.DefaultScreenWidth / 2 + 20,
            // Ugly way need to find a better solution for that
            Height = Settings.DefaultScreenHeight 
                     - 100 // Title
                     - lineDivider.Height 
                     - 22 // Selected Server Text
                     - _selectedServerContainer.Height 
                     - 100, // Bottom Menu Bar
            X = Settings.DefaultScreenWidth / 2,
            Y = _selectedServerContainer.Y + _selectedServerContainer.Height,
            EnableClip = true
        });
        AddChild(_serverListContainer);
        
        #endregion

        #region Menu Bar
        
        var backButton = new MenuBarButton("back", TitleScreen.FontSize, () => {
            ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0);
        });
        backButton.SetAnchor(UiAnchor.Middle);
        backButton.X = Settings.DefaultScreenWidth / 2;
        backButton.Y = Settings.DefaultScreenHeight - 50;
        AddChild(backButton);
        
        #endregion
        
        if (RequestNewCharList)
            AddEventListener(CharacterList.LoadAsync(), OnCharListReceived);
        else {
            BuildServerListUI(CharacterList.Model.Servers.ServerItems);
        }
    }

    private void BuildServerListUI(ServerListItem[] servers) {
        var selectedServer = servers.FirstOrDefault(s => s.Port == Settings.SelectedGameServerPort);
        SelectServer(selectedServer ?? servers[0]);

        int leftX = _serverListContainer.Width / 4;
        int rightX = _serverListContainer.Width * 3 / 4;
        int startY = 10;

        int i = 0;
        foreach (var server in servers) {
            var serverRect = new ServerRect(server);
            
            serverRect.X = i % 2 == 0 ? leftX : rightX;
            serverRect.Y = startY + (serverRect.Height + 10) * (i / 2);
            _serverListContainer.AddChild(serverRect);

            serverRect.Clicked += () => {
                SelectServer(server);
            };

            i++;
        }
    }

    private void OnCharListReceived() {
        BuildServerListUI(CharacterList.Model.Servers.ServerItems);
    }

    private void SelectServer(ServerListItem server) {
        // All GameServers would be hosted on the same IP because there's no
        // address property on ServerListItem(?) so only Port matters
        Settings.SelectedGameServerPort.SetValue((ushort)server.Port);

        if (_selectedServerRect is not null) {
            _selectedServerRect.SetServer(server);
            return;
        }
        
        var selectedRect = new ServerRect(server);
        selectedRect.SetAnchor(UiAnchor.Middle);
        selectedRect.X = _selectedServerContainer.Width / 2;
        selectedRect.Y = _selectedServerContainer.Height / 2;
        _selectedServerContainer.AddChild(selectedRect);
        
        _selectedServerRect = selectedRect;
    }
}
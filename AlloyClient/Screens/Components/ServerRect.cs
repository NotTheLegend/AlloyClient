using System;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Data;

namespace AlloyClient.Screens.Components;

public class ServerRect : Container {

    #region Config

    private static readonly ContainerConfig Config = new() {
        Anchor = UiAnchor.MiddleTop,
        Width = Settings.DefaultScreenWidth / 4,
        Height = 64
    };

    private const float CrowdedServerThreshold = 0.75f;
    private const int FontSize = 22;

    #endregion

    #region Colors

    private const uint BackgroundColor = 0x6b6b6b;
    private const uint BackgroundHoverColor = 0x878787;
    private const uint AvailableServerColor = 0x12964b;
    private const uint CrowdedServerColor = 0xe4bd10;
    private const uint FullServerColor = 0xb41221;

    #endregion

    public event Action Clicked;

    // Interactive UI elements
    private readonly ColorRect _background;
    private readonly SimpleText _serverNameText;
    private readonly SimpleText _playersText;

    public ServerRect(ServerItem model) : base(Config) {

        _background = new ColorRect(new ColorRectConfig {
            Width = Width,
            Height = Height,
            Color = BackgroundColor,
            Alpha = 1f
        });

        AddChild(_background);

        _serverNameText = new SimpleText(new TextConfig {
            Text = "Name",
            FontSize = FontSize,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.MiddleLeft
        });

        _serverNameText.X = 10;
        _serverNameText.Y = Height / 2;
        AddChild(_serverNameText);

        _playersText = new SimpleText(new TextConfig {
            Text = "0 / 0",
            FontSize = FontSize,
            FontType = FontType.Bold,
            OutlineThickness = 3,
            Color = AvailableServerColor,
            Anchor = UiAnchor.MiddleRight
        });

        _playersText.X = Width - 10;
        _playersText.Y = Height / 2;
        AddChild(_playersText);

        MouseEnabled = true;

        AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        AddEventListener(MouseEvent.MouseOut, OnMouseOut);
        AddEventListener(MouseEvent.LeftClick, () => Clicked?.Invoke());

        SetServer(model);
    }

    private void OnMouseOver() {
        _background.SetColor(BackgroundHoverColor);
    }

    private void OnMouseOut() {
        _background.SetColor(BackgroundColor);
    }

    /// <summary>
    /// Sets the server information and updates the UI elements.
    /// </summary>
    public void SetServer(ServerItem server) {
        _serverNameText.SetText(server.Name);
        _playersText.SetText($"{server.Players} / {server.MaxPlayers}");

        ServerState state = GetServerState(server.Players, server.MaxPlayers);
        _playersText.SetColor(ServerStateToColor(state));
    }

    private ServerState GetServerState(int players, int max) {
        if (players >= max) return ServerState.Full;
        if ((float)players / max >= CrowdedServerThreshold) return ServerState.Crowded;

        return ServerState.Available;
    }

    private uint ServerStateToColor(ServerState state) {
        return state switch {
            ServerState.Crowded => CrowdedServerColor,
            ServerState.Full => FullServerColor,
            _ => AvailableServerColor
        };
    }

    private enum ServerState {
        Available,
        Crowded,
        Full
    }
}
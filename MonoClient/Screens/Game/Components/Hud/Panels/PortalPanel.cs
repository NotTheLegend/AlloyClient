using Microsoft.Xna.Framework;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Objects;
using MonoClient.State.Input;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Utils.Signals;

namespace MonoClient.Screens.Game.Components.Hud.Panels;

public class PortalPanel : Panel {

    private readonly Entity _portal;

    private readonly bool _locked;
    
    private readonly SimpleText _fullText;

    private readonly TextButton _enterButton;

    public PortalPanel(Entity entity) {
        _portal = entity;
        _locked = entity.Properties.LockedPortal;
        InputHandler.OnInteract.Set(EnterPortal);

        var txt = _portal.Properties.DisplayName;

        if (_locked && txt.IndexOf("Locked") == 0) {
            txt = txt[7..];
        }

        var name = new SimpleText(new TextConfig {
            X = Width / 2,
            Y = 16,
            Text = txt,
            FontSize = 22,
            FontType = FontType.Bold,
            OutlineColor = 0xFFFFFF,
            Anchor = UiAnchor.MiddleTop
        });
        AddChild(name);
        
        _fullText = new SimpleText(new TextConfig {
            X = Width / 2,
            Y = name.Height + 50,
            Text = _locked ? "Locked" : "Full",
            FontSize = 20,
            FontType = FontType.Bold,
            OutlineColor = 0xFF0000,
            Color = 0xFF0000,
            Anchor = UiAnchor.MiddleTop
        });
        _fullText.Y = name.Height + 10;

        _enterButton = new TextButton(new TextButtonConfig {
            Text = "Enter",
            FontSize = 20,
            OnClicked = EnterPortal,
            FontType = FontType.Bold,
            X = Width / 2,
            Y = name.Height + 50,
            Anchor = UiAnchor.MiddleTop
        });
        AddChild(_enterButton);
    }

    private void EnterPortal() {
        var pkt = UsePortal.CreatePacket();
        pkt.ObjectId = _portal.ObjectId;
        Client.QueuePacket(pkt);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if ((!_portal.PortalUsable || _locked) && ContainsChild(_enterButton)) {
            RemoveChild(_enterButton);
            AddChild(_fullText);
        }

        if ((_portal.PortalUsable && !_locked) && ContainsChild(_fullText)) {
            RemoveChild(_fullText);
            AddChild(_enterButton);
        }
    }
}
using System;
using AlloyClient.Game.Objects;
using AlloyClient.Networking;
using AlloyClient.Networking.Packets.Outgoing;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.BuiltIn.Buttons;
using Alloy.UiLib.Core;
using Alloy.UiLib.Enums;
using Alloy.Common;

namespace AlloyClient.Game.Components.Hud.Panels;

public class PortalPanel : Panel {

    private readonly Entity _portal;

    private readonly bool _locked;
    
    private readonly SimpleText _fullText;

    private readonly TextButton _enterButton;

    public PortalPanel(Entity entity) {
        _portal = entity;
        _locked = entity.Properties.LockedPortal;

        var txt = _portal.Properties.DisplayName;

        if (_locked && txt.StartsWith("Locked", StringComparison.Ordinal)) {
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
            OnClicked = OnInteractKey,
            FontType = FontType.Bold,
            X = Width / 2,
            Y = name.Height + 50,
            Anchor = UiAnchor.MiddleTop
        });
        AddChild(_enterButton);
        
        AddEventListener(Event.AddedToStage, () => { AddEventListener(Event.EnterFrame, OnFrameEnter);});
        AddEventListener(Event.RemovedFromStage, () => { RemoveEventListener(Event.EnterFrame, OnFrameEnter);});
    }

    protected override void OnInteractKey() {
        var pkt = UsePortal.CreatePacket();
        pkt.ObjectId = _portal.ObjectId;
        Client.QueuePacket(pkt);
    }

    private void OnFrameEnter() {
        if ((!_portal.PortalUsable || _locked) && Contains(_enterButton)) {
            RemoveChild(_enterButton);
            AddChild(_fullText);
        }

        if ((_portal.PortalUsable && !_locked) && Contains(_fullText)) {
            RemoveChild(_fullText);
            AddChild(_enterButton);
        }
    }
}
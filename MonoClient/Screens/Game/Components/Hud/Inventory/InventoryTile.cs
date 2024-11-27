using System;
using System.Linq;
using Common.Vector;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoClient.Assets.Libraries;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects;
using MonoClient.Objects.Util;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.State;
using MonoClient.Ui.Components.Tooltips;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public class InventoryTile : Sprite {
    private ObjectRect _itemSprite;
    private ushort _itemType;
    private ItemDesc _itemDesc;
    private TierText _tierTag;
    private EquipmentToolTip _tooltip;
    
    private readonly bool _draggable;

    private IntVector2 _dragStart;
    private bool _checkForDrag;
    private bool _dragging;
    
    public byte Slot;
    public Entity Owner;

    public InventoryTile(ItemDesc item, bool draggable = true) {
        _itemType = item?.ObjectType ?? 0;
        _itemDesc = item;

        _draggable = draggable;
        
        var background = new ColorRect(new ColorRectConfig {
            Alpha = 1f,
            Color = 0x0d0d0d,
            Width = 54,
            Height = 54,
            X = -2,
            Y = -2
        });
        AddChild(background);
        
        var inner = new ColorRect(new ColorRectConfig {
            Alpha = 1f,
            Color = 0x4C4C4C,
            Width = 50,
            Height = 50
        });
        AddChild(inner);

        _itemSprite = new ObjectRect(new ObjectRectConfig {
            Texture = AssetUtils.GetTextureInfo(_itemType <= 0 ? (ushort)0x0096 : _itemType),
            Width = 50,
            Height = 50
        });
        AddChild(_itemSprite);

        if (_itemDesc != null)
        {
            _tierTag = new TierText(_itemDesc);
            _tierTag.X = _itemSprite.Width;
            _tierTag.Y = _itemSprite.Height;
            _tierTag.SetAnchor(UiAnchor.RightBottom);
            _itemSprite.AddChild(_tierTag);
        }

        CreateListeners();
    }
    
    private void CreateListeners() {
        _itemSprite.MouseEnabled = true;
        
        if (_draggable) {
            _itemSprite.AddEventListener(MouseEventId.LeftDown, OnMouseDown);
        }
		
        _itemSprite.AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        _itemSprite.AddEventListener(MouseEventId.MouseOut, OnMouseOut);
    }

    private void OnMouseOver() {
        if (_itemDesc == null || _dragging) return;
        _tooltip = new EquipmentToolTip(_itemDesc);
        TooltipManager.AddTooltip(_tooltip);
    }

    private void OnMouseOut() {
        if (_itemDesc == null || _tooltip == null || _dragging) return;
        TooltipManager.RemoveTooltip(_tooltip);
        _tooltip = null;
    }

    private void OnMouseDown(MouseEventArgs args) {
        _dragStart = args.Coords;
        _checkForDrag = true;
        _itemSprite.AddEventListener(MouseEventId.LeftUp, CancelDragCheck);
    }

    private void CancelDragCheck() {
        _checkForDrag = false;
        _itemSprite.RemoveEventListener(MouseEventId.LeftUp, CancelDragCheck);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (!_checkForDrag) return;
        var delta = MouseInput.GetMousePosition() - _dragStart;
        var dist = MathF.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

        if (dist > 3) {
            CancelDragCheck();
            OnBeginDrag();
        }
    }

    private void OnBeginDrag() {
        _dragging = true;
        _tierTag.Visible = false;
        
        TooltipManager.RemoveTooltip(_tooltip);
       
        _itemSprite.StartDrag<InventoryTile>();
        _itemSprite.AddEventListener(MouseEventId.LeftUp, OnEndDrag);
        RemoveChild(_itemSprite);
        Map.GameSprite.AddChild(_itemSprite);
    }

    private void OnEndDrag(MouseEventArgs args) {
        _dragging = false;
        _itemSprite.RemoveEventListener(MouseEventId.LeftUp, OnEndDrag);
        _itemSprite.EndDrag();
        Map.GameSprite.RemoveChild(_itemSprite);
        AddChild(_itemSprite);
        _tierTag.Visible = true;
        
        HandleDropTarget();
    }

    private void HandleDropTarget() {
        if (_itemSprite.DropTarget == null) return;
        if (_itemSprite.DropTarget is not InventoryTile tile) return;// shouldnt happen cuz its filtered but you never know
        
        // todo add inv drop and the rest of the inventory actions
        // the easiest way would be add an invisible inventory tile that covers the gamescreen area for a valid drop target or something
        
        var swap = InvSwap.CreatePacket();
        swap.Time = (int)Map.LastGameTime.TotalGameTime.TotalSeconds;
        swap.Position = new Position { X = Map.LocalPlayer.Position.X, Y = Map.LocalPlayer.Position.Y };
            
        swap.SlotObj1 = new ObjectSlot {
            ObjectId = Owner.ObjectId,
            SlotId = Slot,
            ObjectType = _itemType
        };
        swap.SlotObj2 = new ObjectSlot {    
            ObjectId = tile.Owner.ObjectId,
            SlotId = tile.Slot,
            ObjectType = tile._itemType
        };
        Client.QueuePacket(swap);
            
        (tile._itemType, _itemType) = (_itemType, tile._itemType);
        UpdateTile();
        tile.UpdateTile();
    }

    private void UpdateTile() {
        _itemSprite.ChangeTexture(AssetUtils.GetTextureInfo(_itemType <= 0 ? (ushort)0x0096 : _itemType));
    }
}
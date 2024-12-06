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
using MonoClient.UiLib.Utils;
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

    private Timer _doubleTimer = new Timer(250, 1);
    private bool _pendingDouble;
    
    public byte Slot;
    public Entity Owner;

    public InventoryTile(ItemDesc item, bool draggable = true) {
        _itemType = item?.ObjectType ?? 0;
        _itemDesc = item;

        _draggable = draggable;
        _doubleTimer.AddEventListener(TimerEvent.TimerComplete, OnSingleClick);
        
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
            _itemSprite.AddEventListener(MouseEventId.LeftUp, OnMouseUp);
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
        _pendingDouble = false;
    }

    private void OnMouseDown(MouseEventArgs args) {
        _dragStart = args.Coords;
        _checkForDrag = true;
        _itemSprite.AddEventListener(MouseEventId.LeftUp, CancelDragCheck);
    }

    private void OnMouseUp(MouseEventArgs args) {
        if (_dragging) return;

        if (args.ShiftKey) {
            _pendingDouble = false;
            // todo: use item
            return;
        }

        if (args.CtrlKey) {
            _pendingDouble = false;
            // todo: swap to backpack
            return;
        }

        if (_pendingDouble) {
            _pendingDouble = false;
            _doubleTimer.Stop();
            // todo: double Click
            // equip or use
            return;
        }

        _pendingDouble = true;
        _doubleTimer.Reset();
        _doubleTimer.Start();
    }

    private void OnSingleClick() {
        _doubleTimer.Stop();
        _pendingDouble = false;
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
            _pendingDouble = false;
            CancelDragCheck();
            OnBeginDrag();
        }
    }

    private void OnBeginDrag() {
        _dragging = true;
        _tierTag.Visible = false;
        
        TooltipManager.RemoveTooltip(_tooltip);
       
        _itemSprite.StartDrag();
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
        var list = new[] {typeof(InventoryTile), typeof(InventoryGrid), typeof(GameScreen)};
        
        var target = _itemSprite.DropTarget.GetTypeFromList(list);

        switch (target) {
            case InventoryTile tile:
                if (!tile._draggable) break;
                
                var swap = InvSwap.CreatePacket();
                swap.Time = (int)Map.LastGameTime.TotalGameTime.TotalMilliseconds;
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
                break; // swap
            case InventoryGrid grid:
                break; // add to first free slot
            case GameScreen:
                var drop = InvDrop.CreatePacket();
                drop.SlotObject = new ObjectSlot {
                    ObjectId = Owner.ObjectId,
                    SlotId = Slot,
                    ObjectType = _itemType
                };
                
                Client.QueuePacket(drop);
                break; // drop
            default:
                //reset tile
                break;
            
        }
    }

    private void UpdateTile() {
        _itemSprite.ChangeTexture(AssetUtils.GetTextureInfo(_itemType <= 0 ? (ushort)0x0096 : _itemType));
    }
}
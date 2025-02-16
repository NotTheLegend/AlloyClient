using System;
using System.Drawing;
using Common.Vector;
using Microsoft.Xna.Framework;
using MonoClient.Display;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.State;
using MonoClient.Ui.Components.Tooltips;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;
using MonoClient.UiLib.Input;
using MonoClient.UiLib.Utils;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public sealed class ItemTile : Sprite {

    public int Size = 50; 
    
    public readonly byte SlotId;

    public readonly byte SlotType;
    
    public readonly bool Interactive;

    public readonly bool OneWay;

    public readonly Entity Owner;
    
    public ItemDesc Item;

    private readonly ObjectRect _sprite;
    private readonly SimpleText _tierText;
    
    private EquipmentToolTip _tooltip;

    private IntVector2 _dragStart;
    private bool _checkForDrag;
    private bool _dragging;
    private uint _bgColor;

    private readonly Timer _doubleTimer = new Timer(250, 1);
    private bool _pendingDouble;

    private readonly CutEdgeRect _background;
    private readonly ObjectRect _slotDetail;
    private readonly SimpleText _slotId;

    public ItemTile(Entity owner, byte slotId, bool interactive, CutEdges cut, bool oneWay, byte slotType = 0, int tileSize = 50, uint bgcolor = 0x545454) {
        Size = tileSize;
        Owner = owner;
        SlotId = slotId;
        SlotType = slotType;
        Interactive = interactive;
        OneWay = oneWay;
        _bgColor = bgcolor;

        _doubleTimer.AddEventListener(TimerEvent.TimerComplete, OnSingleClick);
        
        _background = new CutEdgeRect(new CutEdgeConfig { Width = Size, Height = Size, CutX = 4, CutY = 4, Cuts = cut, Color = _bgColor });
        AddChild(_background);
        
        _slotDetail = new ObjectRect(new ObjectRectConfig { Texture = AssetUtils.GetTextureInfo(0x0096), Width = Size, Height = Size });
        _slotDetail.Visible = false;
        _slotDetail.ColorTransformation = new ColorTransform(0, 0, 0, 1, 54, 54, 54, 0);
        _slotDetail.SetColorSecondary(0, 0);
        AddChild(_slotDetail);
        
        if (SlotType != 0) {
            _slotDetail.ChangeTexture(ItemConstants.GetSlot(SlotType));
            _slotDetail.Visible = true;
        }

        _slotId = new SimpleText(new TextConfig { Text = "", X = Size / 2, Y = Size / 2, FontSize = 32, Bold = 1, Color = 0x363636, OutlineColor = 0x363636, Anchor = UiAnchor.Middle });
        _slotId.Visible = false;
        AddChild(_slotId);

        if (Owner is Player && SlotType == 0) {
            _slotId.Visible = true;
        }
        
        _sprite = new ObjectRect(new ObjectRectConfig { Texture = AssetUtils.GetTextureInfo(0x0096), Width = Size, Height = Size });
        AddChild(_sprite);
        
        _tierText = new SimpleText(new TextConfig { FontSize = 16, Bold = 1, Text = "", OutlineThickness = 6 });
        _tierText.Visible = false;
        _tierText.SetAnchor(UiAnchor.RightBottom);
        _tierText.X = Size - 2;
        _tierText.Y = Size;
        AddChild(_tierText);

        if (Owner != null) {
            SetItem(Owner.Equipment[SlotId]);
        }
        
        _sprite.MouseEnabled = true;
        
        if (Interactive) {
            _sprite.AddEventListener(MouseEventId.LeftDown, OnMouseDown);
            _sprite.AddEventListener(MouseEventId.LeftUp, OnMouseUp);
        }
        
        _sprite.AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        _sprite.AddEventListener(MouseEventId.MouseOut, OnMouseOut);
    }

    public void SetItem(ItemDesc item)
    {
        Item = item;
        if (Item != null && Item.ObjectType > 0)
        {
            _sprite.ChangeTexture(AssetUtils.GetTextureInfo(Item.ObjectType));
            _background.SetColor(IsUsableByPlayer(Item) ? _bgColor : 0x5C1D1Du);
            _slotDetail.Visible = false;
            _slotId.Visible = false;
        }
        else
        {
            _sprite.ChangeTexture(AssetUtils.GetTextureInfo(0x0096));
            _background.SetColor(_bgColor);
            if (SlotType != 0) _slotDetail.Visible = true;
            if (Owner is Player && SlotType == 0) _slotId.Visible = true;
            if (_tooltip != null) TooltipManager.RemoveTooltip(_tooltip);
        }

        UpdateTierTag();
    }

    public void SetTileNumber(int slot) {
        _slotId.SetText($"{slot}");
    }

    public void SetDim(bool isDim) {
        _sprite.ColorTransformation = isDim ? ColorTransform.Dark : ColorTransform.Default;
    }

    private void UpdateTierTag() {
        if (Item == null || Item.Consumable || Item.SlotType == 10) {
            _tierText.Visible = false;
            return;
        }
        
        var color = 0xFFFFFFu;
        var tag = $"T{Item.Tier}";

        if (Item.Tier == -1) {
            color = 0x8A2BE2;
            tag = "UT";
        }

        //todo: item set
        /*if (Item.Set) {
            color = 0xFF9900;
            tag = "ST";
        }*/


        _tierText.SetText(tag);
        _tierText.SetColor(color);
        _tierText.Visible = true;
    }
    
    private void OnMouseOver() {
        if (Item == null || _dragging) return;
        _tooltip = new EquipmentToolTip(Item);
        TooltipManager.AddTooltip(_tooltip);
    }

    private void OnMouseOut() {
        if (Item == null || _tooltip == null || _dragging) return;
        TooltipManager.RemoveTooltip(_tooltip);
        _tooltip = null;
        _pendingDouble = false;
    }
    
    private void OnMouseDown(MouseEventArgs args) {
        if (Item == null) return;
        
        _dragStart = args.Coords;
        _checkForDrag = true;
        _sprite.AddEventListener(MouseEventId.LeftUp, CancelDragCheck);
    }

    private void OnMouseUp(MouseEventArgs args) {
        if (_dragging) return;

        if (args.ShiftKey) {
            _pendingDouble = false;
            
            // added basic consume logic, this will be looked at another time i assume
            if(Item.ObjectType == ItemConstants.PotionType || Item.Consumable)
            {
                int timeStuff = (int)Map.LastGameTime.TotalGameTime.TotalMilliseconds;
                Console.WriteLine($" Stats: Time: {timeStuff} ObjectId: {Owner.ObjectId} SlotId: {SlotId} ObjectType: {Item.ObjectType} PosX: {Owner.Position.X} PosY: {Owner.Position.Y} Byte: {(byte)UseType.START_USE}");
                useItem(timeStuff, Owner.ObjectId, SlotId, Item.ObjectType, Owner.Position.X, Owner.Position.Y, (byte)UseType.START_USE);
            }

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
        _sprite.RemoveEventListener(MouseEventId.LeftUp, CancelDragCheck);
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

        if (SlotType != 0)
            _slotDetail.Visible = true;
        
        if (Owner is Player && SlotType == 0)
            _slotId.Visible = true;
        
        TooltipManager.RemoveTooltip(_tooltip);
        
        RemoveChild(_sprite);
        RemoveChild(_tierText);
        
        _sprite.StartDrag(true);
        _sprite.AddEventListener(MouseEventId.LeftUp, OnEndDrag);
        Map.GameSprite.AddChild(_sprite);
    }

    private void OnEndDrag(MouseEventArgs args) {
        _dragging = false;
        _sprite.RemoveEventListener(MouseEventId.LeftUp, OnEndDrag);
        _sprite.EndDrag();
        Map.GameSprite.RemoveChild(_sprite);
        AddChild(_sprite);
        AddChild(_tierText);
        
        _slotDetail.Visible = false;
        _slotId.Visible = false;
        
        HandleDropTarget();
    }

    private void HandleDropTarget() {
        var list = new[] {typeof(ItemTile), typeof(InventoryGrid), typeof(HudView), typeof(GameScreen)};
        
        var target = _sprite.DropTarget.GetTypeFromList(list);

        switch (target) {
            case ItemTile tile:
                Console.WriteLine($"{!tile.Interactive} {tile.OneWay} {!CanSwapItems(this, tile)}");
                if (!tile.Interactive) break;
                if (tile.OneWay) break;
                if (!CanSwapItems(this, tile)) break;

                var swap = InvSwap.CreatePacket();
                swap.Time = (int)Map.LastGameTime.TotalGameTime.TotalMilliseconds;
                swap.Position = new Position { X = Map.LocalPlayer.Position.X, Y = Map.LocalPlayer.Position.Y };
            
                swap.SlotObj1 = new ObjectSlot {
                    ObjectId = Owner.ObjectId,
                    SlotId = SlotId,
                    ObjectType = Item.ObjectType
                };
                swap.SlotObj2 = new ObjectSlot {    
                    ObjectId = tile.Owner.ObjectId,
                    SlotId = tile.SlotId,
                    ObjectType = tile.Item?.ObjectType ?? 0
                };
                Client.QueuePacket(swap);
            
                (tile.Item, Item) = (Item, tile.Item);
                
                SetItem(Item);
                tile.SetItem(tile.Item);
                break; // swap
            case InventoryGrid grid:
                break; // add to first free slot
            case GameScreen:
                var drop = InvDrop.CreatePacket();
                drop.SlotObject = new ObjectSlot {
                    ObjectId = Owner.ObjectId,
                    SlotId = SlotId,
                    ObjectType = Item.ObjectType
                };
                
                Client.QueuePacket(drop);
                
                SetItem(null);
                break; // drop
            default:
                //reset tile
                SetItem(Item);
                break;
            
        }
    }

    private static bool CanSwapItems(ItemTile source, ItemTile target) {
        return source.CanHoldItem(target.Item) && target.CanHoldItem(source.Item);
    }

    private bool CanHoldItem(ItemDesc item) {
        return (item?.ObjectType ?? 0) == 0 || SlotType == 0 || SlotType == item.SlotType;
    }

    private static bool IsUsableByPlayer(ItemDesc item) {
        if (Map.LocalPlayer == null || item == null) return true;
        if (item.ObjectType == 0) return false;

        var slotType = item.SlotType;

        if (slotType == ItemConstants.PotionType)
            return true;

        var slots = Map.LocalPlayer.Properties.SlotTypes;
        for (var i = 0; i < slots.Count; i++) {
            if (slots[i] == slotType)
                return true;
        }

        return false;
    }

    private static void useItem(int time, int objectId, byte slotId, ushort objectType, float itemUsePosX, float itemUsePosY, byte useType) //Simple for now
    {
        var packet = UseItem.CreatePacket();

        packet.Time = time;
        packet.SlotObject.ObjectId = objectId;
        packet.SlotObject.SlotId = slotId;
        packet.SlotObject.ObjectType = objectType;
        packet.ItemUsePos.X = itemUsePosX;
        packet.ItemUsePos.Y = itemUsePosY;
        packet.UseType = useType;

        Client.QueuePacket(packet);
    }
}
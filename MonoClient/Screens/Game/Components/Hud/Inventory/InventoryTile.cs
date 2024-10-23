using System.Linq;
using Microsoft.Xna.Framework;
using MonoClient.Assets.Libraries;
using MonoClient.Networking;
using MonoClient.Networking.Packets.Outgoing;
using MonoClient.Networking.Structs.DataObjects;
using MonoClient.Objects;
using MonoClient.Objects.Util;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Input;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public class InventoryTile : Sprite {
    public ColorRect ItemBox;
    public ObjectRect ItemSprite;
    public SimpleText TierText;
    public ushort ItemType;
    public ItemDesc ItemDesc;
    
    public bool Dragging;
    public byte Slot;
    public Entity Owner;

    private Vector2 _lastMousePos;

    public InventoryTile(ItemDesc item) {
        ItemType = item?.ObjectType ?? 0;
        ItemDesc = item;
        
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

        ItemSprite = new ObjectRect(new ObjectRectConfig {
            Texture = AssetUtils.GetTextureInfo(ItemType <= 0 ? (ushort)0x0096 : ItemType),
            Width = 50,
            Height = 50
        });
        AddChild(ItemSprite);
        
        ItemBox = new ColorRect(new ColorRectConfig {
            Alpha = 0f,
            Color = 0xFF0000,
            Width = 50,
            Height = 50
        });
        AddChild(ItemBox);
        
        TierText = new SimpleText(new TextConfig {
            X = 27,
            Y = 34,
            Text = "UT",
            Bold = true,
            FontSize = 18,
            Color = 0xFFFFFF,
            OutlineThickness = 4,
            OutlineColor = 0
        });
        CreateTierText();
        CreateListeners();
    }

    private void CreateTierText() {
        if (TierText != null)
            RemoveChild(TierText);
        
        if (ItemDesc == null)
            return;
        
        if (ItemDesc.Legendary) {
            TierText.SetText("LG");
            TierText.Color = Color.Gold;
        }
        else if (ItemDesc.Demonic) {
            TierText.SetText("DC");
            TierText.Color = Color.Crimson;
        }
        else if (ItemDesc.Tier >= 0) {
            TierText.SetText($"T{ItemDesc.Tier}");
        }
        else {
            TierText.SetText("UT");
            TierText.Color = Color.Purple;
        }

        AddChild(TierText);
    }
    
    private void CreateListeners() {
        ItemSprite.MouseEnabled = true;
        ItemSprite.AddEventListener(MouseEventId.LeftDown, onDragItem);
        ItemSprite.AddEventListener(MouseEventId.LeftClick, onReleaseItem);
    }
    
    private void onDragItem() {
        TierText.Visible = false;
        Dragging = true;

        _lastMousePos = new Vector2(-1);
        ItemSprite.Scale = new Vector2(1.2f, 1.2f);
    }

    private void onReleaseItem() {
        Dragging = false;
        SwapItems();
    }

    private void SwapItems() {
        var inBounds = false;
        var pos = MouseInput.GetMousePosition();
        
        if (ItemBox.IsInBounds(pos)) {
            TierText.Visible = true;
            ItemBox.Visible = false;
            
            ItemSprite.Scale = Vector2.One;
            ItemSprite.X = 0;
            ItemSprite.Y = 0;

            return;
        }
        
        var tileList = InventoryGrid.InventoryTiles
            .Concat(ContainerGrid.InventoryTiles)
            .ToList();
            
        foreach (var tile in tileList) {
            if (!tile.ItemBox.IsInBounds(pos)) 
                continue;

            inBounds = true;
            
            var swap = InvSwap.CreatePacket();
            swap.SlotObj1 = new ObjectSlot {
                ObjectId = Owner.ObjectId,
                SlotId = Slot
            };
            swap.SlotObj2 = new ObjectSlot {    
                ObjectId = tile.Owner.ObjectId,
                SlotId = tile.Slot
            };
            Client.QueuePacket(swap);
            
            var newType = tile.ItemType;
                
            tile.ItemType = ItemType;
            tile.RebuildItemSprite();

            ItemType = newType;
            RebuildItemSprite();
            
            break;
        }

        if (!inBounds) {
            var drop = InvDrop.CreatePacket();
            drop.SlotObject = new ObjectSlot() {
                ObjectId = Map.LocalPlayerId,
                SlotId = Slot
            };
            Client.QueuePacket(drop);

            ItemType = 0;
            RebuildItemSprite();
        }
        
        TierText.Visible = true;
        ItemBox.Visible = false;
            
        ItemSprite.Scale = Vector2.One;
        ItemSprite.X = 0;
        ItemSprite.Y = 0;
    }

    public void RebuildItemSprite() {
        RemoveChild(ItemSprite);
        
        ItemSprite = new ObjectRect(new ObjectRectConfig {
            Texture = AssetUtils.GetTextureInfo(ItemType <= 0 ? (ushort)0x0096 : ItemType),
            Width = 50,
            Height = 50
        });
        
        AddChild(ItemSprite);
        
        CreateListeners();
        CreateTierText();
    }

    public void Update() {
        if (!Dragging)
            return;
        
        var mousePos = MouseInput.GetMousePosition();
        var mouseX = mousePos.X;
        var mouseY = mousePos.Y;

        if (_lastMousePos.X == -1) {
            _lastMousePos = new Vector2(mouseX, mouseY);
        }

        var diffX = mouseX - _lastMousePos.X;
        var diffY = mouseY - _lastMousePos.Y;

        ItemSprite.X += (int)diffX * (int)Settings.XScaleDown;
        ItemSprite.Y += (int)diffY * (int)Settings.YScaleDown;

        _lastMousePos = new Vector2(mouseX, mouseY);
    }
}
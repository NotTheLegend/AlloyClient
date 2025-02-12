using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using MonoClient.Objects;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud.Inventory
{
    public sealed class TabStrip : Sprite
    {
        private enum TabTypes
        {
            None,
            Inventory,
            StatsView,
            Backpack,
            PetInfo
        }

        private static readonly CutEdges[] Cuts = [CutEdges.TopLeft, CutEdges.None, CutEdges.None, CutEdges.TopRight, CutEdges.BottomLeft, CutEdges.None, CutEdges.None, CutEdges.BottomRight];

        private readonly int _offset;
        private readonly bool _backpack;

        private readonly int _tabHeight;
        private readonly int _tabWidth;

        private readonly uint TabColor = 2368034;
        private readonly uint BackgroundColor = 7039594;

        private Dictionary<int, TabTypes> Tabs = new Dictionary<int, TabTypes>()
        {
            { (int)TabTypes.Inventory, TabTypes.Inventory }, //Key is 1
            { (int)TabTypes.StatsView, TabTypes.StatsView }  //Key is 2
        };

        public int currentTabIndex = 1;

        private readonly IconButton InventoryTabButton;
        private readonly IconButton StatsViewTabButton;
        private readonly IconButton BackpackTabButton;

        private readonly CutEdgeRect InventoryTab;
        private readonly CutEdgeRect StatsViewTab;
        private readonly CutEdgeRect BackpackTab;

        private InventoryGrid _inventoryGrid;
        private StatsPanel _statsPanel;
        private BackpackPanel _backpackPanel;

        public TabStrip()
        {
            Update();

            if (1==1)//(_backpack && !Tabs.ContainsKey(3)) //Contains Key 3 translates to backpack enum
            {
                AddTab(TabTypes.Backpack);
            }

            InitializeTabs(); //Order is fucked because theres NO Z AXIS.... 
        }

        private void AddTab(TabTypes tab) //call when equipping a backpack and maybe equipping a pet?
        {
            Tabs.Add((int)tab, tab);
            Update();
        }

        private void Update()
        {
            int Y = -24;
            int X = 0;

            foreach (var tab in Tabs)
            {
                CutEdgeRect invTab;
                IconButton invTabButton;

                switch (tab.Value)
                {
                    case TabTypes.Inventory:
                        invTab = InventoryTab;
                        invTabButton = InventoryTabButton;
                        break;
                    case TabTypes.StatsView:
                        invTab = StatsViewTab;
                        invTabButton = StatsViewTabButton;
                        break;
                    case TabTypes.Backpack:
                        invTab = BackpackTab;
                        invTabButton = BackpackTabButton;
                        break;
                    case TabTypes.PetInfo:
                        continue;
                }

                invTab = new CutEdgeRect(new CutEdgeConfig { Width = 34, Height = 32, CutX = 4, CutY = 4, Cuts = CutEdges.Top, Color = currentTabIndex == tab.Key ? TabColor : BackgroundColor });
                invTab.X = X;
                invTab.Y = Y;

                AddChild(invTab);

                invTabButton = new IconButton(new IconButtonConfig
                {
                    Texture = TextureInfo.FromGameAtlas("lofiInterfaceBig", 23 + (tab.Key)),
                    Alpha = 1,
                    Padding = false,
                    X = X + 6,
                    Y = Y,
                    Width = 24,
                    Height = 24,
                    OnClick = () => OnTabSelected(tab.Value)
                });
                AddChild(invTabButton);

                X = invTab.X + 38;
            }
        }

        private void InitializeTabs()
        {
            _inventoryGrid = new InventoryGrid(Map.LocalPlayer, 4, Map.LocalPlayer.HasBackPack);
            _inventoryGrid.X = 0;
            _inventoryGrid.Y = 0;
            AddChild(_inventoryGrid);

            _statsPanel = new StatsPanel();
            _statsPanel.X = 0;
            _statsPanel.Y = 0;
            AddChild(_statsPanel);

            _backpackPanel = new BackpackPanel();
            _backpackPanel.X = 0;
            _backpackPanel.Y = 0;
            AddChild(_backpackPanel);

            SetTabVisibility(TabTypes.Inventory);
        }

        private void SetTabVisibility(TabTypes tabType)
        {
            _inventoryGrid.Visible = tabType == TabTypes.Inventory;
            _statsPanel.Visible = tabType == TabTypes.StatsView;
            _backpackPanel.Visible = tabType == TabTypes.Backpack;
        }

        private void OnTabSelected(TabTypes tabType)
        {
            currentTabIndex = (int)tabType;
            SetTabVisibility(tabType);
        }
    }

    public class StatsPanel : Sprite
    {
        public StatsPanel()
        {
            var bg = new CutEdgeRect(new CutEdgeConfig { Width = 224, Height = 150, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x242222 });
            AddChild(bg);

            SimpleText labelReal;
            string stats = "Stat 1 ect";
            labelReal = new SimpleText(new TextConfig { Text = "stats", FontSize = 16, Bold = true, X = 4, Y = 80 / 2, OutlineThickness = 1, Color = 0xFFFFFF, OutlineColor = 0xFFFFFF, Anchor = UiAnchor.MiddleLeft });
            AddChild(labelReal);
        }
    }

    public class BackpackPanel : Sprite
    {
        public BackpackPanel()
        {

        }
    }
}


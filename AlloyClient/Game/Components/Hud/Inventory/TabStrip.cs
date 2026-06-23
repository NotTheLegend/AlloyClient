using System.Collections.Generic;
using AlloyClient.Game.Objects;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using AlloyClient.Ui.Components.Buttons;
using AlloyClient.Utils;

namespace AlloyClient.Game.Components.Hud.Inventory
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

        private readonly uint TabColor = 2368034;
        private readonly uint BackgroundColor = 7039594;

        private Dictionary<int, TabTypes> Tabs = new Dictionary<int, TabTypes>()
        {
            { (int)TabTypes.Inventory, TabTypes.Inventory }, //Key is 1
            { (int)TabTypes.StatsView, TabTypes.StatsView }  //Key is 2
        };

        public int currentTabIndex = 1;

        private readonly IconButton InventoryTabButton;
        private readonly IconButton BackpackTabButton;
        private readonly IconButton StatsViewTabButton;

        private readonly CutEdgeRect InventoryTab;
        private readonly CutEdgeRect BackpackTab;
        private readonly CutEdgeRect StatsViewTab;

        private InventoryGrid _inventoryGrid;
        private InventoryGrid _backpackPanel;
        private StatsPanel _statsPanel;

        private CutEdgeRect invTab;
        private IconButton invTabButton;

        private readonly Entity _owner;

        public TabStrip(Entity owner)
        {
            _owner = owner;

            currentTabIndex = (int)TabTypes.Inventory;

            Update();
        }

        private void Update()
        {
            int Y = -24;
            int X = 6;

            if (Map.LocalPlayer.HasBackPack && !Tabs.ContainsKey(3)) 
            { 
                AddTab(TabTypes.Backpack); 
            }
            
            RemoveChildren(); //Remove All Panels
            _owner.InventoryUpdate.RemoveAll(); //Remove InventoryUpdate

            foreach (var tab in Tabs)
            {
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
                        break;
                }

                invTab = new CutEdgeRect(new CutEdgeConfig { Width = 34, Height = 24, CutX = 5, CutY = 5, Cuts = CutEdges.Top, Color = currentTabIndex == tab.Key ? TabColor : BackgroundColor });
                invTab.X = X;
                invTab.Y = Y;

                AddChild(invTab);

                invTabButton = new IconButton(new IconButtonConfig
                {
                    Texture = TextureHelper.FromGameAtlas("lofiInterfaceBig", 23 + (tab.Key), false),
                    Alpha = 1,
                    X = X + 6,
                    Y = Y,
                    Width = 24,
                    Height = 24,
                    OnClick = () => OnTabSelected(tab.Value)
                });

                AddChild(invTabButton);

                X = invTab.X + 40;
            }

            InitializeTabs();
        }

        private void AddTab(TabTypes tab)
        {
            Tabs.Add((int)tab, tab);
        }

        private void InitializeTabs()
        {
            _inventoryGrid = new InventoryGrid(Map.LocalPlayer, 4, false);
            AddChild(_inventoryGrid);

            _statsPanel = new StatsPanel();
            AddChild(_statsPanel);

            _backpackPanel = new InventoryGrid(Map.LocalPlayer, 12, false, true);
            AddChild(_backpackPanel);

            SetTabVisibility(currentTabIndex);
        }

        private void SetTabVisibility(int tabType)
        {
            _inventoryGrid.Visible = tabType == (int)TabTypes.Inventory;
            _statsPanel.Visible = tabType == (int)TabTypes.StatsView;
            _backpackPanel.Visible = tabType == (int)TabTypes.Backpack;
        }

        private void OnTabSelected(TabTypes tabType)
        {
            currentTabIndex = (int)tabType;
            Update();
        }
    }

    public class StatsPanel : Sprite
    {
        public StatsPanel()
        {
            var p = Map.LocalPlayer;
            int y = 150/2 - 16 - 10; //Height - Size - Spacing 
            int offset = 40;
            var bg = new CutEdgeRect(new CutEdgeConfig { Width = 224, Height = 150, CutX = 6, CutY = 6, Cuts = CutEdges.All, Color = 0x242222 });
            AddChild(bg);

            string[] IndexName = { "ATK" , "DEF", "SPD", "DEX" , "VIT" , "WIS"};
            int[] IndexValue = { p.Attack, p.Defense, p.Speed, p.Dexterity, p.Vitality, p.Wisdom};


            for (int i = 0; i < IndexValue.Length; i++) 
            {
                bool even = (i == 0 || i == 2 || i == 4);
                bool extraInfo = false;

                SimpleText StatName = new SimpleText(new TextConfig 
                { 
                    Text = IndexName[i], 
                    FontSize = 16, 
                    FontType = FontType.Normal, 
                    X = even ? offset : Width - 16 - offset*2, 
                    Y = y, 
                    OutlineThickness = 0, 
                    Color = 0xFFFFFF, 
                    OutlineColor = 0xFFFFFF, 
                    Anchor = UiAnchor.
                    MiddleLeft 
                });

                AddChild(StatName);

                SimpleText StatValue = new SimpleText(new TextConfig 
                { 
                    Text = IndexValue[i].ToString() + (extraInfo ? $" +{0}" : ""), 
                    FontSize = 16, 
                    FontType = FontType.Bold, 
                    X = (even ? offset : Width - 16 - offset*2) + StatName.Width + 5, //kinda gross but its needed
                    Y = y, 
                    OutlineThickness = 0, 
                    Color = 0xFFC800, 
                    OutlineColor = 0xFFFFFF, 
                    Anchor = UiAnchor.MiddleLeft 
                });

                AddChild(StatValue);

                y += even ? 0 : StatName.Height + 10;
            }
        }
    }
}


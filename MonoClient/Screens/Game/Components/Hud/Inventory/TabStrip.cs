using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MonoClient.Objects;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using NAudio.SoundFont;

namespace MonoClient.Screens.Game.Components.Hud.Inventory;

public sealed class TabStrip : Sprite {

    enum TabTypes
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

    Dictionary<int, TabTypes> Tabs = new Dictionary<int, TabTypes>()
    {
        { (int)TabTypes.Inventory, TabTypes.Inventory }, //Key is 1
        { (int)TabTypes.StatsView, TabTypes.StatsView }  //Key is 2
    };

    public int currentTabIndex = 1;

    CutEdgeRect InventoryTab;
    CutEdgeRect StatsViewTab;
    CutEdgeRect BackpackTab;

    public TabStrip() {

        Update();

        if (_backpack && !Tabs.ContainsKey(3)) //Contains Key 3 translates to backpack enum
        {
            AddTab(TabTypes.Backpack);
        }
    }  

    private void AddTab(TabTypes tab) //call when equiping a backpack and maybe equiping a pet?
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
            uint tempColor = currentTabIndex == tab.Key ? TabColor : BackgroundColor;

            switch (tab.Value)
            {
                case TabTypes.Inventory:
                    invTab = InventoryTab;
                    break;
                case TabTypes.StatsView:
                    invTab = StatsViewTab;
                    break;
                case TabTypes.Backpack:
                    invTab = BackpackTab;
                    break;
                case TabTypes.PetInfo:
                    //Lets hope we never need to show pet info :kappa:
                    break;
            }

            invTab = new CutEdgeRect(new CutEdgeConfig { Width = 34, Height = 32, CutX = 4, CutY = 4, Cuts = CutEdges.Top, Color = tempColor });
            invTab.X = X;
            invTab.Y = Y;

            AddChild(invTab);

            X = invTab.X + 38;
        } 
    }

    private void OnTabSelected()
    {

    }
}
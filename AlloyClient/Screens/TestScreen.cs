using System;
using Alloy.UiLib.BuiltIn;
using AlloyClient.Display;

namespace AlloyClient.Screens;

public class TestScreen : Screen {




    public TestScreen() {
        var c = new ColorRectConfig {
            Width = 100,
            Height = 100,
            Color = 0xFF0000
        };

        var r = new ColorRect(c);


        
        AddChild(r);
    }
    
}
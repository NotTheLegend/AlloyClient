using System.Threading.Tasks;
using Common;
using MonoClient.Assets;
using MonoClient.Data;
using MonoClient.Display;
using MonoClient.Screens.Title;
using MonoClient.State;
using MonoClient.Ui.Components.Graphics;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens;

public class LoadingScreen : Screen {
    
    public LoadingScreen() {
        var background = new ScreenGraphic(new ScreenGraphicConfig { Width = Settings.DefaultScreenWidth, Height = Settings.DefaultScreenHeight });
        AddChild(background);
        
        var config = new TextConfig {
            Text = "Loading...",
            FontSize = 40,
            FontType = FontType.Bold,
            X = Settings.DefaultScreenWidth / 2,
            Y = Settings.DefaultScreenHeight - 50,
            Color = 0xFFFFFF,
            Anchor = UiAnchor.Middle
        };

        var text = new SimpleText(config);
        AddChild(text);
        
        AddEventListener(UiLib.Core.Events.TaskEvent.Completed, Task.WhenAll(
            Account.LoadAsync(),
            //Task.Run(SoundManager.PreLoadSounds),
            //Task.Run(Music.PreLoadSongs),
            AssetParser.ParseAssetsAsync(),
            Task.Delay(2000) // Loading screen too fast lmao
        ), () => { ScreenManager.FadeToScreen(new TitleScreen(), Easing.SineInOut, 1000, 0x0); });
    }
}
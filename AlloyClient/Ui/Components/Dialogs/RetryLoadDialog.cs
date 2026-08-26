using Alloy.UiLib.Extra;
using AlloyClient.Display;
using AlloyClient.Screens;

namespace AlloyClient.Ui.Components.Dialogs;

public class RetryLoadDialog : Dialog {
    private static readonly DialogOption Retry = new ("Retry", () => ScreenManager.FadeToScreen(new LoadingScreen(true), Easing.SineInOut, 500, 0));
    private static readonly DialogOption Quit = new ("Quit", () => Main.OnQuit.Dispatch());

    public RetryLoadDialog()
        : base(
            "Load error",
            "Failed to load the game, server might be down, please try again later.",
            Retry,
            Quit) { }
}

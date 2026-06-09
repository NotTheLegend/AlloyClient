using Alloy.UiLib.Extra;
using AlloyClient.Display;
using AlloyClient.Screens;

namespace AlloyClient.Ui.Components.Dialogs;

public class RetryLoadDialog(string message) : Dialog(message, string.Empty, Retry, Quit) {
    private static readonly DialogOption Retry = new ("Retry", () => ScreenManager.FadeToScreen(new LoadingScreen(true), Easing.SineInOut, 500, 0));
    private static readonly DialogOption Quit = new ("Quit", () => Main.OnQuit.Dispatch());
}
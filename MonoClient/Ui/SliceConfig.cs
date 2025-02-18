using MonoClient.UiLib;

namespace MonoClient.Ui;

public static class SliceConfig {
    
    public const string StatusBar = "bar3";
    public const string ScrollBarBg = "ScrollBar/ScrollBarBackground";
    public const string ScrollBar = "ScrollBar/ScrollBarHandle";

    public const string TooltipBackgroundLarge = "tooltipBackgroundLarge";
    public const string TooltipBackgroundMedium = "tooltipBackgroundMedium";
    public const string TooltipBackgroundSmall = "tooltipBackgroundSmall";
    
    internal static void LoadSliceData() {
        SliceDataManager.LoadBuiltIn();
        SliceDataManager.CreateSlice(StatusBar, 7, 7, "bar3");
        
        SliceDataManager.CreateSlice(ScrollBarBg, 4, 4, "ScrollBar/ScrollBarBackground");
        SliceDataManager.CreateSlice(ScrollBar, 7, 7, "ScrollBar/ScrollBarHandle");

        SliceDataManager.CreateSlice(TooltipBackgroundLarge, 30, 30, "tooltipBackgroundLarge");
        SliceDataManager.CreateSlice(TooltipBackgroundMedium, 20, 20, "tooltipBackgroundMedium");
        SliceDataManager.CreateSlice(TooltipBackgroundSmall, 10, 10, "tooltipBackgroundSmall");
    }
}
using MonoClient.UiLib;

namespace MonoClient.Ui;

public static class SliceConfig {
    
    public const string StatusBar = "bar1";
    public const string ScrollBarBg = "ScrollBar/ScrollBarBackground";
    public const string ScrollBar = "ScrollBar/ScrollBarHandle";
    public const string TooltipBackground = "tooltipBackground";
    
    internal static void LoadSliceData() {
        SliceDataManager.LoadBuiltIn();
        SliceDataManager.CreateSlice(StatusBar, 7, 7, "bar1");
        
        SliceDataManager.CreateSlice(ScrollBarBg, 4, 4, "ScrollBar/ScrollBarBackground");
        SliceDataManager.CreateSlice(ScrollBar, 7, 7, "ScrollBar/ScrollBarHandle");
        SliceDataManager.CreateSlice(TooltipBackground, 30, 30, "tooltipBackground");
    }
}
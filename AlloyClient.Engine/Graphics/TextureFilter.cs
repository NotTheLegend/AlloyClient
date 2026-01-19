namespace AlloyClient.Engine.Graphics;

public readonly struct TextureFilter {
    
    public static readonly TextureFilter Nearest = new (TextureMinFilter.Nearest, TextureMagFilter.Nearest);
    
    public static readonly TextureFilter Linear = new (TextureMinFilter.Linear, TextureMagFilter.Linear);
    
    public readonly int MinFilter;

    public readonly int MagFilter;

    private TextureFilter(TextureMinFilter min, TextureMagFilter mag) {
        Check(min);
        Check(mag);
        
        MinFilter = (int)min;
        MagFilter = (int)mag;
    }
    
    private static void Check(TextureMinFilter val) {
        switch (val) {
            case TextureMinFilter.Nearest:
            case TextureMinFilter.Linear:
                break;
            default:
                throw new Exception("Not a valid texture filter");
        }
    }
    
    private static void Check(TextureMagFilter val) {
        switch (val) {
            case TextureMagFilter.Nearest:
            case TextureMagFilter.Linear:
                break;
            default:
                throw new Exception("Not a valid texture filter");
        }
    }
}
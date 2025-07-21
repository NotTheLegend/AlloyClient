namespace Common;

public struct Color {
    
    public byte R;

    public byte G;
    
    public byte B;
    
    public byte A;

    public Color(byte r, byte g, byte b, byte a) {
        R = r;
        G = g; 
        B = b; 
        A = a;
    }
    
    public Color(byte c) {
        R = G = B = c;
        A = 255;
    }

    public static Color Transparent = new Color(0, 0, 0, 0);
    public static Color Black = new Color(0);
}
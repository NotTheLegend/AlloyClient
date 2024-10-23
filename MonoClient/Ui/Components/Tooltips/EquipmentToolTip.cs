using System;

namespace MonoClient.Ui.Components.Tooltips;

public class EquipmentToolTip {

    public static float Round(float number, int decimalPlaces = 1) {
        float exp = MathF.Pow(10, decimalPlaces);
        if (decimalPlaces > 0) {
            number = (int)(number * exp) / exp;
        }
        else if (decimalPlaces == 0) {
            number = (int)number;
        }

        return number;
    }
}
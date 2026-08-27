using System;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Core;

public struct Transform2D {
    public float M11;
    public float M12;
    public float M21;
    public float M22;
    public float TX;
    public float TY;

    public static Transform2D Identity() {
        return new Transform2D {
            M11 = 1f,
            M22 = 1f
        };
    }

    public static Transform2D Create(float x, float y, float scaleX, float scaleY, float rotation, float anchorX, float anchorY) {
        var sin = MathF.Sin(rotation);
        var cos = MathF.Cos(rotation);
        var transform = new Transform2D {
            M11 = scaleX * cos,
            M12 = -scaleX * sin,
            M21 = scaleY * sin,
            M22 = scaleY * cos
        };

        transform.TX = x + transform.M11 * anchorX + transform.M12 * anchorY;
        transform.TY = y + transform.M21 * anchorX + transform.M22 * anchorY;
        return transform;
    }

    public static Transform2D Multiply(in Transform2D parent, in Transform2D local) {
        return new Transform2D {
            M11 = parent.M11 * local.M11 + parent.M12 * local.M21,
            M12 = parent.M11 * local.M12 + parent.M12 * local.M22,
            M21 = parent.M21 * local.M11 + parent.M22 * local.M21,
            M22 = parent.M21 * local.M12 + parent.M22 * local.M22,
            TX = parent.M11 * local.TX + parent.M12 * local.TY + parent.TX,
            TY = parent.M21 * local.TX + parent.M22 * local.TY + parent.TY
        };
    }

    public bool TryInvert(out Transform2D inverse) {
        var determinant = M11 * M22 - M12 * M21;
        if (MathF.Abs(determinant) <= float.Epsilon) {
            inverse = default;
            return false;
        }

        var reciprocal = 1f / determinant;
        inverse = new Transform2D {
            M11 = M22 * reciprocal,
            M12 = -M12 * reciprocal,
            M21 = -M21 * reciprocal,
            M22 = M11 * reciprocal
        };

        inverse.TX = -(inverse.M11 * TX + inverse.M12 * TY);
        inverse.TY = -(inverse.M21 * TX + inverse.M22 * TY);
        return true;
    }

    public Vector2 TransformPoint(Vector2 point) {
        return new Vector2(
            M11 * point.X + M12 * point.Y + TX,
            M21 * point.X + M22 * point.Y + TY
        );
    }

    public Vector2 TransformVector(Vector2 vector) {
        return new Vector2(
            M11 * vector.X + M12 * vector.Y,
            M21 * vector.X + M22 * vector.Y
        );
    }
}
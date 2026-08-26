using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Alloy.Common.Structs;
using OpenTK.Mathematics;

namespace Alloy.Common;

public static class Utils {

    extension(Vector2i vector) {
        public (int, int) ToPair() => (vector.X, vector.Y);

        public Vector2i Scale(Vector2 scale) => new ((int) (vector.X * scale.X), (int) (vector.Y * scale.Y));
    }
    
    public static AtlasData ReadAtlasData(this BinaryReader reader) {
        return new AtlasData {
            U = reader.ReadSingle(),
            V = reader.ReadSingle(),
            W = reader.ReadSingle(),
            H = reader.ReadSingle()
        };
    }

    extension(JsonElement element) {
        public float GetValueFloat(string name) => element.GetProperty(name).GetSingle();
        public int GetValueInt(string name) => element.GetProperty(name).GetInt32();
    }

    private static readonly Regex _letterRegex = new("[a-zA-Z]");
    public static int GetBase(string val) {
        var isHex = _letterRegex.IsMatch(val) && !val.EndsWith('x');
        return isHex ? 16 : 10;
    }
    
    extension(XElement element) {
        public T GetValue<T>(string n, T def = default) {
            if (element.Element(n) == null) {
                return def;
            }

            var val = element.Element(n)!.Value;
            var t = typeof(T);
            var b = GetBase(val);
            return Get<T>(val, t, b);
        }
        
        public T GetAttribute<T>(string n, T def = default) {
            if (element.Attribute(n) == null) {
                return def;
            }

            var val = element.Attribute(n)!.Value;
            var t = typeof(T);
            var b = GetBase(val);
            return Get<T>(val, t, b);
        }
        
        public bool GetElement(string name, out XElement elem) {
            var b = element.HasElement(name);
            elem = b ? element.Element(name) : null;
            return b;
        }
    
        public bool GetElements(string name, out XElement[] elem) {
            var b = element.HasElement(name);
            elem = b ? element.Elements(name).ToArray() : null;
            return b;
        }

        public bool HasElement(string name) {
            return element.Element(name) != null;
        }

        public bool HasAttribute(string name) {
            return element.Attribute(name) != null;
        }
    }
    
    private static T Get<T>(string val, Type t, int b) {
        if (t == typeof(string)) {
            return (T)Convert.ChangeType(val, t);
        }
        if (t == typeof(sbyte)) {
            return (T)Convert.ChangeType(Convert.ToSByte(val), t);
        }
        if (t == typeof(byte)) {
            return (T)Convert.ChangeType(Convert.ToByte(val), t);
        }
        if (t == typeof(short)) {
            return (T)Convert.ChangeType(Convert.ToInt16(val, b), t);
        }
        if (t == typeof(ushort)) {
            return (T)Convert.ChangeType(Convert.ToUInt16(val, b), t);
        }
        if (t == typeof(int)) {
            return (T)Convert.ChangeType(Convert.ToInt32(val, b), t);
        }
        if (t == typeof(uint)) {
            return (T)Convert.ChangeType(Convert.ToUInt32(val, b), t);
        }
        if (t == typeof(double)) {
            return (T) Convert.ChangeType(double.Parse(val, CultureInfo.InvariantCulture), t);
        }
        if (t == typeof(float)) {
            return (T) Convert.ChangeType(float.Parse(val, CultureInfo.InvariantCulture), t);
        }
        if (t == typeof(bool)) {
            return (T) Convert.ChangeType(string.IsNullOrWhiteSpace(val) || bool.Parse(val), t);
        }
        throw new Exception($"Type of {t} is not supported by Get");
    }

    extension(BinaryReader reader) {
        public Vector2 ReadVector2() => new(reader.ReadSingle(), reader.ReadSingle());
    
        public Vector3 ReadVector3() => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    
        public Vector4 ReadVector4() => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }

    extension(Path) {
        public static string CombineAlt(string path1, string path2) {
            return Path.Combine(path1, path2).Replace('\\', '/');
        }
    }
    
    public static int[] FromCommaSepString(this string src, string delim = ", ") {
        return src == string.Empty ? [] : src.Split(delim).Select(int.Parse).ToArray();
    }
}
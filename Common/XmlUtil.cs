using System.Globalization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Common;

public static class XmlUtil {
    public static T GetValue<T>(this XElement e, string n, T def = default) {
        if (e.Element(n) == null) {
            return def;
        }

        var val = e.Element(n)!.Value;
        var t = typeof(T);
        var b = GetBase(val);
        return Get<T>(val, t, b);
    }

    public static T GetAttribute<T>(this XElement e, string n, T def = default) {
        if (e.Attribute(n) == null) {
            return def;
        }

        var val = e.Attribute(n)!.Value;
        var t = typeof(T);
        var b = GetBase(val);
        return Get<T>(val, t, b);
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

    public static bool GetElement(this XElement e, string name, out XElement elem) {
        var b = e.HasElement(name);
        elem = b ? e.Element(name) : null;
        return b;
    }
    
    public static bool GetElements(this XElement e, string name, out XElement[] elem) {
        var b = e.HasElement(name);
        elem = b ? e.Elements(name).ToArray() : null;
        return b;
    }

    public static bool HasElement(this XElement e, string name) {
        return e.Element(name) != null;
    }

    public static bool HasAttribute(this XElement e, string name) {
        return e.Attribute(name) != null;
    }

    public static int GetBase(string val) {
        var isHex = val.Contains('x') && !val.EndsWith("x");
        return isHex ? 16 : 10;
    }
}

public static class XmlSerializer<T> {
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public static string Serialize(T obj) {
        using var writer = new MemoryStream();
        Serializer.Serialize(writer, obj);
        return Encoding.UTF8.GetString(writer.ToArray());
    }

    public static T Deserialize(string xml) {
        using var reader = new StringReader(xml);
        return (T) Serializer.Deserialize(reader);
    }
}
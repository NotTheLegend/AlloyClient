using System.Reflection;

namespace Alloy.Common;

public sealed record ShaderSource(string Name, string Vertex, string Fragment);

public sealed class EmbeddedShaders {

    private readonly Dictionary<string, ShaderSource> _shaders;
    
    private EmbeddedShaders(Assembly assembly) {
        _shaders = LoadAll(assembly);
    }
    
    public static EmbeddedShaders Init<T>() => new(typeof(T).Assembly);
    
    public ShaderSource this[string name] => _shaders.TryGetValue(name, out var shader) ? shader : throw new KeyNotFoundException($"No shader '{name}'. Available: {string.Join(", ", _shaders.Keys)}");

    private static Dictionary<string, ShaderSource> LoadAll(Assembly assembly) {
        const string vertExt = ".vert";
        const string fragExt = ".frag";
        
        var pairs = new Dictionary<string, (string Vert, string Frag)>(StringComparer.OrdinalIgnoreCase);

        foreach (var resource in assembly.GetManifestResourceNames()) {
            if (resource.EndsWith(vertExt, StringComparison.OrdinalIgnoreCase)) {
                var key = resource[..^vertExt.Length];
                pairs[key] = pairs.TryGetValue(key, out var p) ? (resource, p.Frag) : (resource, null);
            } else if (resource.EndsWith(fragExt, StringComparison.OrdinalIgnoreCase)) {
                var key = resource[..^fragExt.Length];
                pairs[key] = pairs.TryGetValue(key, out var p) ? (p.Vert, resource) : (null, resource);
            }
        }

        var result = new Dictionary<string, ShaderSource>(StringComparer.OrdinalIgnoreCase);

        foreach (var (baseName, (vert, frag)) in pairs) {
            if (vert is null || frag is null) {
                throw new InvalidOperationException(
                    $"Shader '{baseName}' is missing its {(vert is null ? "vertex" : "fragment")} stage.");
            }
            
            var name = baseName[(baseName.LastIndexOf('.') + 1)..];

            if (result.ContainsKey(name)) { 
                throw new InvalidOperationException($"Duplicate shader name '{name}' (from '{baseName}'). Rename one of the files.");
            }

            result[name] = new ShaderSource(name, ReadString(assembly, vert), ReadString(assembly, frag));
        }

        return result;
    }

    private static string ReadString(Assembly assembly, string resourceName) {
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Could not open resource '{resourceName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
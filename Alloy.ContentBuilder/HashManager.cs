using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Alloy.Common;

namespace Alloy.ContentBuilder;

public static class HashManager {

    private const string FileName = "content.hash";
    
    private static ReadOnlyDictionary<string, string> _oldHashes;

    private static Dictionary<string, string> _currentHashes = [];

    public static void Init(string binPath) {
        var hashes = new Dictionary<string, string>();
        var file = Path.CombineAlt(binPath, FileName);

        if (!File.Exists(file)) {
            _oldHashes = new ReadOnlyDictionary<string, string>(hashes);
            return;
        }

        var lines = File.ReadAllLines(file);
        foreach (var line in lines) {
            var kvp = line.Split(';');

            if (kvp.Length < 2)
                continue;

            hashes[kvp[0]] = kvp[1];
        }

        _oldHashes = new ReadOnlyDictionary<string, string>(hashes);
    }
    
    public static void SaveHashes(string binPath) {
        var file = Path.CombineAlt(binPath, "content.hash");
        File.Delete(file);
        File.WriteAllLines(file, _currentHashes.Select(s => $"{s.Key};{s.Value}"));
    }

    
    /// <returns>true if file hash is the same</returns>
    public static bool CheckFileHash(string filePath, Paths paths) {
        var data = File.ReadAllBytes(filePath);
        var md5 = MD5.HashData(data);
        var hash = Convert.ToBase64String(md5);
        var name = filePath.Replace(paths.Content, "");

        _currentHashes[name] = hash;

        if (_oldHashes.TryGetValue(name, out var oldHash))
            return oldHash == hash;
        
        return false;
    }
}
using System.Security.Cryptography;

namespace ContentBuilder;

public partial class Builder {

    private void CopyFolder(string folder, string ext, bool verbose) {
        var files = Directory.GetFiles(folder, ext, SearchOption.AllDirectories);

        var count = 0;

        if (verbose) {
            Console.WriteLine();
        }

        foreach (var file in files) {
            var data = File.ReadAllBytes(file);
            var md5 = MD5.HashData(data);
            var hash = Convert.ToBase64String(md5);
            var name = Path.GetFileName(file);

            CurrentHashes[name] = hash;
            
            if (OldHashes.TryGetValue(name, out var oldHash)) {
                if (oldHash == hash) {
                    if (verbose) {
                        Console.WriteLine($"Skipping file: {name}");
                    }
                    continue;
                }
                    
            }

            if (verbose) {
                Console.WriteLine($"Copying file: {name}");
            }
            
            //todo write file to output
            
            count++;
        }
        
        Console.WriteLine($"Updated {count}/{files.Length} from {folder}");
    }

    private void CopyFile(string file) {
        var data = File.ReadAllBytes(file);
        var md5 = MD5.HashData(data);
        var hash = Convert.ToBase64String(md5);
        var name = Path.GetFileName(file);

        CurrentHashes[name] = hash;

        if (OldHashes.TryGetValue(name, out var oldHash)) {
            if (oldHash == hash) {
                Console.WriteLine($"Skipping file: {name}");
                return;
            }

        }
        
        //todo write file to output

        Console.WriteLine($"Copying file: {name}");
    }
}
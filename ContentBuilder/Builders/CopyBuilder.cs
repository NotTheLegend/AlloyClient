namespace ContentBuilder.Builders;

public static class CopyBuilder {

    public static void Process(FolderSettings settings, Paths paths) {
        var files = Directory.GetFiles(settings.Folder, settings.Ext, SearchOption.AllDirectories);

        var count = 0;

        if (Builder.Verbose) {
            Console.WriteLine();
        }

        foreach (var file in files) {
            var name = Path.GetFileName(file);

            var isSame = HashManager.CheckFileHash(file, paths);

            if (isSame) {
                if (Builder.Verbose) Console.WriteLine($"Skipping file: {name}");
                continue;
            }

            if (Builder.Verbose) {
                Console.WriteLine($"Copying file: {name}");
            }

            Write(file, paths);
            
            count++;
        }
        
        Console.WriteLine($"Updated {count}/{files.Length} from {settings.Folder}");
    }
    
    public static void Process(FileSettings settings, Paths paths) {
        var name = Path.GetFileName(settings.File);
        var file = settings.File;
        
        var isSame = HashManager.CheckFileHash(settings.File, paths);

        if (isSame) {
            Console.WriteLine($"Skipping file: {name}");
            return;
        }
        
        Write(file, paths);

        Console.WriteLine($"Copying file: {name}");
    }

    private static void Write(string file, Paths paths) {
        var newPath = file.Replace(paths.Content, paths.Output);

        Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
        
        File.Copy(file, newPath, true);
    }
    
}
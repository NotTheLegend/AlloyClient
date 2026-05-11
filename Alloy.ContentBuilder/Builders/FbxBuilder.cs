using System.Numerics;
using SharpAssimp;

namespace Alloy.ContentBuilder.Builders;

public static class FbxBuilder {

    //Copied from monogame
    private const PostProcessSteps Flags = PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.FlipUVs | /* <- Required for Direct3D -> */ PostProcessSteps.FlipWindingOrder | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.ImproveCacheLocality | PostProcessSteps.OptimizeMeshes | PostProcessSteps.Triangulate;

    private static readonly AssimpContext Importer = new ();
    
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
                if (Builder.Verbose) Console.WriteLine($"Skipping model: {name}");
                continue;
            }

            if (Builder.Verbose) {
                Console.WriteLine($"Building model: {name}");
            }

            var scene = ImportModel(file);

            if (scene == null) {
                Console.WriteLine($"Error importing fbx: {file}");
                return;
            }
        
            var data = ProcessScene(scene);
        
            Write(data, file, paths);
            
            count++;
        }
        
        Console.WriteLine($"Updated {count}/{files.Length} from {settings.Folder}");
    }
    
    public static void Process(FileSettings settings, Paths paths) {
        var name = Path.GetFileName(settings.File);
        
        var isSame = HashManager.CheckFileHash(settings.File, paths);

        if (isSame) {
            Console.WriteLine($"Skipping file: {name}");
            return;
        }

        var scene = ImportModel(settings.File);

        if (scene == null) {
            Console.WriteLine($"Error importing fbx: {settings.File}");
            return;
        }
        
        var data = ProcessScene(scene);
        
        Write(data, settings.File, paths);

        Console.WriteLine($"Copying file: {name}");
    }

    private static Scene ImportModel(string file) {
        var scene = Importer.ImportFile(file, Flags);
        
        if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
        {
            Console.WriteLine("Unable to load model from: " + file);
            return null;
        }

        return scene;
    }

    private static ModelData ProcessScene(Scene scene) {
        List<int> ind = [];
        List<Vector3> vert = [];
        List<Vector3> norm = [];
        List<Vector2> uv = [];

        // hardcoded af but im not doing a full fbx load cuz rotmg models are just 1 mesh with no skeleton
        var m = scene.Meshes[scene.RootNode.Children[0].MeshIndices[0]];

        var hasUV = m.HasTextureCoords(0);
        
        for (var i = 0; i < m.FaceCount; i++)
        {
            var face = m.Faces[i];
            for (var j = 0; j < face.IndexCount; j++)
                ind.Add(face.Indices[j]);
        }

        for (var i = 0; i < m.VertexCount; i++) {
            vert.Add(m.Vertices[i]);    
            
            if (m.HasNormals) {
                norm.Add(m.Normals[i]);
            }

            if (hasUV) {
                uv.Add(new Vector2(m.TextureCoordinateChannels[0][i].X, m.TextureCoordinateChannels[0][i].Y));
            } else {
                uv.Add(new Vector2(0f));
            }
        }

        return new ModelData(ind, vert, norm, uv, hasUV);
    }

    private static void Write(ModelData data, string file, Paths paths) {
        var newFile = Path.ChangeExtension(file.Replace(paths.Content, paths.Output), ".bin");
        Directory.CreateDirectory(Path.GetDirectoryName(newFile)!);

        using var stream = File.Create(newFile);
        using var writer = new BinaryWriter(stream);
        data.Write(writer);
    }

    public static void Dispose() => Importer.Dispose();
}
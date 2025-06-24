using ContentBuilder;
using ContentBuilder.Builders;

if (args.Length != 3) {
    throw new Exception("Wrong number of arguments");
}

//args = ["E:\\Development\\Github\\mono-7.0\\MonoClient", "E:\\Development\\Github\\mono-7.0\\MonoClient\\bin\\Debug\\net8.0", "Content"];
        
var outputPath = Path.Combine(args[1], args[2]);
var contentPath = Path.Combine(args[0], args[2]);
var binPath = Path.Combine(contentPath, "bin");
var paths = new Paths(contentPath, outputPath);

HashManager.Init(binPath);

Builder.Run(paths);

HashManager.SaveHashes(binPath);

FbxBuilder.Dispose();

//todo jdoc stuff
//todo redo hash manager to allow parallel file processing for fbx and copy
//todo dye masks atlas creation
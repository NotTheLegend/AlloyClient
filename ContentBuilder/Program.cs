// Comment/uncomment to toggle user debug mode
//#define USER_DEBUG

using ContentBuilder;
using ContentBuilder.Builders;

#if !USER_DEBUG
if (args.Length != 3) {
    throw new Exception("Wrong number of arguments");
}
#endif

#if USER_DEBUG
const string clientProjectPath = "E:\\Development\\Github\\AlloyClient\\AlloyClient"; // hard path to client project (NOT the main(sln) folder)
const string clientProjectBin = "bin\\Debug\\net10.0"; // client build output path
const string contentFolder = "Content"; // folder to read content from and write content to in bin path

args = [clientProjectPath, Path.Combine(clientProjectPath, clientProjectBin), contentFolder];
#endif
        
var outputPath = Path.Combine(args[1], args[2]);
var contentPath = Path.Combine(args[0], args[2]);
var binPath = Path.Combine(contentPath, "bin");
var paths = new Paths(contentPath, Path.Combine(binPath, args[2]));

HashManager.Init(binPath);

Builder.Run(paths);

Builder.Copy(new Paths(paths.Output, outputPath));

HashManager.SaveHashes(binPath);

FbxBuilder.Dispose();

//todo jdoc stuff
//todo redo hash manager to allow parallel file processing for fbx and copy
//todo dye masks
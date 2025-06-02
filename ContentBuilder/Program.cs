using ContentBuilder;

if (args.Length != 3) {
    throw new Exception("Wrong number of arguments");
}
        
var outputPath = args[1];
var contentPath = Path.Combine(args[0], args[2]);

var builder = new Builder(contentPath, outputPath);
builder.Load();
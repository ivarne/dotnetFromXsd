using BaneNor.TafTap.Generator;

var rootElements = new List<string>{
    "PathCanceledMessage",
    "PathDetailsMessage",
};
var versions = new string[]{
    "3.2.0",
    "3.3.0",
};

foreach (var version in versions)
{
    var version_underscore = version.Replace('.', '_');
    var versionNamespace = $"BaneNor.TafTap.Models.v{version_underscore}";
    var versionFolder = Path.Join("..", "BaneNor.TafTap.Models", $"v{version_underscore}");
    Directory.CreateDirectory(versionFolder);
    Console.WriteLine($"Generationg TafTap modles for {version}");
    var generator = TafTapXsdReader.GetGeneratorForVersion(version);
    var rootClasses = new List<ClassModel>();
    if(rootElements.Count == 0)
    {
        rootElements = generator.GetMessageNames().ToList();
    }
    foreach (var rootElement in rootElements)
    {
        Console.WriteLine($"\t{rootElement}");
        rootClasses.Add(generator.Generate(rootElement));
    }

    var fileGenerator = new FileGenerator(versionNamespace, versionFolder, rootClasses);
    await fileGenerator.WriteFiles();
}
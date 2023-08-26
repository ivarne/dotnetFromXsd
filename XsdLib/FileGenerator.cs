using System.Text;
using System.Xml;

namespace XsdLib;


public class FileGenerator
{
    private readonly Dictionary<XmlQualifiedName, int> _referenceCount = new();

    private readonly List<ClassModel> _rootModels;
    private readonly string _versionFolder;
    private readonly string _versionNamespace;
    private readonly GeneratorSettings _settings;

    public FileGenerator(string versionNamespace, string versionFolder, List<ClassModel> rootModels, GeneratorSettings settings)
    {
        _versionFolder = versionFolder;
        _versionNamespace = versionNamespace;
        _rootModels = rootModels;
        _settings = settings;
        foreach (var model in rootModels)
        {
            _initializeReferenceCountRecursive(model);
        }
    }

    private void _initializeReferenceCountRecursive(ClassModel model)
    {
        // Increment count
        _referenceCount.TryGetValue(model.Name, out int count);
        _referenceCount[model.Name] = ++count;

        if (count == 1)
        {
            foreach (var prop in model.ClassProperties)
            {
                if (prop.ClassType is ClassModel cm)
                {
                    _initializeReferenceCountRecursive(cm);
                }
            }
        }
    }

    public async Task WriteFiles()
    {
        foreach (var model in _rootModels)
        {
            await GenerateFile(model);
        }
    }

    private async Task GenerateFile(ClassModel classModel)
    {
        var classModels = new List<ClassModel>()
        {
            classModel
        };
        await RecurseProperties(classModel, classModels);

        var cSharpCode = FormatCsharpFile(classModels);
        await File.WriteAllTextAsync(Path.Join(_versionFolder, $"{classModel.Name.Name}.cs"), cSharpCode, System.Text.Encoding.UTF8);
    }

    private async Task RecurseProperties(ClassModel classModel, List<ClassModel> classModels)
    {
        foreach (var property in classModel.ClassProperties)
        {
            if (property.ClassType is not null)
            {
                if (!_referenceCount.TryGetValue(property.ClassType.Name, out var count) || count < 2)
                {
                    classModels.Add(property.ClassType);
                    await RecurseProperties(property.ClassType, classModels);
                }
                else
                {
                    await GenerateFile(property.ClassType);
                }
            }
        }
    }


    private string FormatCsharpFile(List<ClassModel> classModels)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#pragma warning disable CS1591");
        sb.AppendLine("using System.Xml.Serialization;");
        sb.AppendLine("using System.Text.Json.Serialization;");
        sb.AppendLine();
        sb.AppendLine($"namespace {_versionNamespace};");
        sb.AppendLine();
        foreach (var cm in classModels)
        {
            cm.ToClass(sb, _settings);
        }
        return sb.Replace("\r", "").ToString();
    }
}
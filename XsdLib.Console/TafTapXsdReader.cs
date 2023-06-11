using System.Xml;
using System.Xml.Schema;
using System.IO;

namespace XsdLib;


public static class TafTapXsdReader
{
    public static XmlSchemaSet GetSchemaForVersion(string version)
    {
        return version switch 
        {
            "3.1.0" => GetSchemaForFolder("Baseline 3.1.0"),
            "3.2.0" => GetSchemaForFolder("Baseline 3.2.0"),
            "3.3.0" => GetSchemaForFolder("Baseline 3.3.0"),
            string _any => throw new Exception($"No TafTap tsi schema found with version {_any}"),
        };
    }

    public static Generator GetGeneratorForVersion(string version)
    {
        return new Generator(GetSchemaForVersion(version));
    }

    private static XmlSchemaSet GetSchemaForFolder(string folder)
    {
        var schemaSet = new XmlSchemaSet();
        var directory = new DirectoryInfo(Path.Join("..", "BaneNor.TafTap.Models", "TafTapXsd", folder));
        foreach(var file in directory.GetFiles("*.xsd"))
        {
            schemaSet.Add(null, file.FullName);
        }
        schemaSet.Compile();
        return schemaSet;
    }
}
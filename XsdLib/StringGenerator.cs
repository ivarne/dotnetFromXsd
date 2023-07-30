using System.Text;

namespace XsdLib;

public static class StringGenerator
{
    public static string ToClass(ClassModel root, GeneratorSettings settings)
    {
        var sb = new StringBuilder();
        ToClassRecursive(root, sb, settings);
        return sb.ToString();
    }

    public static void ToClassRecursive(ClassModel element, StringBuilder sb, GeneratorSettings settings)
    {
        element.ToClass(sb, settings);
        foreach(var prop in element.ClassProperties)
        {
            if(prop.ClassType is ClassModel cm)
            {
                ToClassRecursive(cm, sb, settings);
            }
        }
    }
}
using System.Text;

namespace XsdLib;

public static class StringGenerator
{
    public static string ToClass(ClassModel root)
    {
        var sb = new StringBuilder();
        ToClassRecursive(root, sb);
        return sb.ToString();
    }

    public static void ToClassRecursive(ClassModel element, StringBuilder sb)
    {
        element.ToClass(sb);
        foreach(var prop in element.ClassProperties)
        {
            if(prop.ClassType is ClassModel cm)
            {
                ToClassRecursive(cm, sb);
            }
        }
    }
}
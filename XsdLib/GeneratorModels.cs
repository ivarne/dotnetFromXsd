using System.Text;
using System.Xml.Schema;

namespace XsdLib;

public class ClassModel
{
    public required System.Xml.XmlQualifiedName Name { get; set; }
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public required List<ClassProperty> ClassProperties { get; set; }
    public required bool IsRootClass { get; set; }

    public void ToClass(StringBuilder sb, GeneratorSettings settings)
    {
        sb.AppendLine("\n"); // Two empty lines before class definition (between namespace or previous class)
        if (Summary is not null)
        {
            sb.Append($$"""
            /// <summary>
            /// {{System.Security.SecurityElement.Escape(Summary.ReplaceLineEndings("\n/// "))}}
            /// </summary>
            
            """);
        }
        if (Remarks is not null)
        {
            sb.Append($$"""
            /// <remarks>
            /// {{System.Security.SecurityElement.Escape(Remarks.ReplaceLineEndings("\n/// "))}}
            /// </remarks>
            
            """);
        }
        sb.AppendLine($"""[XmlType(AnonymousType = true, Namespace = "{Name.Namespace}")]""");
        if (IsRootClass)
        {
            sb.AppendLine($"""[XmlRoot(Namespace = "{Name.Namespace}", IsNullable = false)]""");
        }
        sb.Append($$"""
        public class {{Name.Name}}
        {
        
        """);
        bool first = true;
        foreach (var prop in ClassProperties)
        {
            if (!first)
            {
                sb.Append('\n');
            }
            else
            {
                first = false;
            }
            prop.ToProperty(sb, settings);
        }
        sb.Append("}");
    }
}

public class ClassProperty
{
    public required string Type { get; set; }
    public ClassModel? ClassType { get; set; }
    public required string Name { get; set; }
    public required bool Required { get; set; }
    public required decimal MaxOccurs { get; set; } = 1;
    public AttributeData[] Attributes { get; set; } = Array.Empty<AttributeData>();
    public string? Summary { get; set; }
    public string? Remarks { get; set; }

    public string GetFullType() => MaxOccurs > 1 ? $"List<{Type}>" : Type;
    public void ToProperty(StringBuilder sb, GeneratorSettings settings)
    {
        if (Summary is not null)
        {
            sb.Append($$"""
                /// <summary>
                /// {{System.Security.SecurityElement.Escape(Summary.ReplaceLineEndings("\n    /// "))}}
                /// </summary>
            
            """);
        }
        if (Remarks is not null)
        {
            sb.Append($$"""
                /// <remarks>
                /// {{System.Security.SecurityElement.Escape(Remarks.ReplaceLineEndings("\n    /// "))}}
                /// </remarks>
            
            """);
        }
        foreach (var attr in Attributes)
        {
            sb.Append($"    [{attr.ToAttr()}]\n");
        }
        sb.Append($$"""
            public {{(Required ? "required " : "")}}{{GetFullType()}}{{(Required ? "" : "?")}} {{Name}} { get; set; }
        
        """);
        if (!Required)
        {
            sb.AppendLine("    [XmlIgnore]");
            if (settings.JsonAttributes)
            {
                sb.AppendLine("    [JsonIgnore]");
            }
            sb.AppendLine($"    public bool {Name}Specified => {Name} != null;");
        }
        // sb.Append("    public ");
        // if (Required)
        // {
        //     sb.Append("required ");
        // }
        // sb.Append(Type);
        // if (!Required)
        // {
        //     sb.Append("?");
        // }
        // sb.Append(" ");
        // sb.Append(Name);
        // sb.Append(" { get; set; }\n");
    }
}



public static class XmlTypeCodeExtentions
{
    public static string? ToDataType(this XmlTypeCode? xmlTypeCode)
        => xmlTypeCode switch
        {
            XmlTypeCode.AnyUri => "\"anyURI\"",
            XmlTypeCode.Base64Binary => "\"base64Binary\"",
            XmlTypeCode.Boolean => "\"boolean\"",
            XmlTypeCode.Byte => "\"byte\"",
            XmlTypeCode.Date => "\"date\"",
            XmlTypeCode.DateTime => "\"dateTime\"",
            XmlTypeCode.Decimal => "\"decimal\"",
            XmlTypeCode.Double => "\"double\"",
            XmlTypeCode.Entity => "\"ENTITY\"",
            XmlTypeCode.Float => "\"float\"",
            XmlTypeCode.GDay => "\"gDay\"",
            XmlTypeCode.GMonth => "\"gMonth\"",
            XmlTypeCode.GMonthDay => "\"gMonthDay\"",
            XmlTypeCode.GYear => "\"gYear\"",
            XmlTypeCode.GYearMonth => "\"gYearMonth\"",
            XmlTypeCode.HexBinary => "\"hexBinary\"",
            XmlTypeCode.Id => "\"ID\"",
            XmlTypeCode.Idref => "\"IDREF\"",
            XmlTypeCode.Int => "\"int\"",
            XmlTypeCode.Integer => "\"integer\"",
            XmlTypeCode.Language => "\"language\"",
            XmlTypeCode.Long => "\"long\"",
            XmlTypeCode.Name => "\"Name\"",
            XmlTypeCode.NCName => "\"NCName\"",
            XmlTypeCode.NegativeInteger => "\"negativeInteger\"",
            XmlTypeCode.NmToken => "\"NMTOKEN\"",
            XmlTypeCode.NormalizedString => "\"normalizedString\"",
            XmlTypeCode.NonNegativeInteger => "\"nonNegativeInteger\"",
            XmlTypeCode.NonPositiveInteger => "\"nonPositiveInteger\"",
            XmlTypeCode.Notation => "\"NOTATION\"",
            XmlTypeCode.PositiveInteger => "\"positiveInteger\"",
            XmlTypeCode.QName => "\"QName\"",
            XmlTypeCode.Duration => "\"duration\"",
            // Ignore string, it's the default type anyway
            // XmlTypeCode.String => "\"string\"",
            XmlTypeCode.Short => "\"short\"",
            XmlTypeCode.Time => "\"time\"",
            XmlTypeCode.Token => "\"token\"",
            XmlTypeCode.UnsignedByte => "\"unsignedByte\"",
            XmlTypeCode.UnsignedInt => "\"unsignedInt\"",
            XmlTypeCode.UnsignedLong => "\"unsignedLong\"",
            XmlTypeCode.UnsignedShort => "\"unsignedShort\"",
            _ => null,
        };

}
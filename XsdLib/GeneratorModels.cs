using System.Text;

namespace XsdLib;

public class ClassModel
{
    public required System.Xml.XmlQualifiedName Name { get; set; }
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public required List<ClassProperty> ClassProperties { get; set; }

    public void ToClass(StringBuilder sb)
    {
        if (Summary is not null)
        {
            sb.Append($$"""
            /// <summary>
            /// {{{System.Security.SecurityElement.Escape(Summary.ReplaceLineEndings("\n/// "))}}
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
        sb.Append($$"""
        [XmlType(AnonymousType=true, Namespace = "{{Name.Namespace}}")]
        [XmlRoot(Namespace = "{{Name.Namespace}}", IsNullable=false)]
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
            prop.ToProperty(sb);
        }
        sb.Append("}\n\n");
    }
}

public class ClassProperty
{
    public required string Type { get; set; }
    public ClassModel? ClassType { get; set; }
    public required string Name { get; set; }
    public required bool Required { get; set; }
    public required decimal MaxOccurs { get; set; } = 1;
    public IEnumerable<string> ClassPropertyAttributes { get; set; } = Enumerable.Empty<string>();
    public string? Summary { get; set; }
    public string? Remarks { get; set; }

    public string GetFullType() => MaxOccurs > 1 ? $"List<{Type}>" : Type;
    public void ToProperty(StringBuilder sb)
    {
        if (Summary is not null)
        {
            sb.Append($$"""
                /// <summary>
                /// {{{System.Security.SecurityElement.Escape(Summary.ReplaceLineEndings("\n    /// "))}}
                /// </summary>
            
            """);
        }
        if (Remarks is not null)
        {
            sb.Append($$"""
                /// <remarks>
                /// {{{System.Security.SecurityElement.Escape(Remarks.ReplaceLineEndings("\n    /// "))}}
                /// </remarks>
            
            """);
        }
        foreach (var attr in ClassPropertyAttributes)
        {
            sb.Append($"    {attr}\n");
        }
        sb.Append($$"""
            public {{(Required ? "required " : "")}}{{GetFullType()}}{{(Required ? "" : "?")}} {{Name}} { get; set; }
        
        """);
        if (!Required)
        {
            sb.Append($$"""
                [XmlIgnore]
                [JsonIgnore]
                public bool {{Name}}Specified => {{Name}} != null;
            
            """);
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
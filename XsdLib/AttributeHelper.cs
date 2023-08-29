using System.Text;
using System.Xml.Schema;
using XsdLib.Utils;

namespace XsdLib;

public abstract record AttributeData(string Name)
{
    protected virtual IEnumerable<string> PositionalConstructorArguments => Enumerable.Empty<string>();
    private List<NamedConstructorArgument> NamedConstructorArguments { get; set; } = new List<NamedConstructorArgument>();
    public void Add(string name, string? value)
    {
        if (value is not null)
            NamedConstructorArguments.Add(new(name, value));
    }

    public string? ToAttr()
    {
        if (NamedConstructorArguments.Count == 0 && !PositionalConstructorArguments.Any())
        {
            return Name;
        }
        var sb = new StringBuilder();
        sb.Append(Name);
        sb.Append("(");
        var iter = 0;
        foreach (var argument in PositionalConstructorArguments)
        {
            if (iter++ != 0)
            {
                sb.Append(", ");
            }
            sb.Append(argument);
        }
        foreach (var ca in NamedConstructorArguments)
        {
            if (iter++ != 0)
            {
                sb.Append(", ");
            }
            sb.Append(ca.Name);
            sb.Append(" = ");
            sb.Append(ca.Value);
        }
        sb.Append(")");
        return sb.ToString();
    }
}
public record NamedConstructorArgument(string Name, string Value);


public record XmlElementAttributeData() : AttributeData("XmlElement")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", XmlTypeCodeExtentions.ToDataType(value));
        }
    }
    public string? ElementName
    {
        init
        {
            if (value is not null)
                Add("ElementName", $"\"{value}\"");
        }
    }
    public XmlSchemaForm? Form
    {
        init
        {
            if (value is not null)
                Add("Form", $"System.Xml.Schema.XmlSchemaForm.{value}");
        }
    }
    public bool? IsNullable
    {
        init
        {
            if (value is not null)
                Add("IsNullable", $"{value}");
        }
    }
    public string? Namespace
    {
        init
        {
            if (value is not null)
                Add("Namespace", $"\"{value}\"");
        }
    }
    public int? Order
    {
        init
        {
            if (value is not null)
                Add("Order", $"{value}");
        }
    }
    public Type? Type
    {
        init
        {
            if (value is not null)
                Add("Type", $"typeof({value.FullName})");
        }
    }
}
public record XmlAttributeAttributeData() : AttributeData("XmlAttribute")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", XmlTypeCodeExtentions.ToDataType(value));
        }
    }
    public string? AttributeName
    {
        init
        {
            if (value is not null)
                Add("AttributeName", $"\"{value}\"");
        }
    }
    public XmlSchemaForm? Form
    {
        init
        {
            if (value is not null)
                Add("Form", $"System.Xml.Schema.XmlSchemaForm.{value}");
        }
    }
    public string? Namespace
    {
        init
        {
            if (value is not null)
                Add("Namespace", $"\"{value}\"");
        }
    }

    public Type? Type
    {
        init
        {
            if (value is not null)
                Add("Type", $"typeof({value.FullName})");
        }
    }
}

public record XmlTextAttributeData() : AttributeData("XmlText")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", XmlTypeCodeExtentions.ToDataType(value));
        }
    }
    public Type? Type
    {
        init
        {
            if (value is not null)
                Add("Type", $"typeof({value})");
        }
    }
}

public abstract record DataAnnotationsAttribute(string name) : AttributeData(name);

public record RegularExpressionAttribute(string RegularExpression) : DataAnnotationsAttribute("RegularExpression")
{
    public static RegularExpressionAttribute? FromRestriction(XmlSchemaSimpleTypeRestriction? restriction)
    {
        // Turn PatternFacet into [RegularExpression()]
        var regex = restriction?.Facets.OfType<XmlSchemaPatternFacet>().FirstOrDefault()?.Value;
        if (regex is not null)
        {
            return new RegularExpressionAttribute(regex);
        }
        return null;
    }

    protected override IEnumerable<string> PositionalConstructorArguments
    {
        get
        {
            yield return $"@\"{RegularExpression.Replace("\"", "\\\"")}\"";
        }
    }
}

public record StringLengthAttribute(int max) : DataAnnotationsAttribute("StringLength")
{
    public static StringLengthAttribute? FromRestriction(XmlSchemaSimpleTypeRestriction? restriction)
    {
        // Turn Max/MinLengthFacet into [StringLength(...)]
        var maxLengthS = restriction?.Facets.OfType<XmlSchemaMaxLengthFacet>().FirstOrDefault()?.Value;
        if (int.TryParse(maxLengthS, out int maxLength))
        {
            var minLengthS = restriction?.Facets.OfType<XmlSchemaMinLengthFacet>().FirstOrDefault()?.Value;
            return new StringLengthAttribute(maxLength)
            {
                MinimumLength = int.TryParse(minLengthS, out var minLength) ? minLength : null
            };
        }

        return null;
    }

    protected override IEnumerable<string> PositionalConstructorArguments
    {
        get
        {
            yield return max.ToString();
        }
    }
    public int? MinimumLength
    {
        init
        {
            Add("MinimumLength", value?.ToString());
        }
    }
}
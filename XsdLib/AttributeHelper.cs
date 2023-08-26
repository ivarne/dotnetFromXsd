using System.Text;
using System.Xml.Schema;

namespace XsdLib;

public record AttributeData(string Name)
{
    private List<AttributeDataConstructorArgument> ConstructorArguments { get; set; } = new List<AttributeDataConstructorArgument>();
    public void Add(string name, string? value)
    {
        if (value is not null)
            ConstructorArguments.Add(new(name, value));
    }

    public string? ToAttr()
    {
        if (ConstructorArguments.Count == 0)
        {
            return Name;
        }
        var sb = new StringBuilder();
        sb.Append(Name);
        sb.Append("(");
        var iter = 0;
        foreach (var ca in ConstructorArguments)
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
public record AttributeDataConstructorArgument(string Name, string Value);


public record XmlElementAttributeData() : AttributeData("XmlElement")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", value.ToDataType());
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
                Add("Type", $"typeof({value})");
        }
    }
}
public record XmlAttributeAttributeData() : AttributeData("XmlAttribute")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", value.ToDataType());
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
                Add("Type", $"typeof({value})");
        }
    }
}

public record XmlTextAttributeData() : AttributeData("XmlText")
{
    public XmlTypeCode? DataType
    {
        init
        {
            Add("DataType", value.ToDataType());
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


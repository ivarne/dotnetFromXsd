using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace XsdLib;

public class Generator
{
    private readonly XmlSchemaSet _schemaSet;
    private readonly GeneratorSettings _settings;

    public Generator(XmlSchemaSet schemaSet, GeneratorSettings settings)
    {
        _schemaSet = schemaSet;
        _settings = settings;
    }
    public ClassModel Generate(string rootElement)
    {
        var element = GetElementByName(rootElement);
        if (element is null)
        {
            throw new Exception($"Could not find element with root {rootElement}");
        }
        return AddClassModel(element);

    }

    public IEnumerable<string> GetMessageNames()
    {
        return GetMessageNames(_schemaSet);
    }
    public  static IEnumerable<string> GetMessageNames(XmlSchemaSet schemaSet)
    {
        foreach (var schemaObj in schemaSet.Schemas())
        {
            if (schemaObj is XmlSchema schema)
            {
                foreach (var elementObj in schema.Elements.Values)
                {
                    if (elementObj is XmlSchemaElement element)
                    {
                        // if (element.Name?.EndsWith("Message") == true)
                        if (element.Name is not null)
                        {
                            yield return element.Name;
                        }
                    }
                }
            }
        }
    }



    private ClassModel AddClassModel(XmlSchemaElement element)
    {
        if (element.ElementSchemaType is XmlSchemaComplexType elementSchemaType)
        {
            var cm = new ClassModel
            {
                Name = element.QualifiedName,
                Summary = GetSummary(element),
                // Remarks = $"Xml namespace: {element.QualifiedName.Namespace}",
                ClassProperties = new(),
                IsRootClass = this._schemaSet.GlobalElements.Contains(element.QualifiedName),
            };

            cm.ClassProperties = GetClassProperties(elementSchemaType);
            return cm;
        }
        throw new NotImplementedException("element.ElementSchemaType is not XmlSchemaComplexType");
    }

    private List<ClassProperty> GetClassProperties(XmlSchemaComplexType elementSchemaType)
    {
        var ret = new List<ClassProperty>();
        if (elementSchemaType.ContentTypeParticle is XmlSchemaSequence sequence)
        {
            foreach (var item in sequence.Items)
            {
                if (item is XmlSchemaElement element)
                {
                    var prop = GetClassProperty(element);
                    ret.Add(prop);
                }
                else if (item is XmlSchemaSequence recursiveSequence)
                {
                    foreach (var rItem in recursiveSequence.Items)
                    {
                        if (rItem is XmlSchemaElement rElement)
                        {
                            var prop = GetClassProperty(rElement);
                            prop.MaxOccurs = recursiveSequence.MaxOccurs;
                            prop.Required = recursiveSequence.MinOccurs > 0;
                            ret.Add(prop);
                        }
                        else
                        {
                            throw new NotImplementedException($"only elements supported in recursive sequences. Found {rItem?.GetType()}");
                        }
                    }
                }
                else if (item is XmlSchemaChoice itemChoice)
                {
                    // TODO: one class or the other
                }
                else if (item is XmlSchemaAny any)
                {
                    // TODO !!!
                }
                else
                {
                    //TODO? add support if relevant xsd has other structure.
                    throw new NotImplementedException($"GetClassProperties is not implemented for: {item.GetType()}");
                }
            }
        }
        else if (elementSchemaType.ContentTypeParticle is XmlSchemaChoice choice)
        {
            foreach (var item in choice.Items)
            {
                if (item is XmlSchemaElement element)
                {
                    var prop = GetClassProperty(element);
                    prop.Required = false;
                    prop.Remarks = $"TODO:!!! figure out if this is correct";
                    ret.Add(prop);
                }
                else
                {
                    throw new NotImplementedException($"GetClassProperties not implemented for coice items of type: {item.GetType()}");
                }
            }
        }
        else if (elementSchemaType.BaseXmlSchemaType is XmlSchemaSimpleType simpleContent)
        {
            ret.Add(new()
            {
                Name = "Value",
                MaxOccurs = 1,
                Required = true,
                // Summary = GetSummary(simpleContent),
                Remarks = GetRestrictionsAsString(simpleContent),
                Type = GetSimpleDataType(simpleContent),
                Attributes = new AttributeData[]{
                    new XmlTextAttributeData()
                    {
                        DataType = simpleContent.Datatype?.TypeCode
                    }
                },
            });
            foreach (var attributeObj in elementSchemaType.AttributeUses.Values)
            {
                if (attributeObj is XmlSchemaAttribute attribute)
                {
                    ret.Add(new()
                    {
                        Name = attribute.QualifiedName.Name,
                        Type = "string", // attributes can't be nullable value types (int, long, DateTime), so just make everything string
                        MaxOccurs = 1,
                        Required = attribute.Use == XmlSchemaUse.Required,
                        // TODO:!
                        // Summary = 
                        // Remarks =
                        Attributes = new AttributeData[]
                        {
                            new XmlAttributeAttributeData()
                            {
                                Namespace = attribute.QualifiedName.Namespace,
                                Form = XmlSchemaForm.Qualified,
                                DataType = attribute.AttributeSchemaType?.TypeCode
                            },
                        }
                    });
                }
            }
        }
        else
        {
            //TODO? add support if relevant xsd has other structure.
            throw new NotImplementedException($"GetClassProperties is not implemented for ContentTypeParticle of type: {elementSchemaType.ContentTypeParticle.GetType()}");
        }
        return ret;
    }

    private ClassProperty GetClassProperty(XmlSchemaElement element)
    {
        switch (element.ElementSchemaType)
        {
            case XmlSchemaComplexType ct:
                if(ct.BaseXmlSchemaType is XmlSchemaSimpleType simpleContent && ct.AttributeUses.Count == 0)
                {
                    // A complex type with a single attribute free content, should just be a single property, not a class with a .Value property
                    return new()
                    {
                        Name = element.QualifiedName.Name,
                        Type = GetSimpleDataType(simpleContent),
                        Summary = GetSummary(element),
                        Remarks = GetRestrictionsAsString(simpleContent),
                        Required = element.MinOccurs > 0,
                        MaxOccurs = element.MaxOccurs,
                        Attributes = new AttributeData[]
                        {
                            new XmlElementAttributeData()
                            {
                                DataType = simpleContent.Datatype?.TypeCode
                            },
                            // $"""[JsonPropertyName("{char.ToLowerInvariant(element.QualifiedName.Name[0])}{element.QualifiedName.Name.Substring(1)}")]""",
                        }
                    };
                }

                var classModel = AddClassModel(element);
                return new()
                {
                    Name = element.QualifiedName.Name,
                    Type = element.QualifiedName.Name,
                    ClassType = classModel,
                    Summary = GetSummary(element),
                    Required = element.MinOccurs > 0,
                    MaxOccurs = element.MaxOccurs,
                    Attributes = new AttributeData[]
                    {
                        new XmlElementAttributeData()
                        // $"""[JsonPropertyName("{char.ToLowerInvariant(element.QualifiedName.Name[0])}{element.QualifiedName.Name.Substring(1)}")]""",
                    }
                };
            case XmlSchemaSimpleType st:
                // var rootElementDefinition = GetElementByName(element.QualifiedName);
                return new()
                {
                    Name = element.QualifiedName.Name,
                    Type = GetSimpleDataType(st),
                    Summary = GetSummary(element),
                    Remarks = GetRestrictionsAsString(st),
                    Required = element.MinOccurs > 0,
                    MaxOccurs = element.MaxOccurs,
                    Attributes = new AttributeData[]
                    {
                        new XmlElementAttributeData()
                        {
                            DataType = st.Datatype?.TypeCode
                        },
                        // $"""[JsonPropertyName("{char.ToLowerInvariant(element.QualifiedName.Name[0])}{element.QualifiedName.Name.Substring(1)}")]""",
                    }
                };
            default:
                throw new NotImplementedException($"Unknown schema property type {element.ElementSchemaType?.GetType()}");
        }

    }

    private static string? GetRestrictionsAsString(XmlSchemaSimpleType st)
    {
        var sb = new StringBuilder();
        switch (st.Content)
        {
            case XmlSchemaSimpleTypeRestriction simpleTypeRestriction:
                foreach (var facet in simpleTypeRestriction.Facets)
                {
                    switch (facet)
                    {
                        case XmlSchemaMaxLengthFacet maxLengthFacet:
                            if (maxLengthFacet.Value is not null)
                            {
                                sb.AppendLine($"MaxLength: {maxLengthFacet.Value}");
                            }
                            break;
                        case XmlSchemaMinLengthFacet minLengthFacet:
                            if (minLengthFacet.Value is not null)
                            {
                                sb.AppendLine($"MinLength: {minLengthFacet.Value}");
                            }
                            break;
                        case XmlSchemaMinInclusiveFacet minInclusiveFacet:
                            if (minInclusiveFacet.Value is not null)
                            {
                                sb.AppendLine($"MinInclusive: {minInclusiveFacet.Value}");
                            }
                            break;
                        case XmlSchemaMaxInclusiveFacet maxInclusiveFacet:
                            if (maxInclusiveFacet.Value is not null)
                            {
                                sb.AppendLine($"MaxInclusive: {maxInclusiveFacet.Value}");
                            }
                            break;
                        case XmlSchemaPatternFacet patternFacet:
                            if (patternFacet.Value is not null)
                            {
                                sb.AppendLine($"Pattern: {patternFacet.Value}");
                            }
                            break;
                        case XmlSchemaEnumerationFacet enumerationFacet:
                            if (enumerationFacet.Value is not null)
                            {
                                sb.AppendLine($"Enumeration: {enumerationFacet.Value}");
                            }
                            break;
                        case XmlSchemaWhiteSpaceFacet whiteSpaceFacet:
                            if (whiteSpaceFacet.Value is not null)
                            {
                                sb.AppendLine($"WhiteSpace: {whiteSpaceFacet.Value}");
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
            default:
                break;
        }

        return sb.Length > 0 ? sb.ToString().Trim() : null;
    }

    private static string GetSimpleDataType(XmlSchemaSimpleType st)
    {
        //See: https://learn.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlelementattribute.datatype?view=net-7.0#system-xml-serialization-xmlelementattribute-datatype
        return st.Datatype?.TypeCode switch
        {
            XmlTypeCode.DateTime => "DateTime",
            XmlTypeCode.Date => "DateTime",
            XmlTypeCode.String => "string",
            XmlTypeCode.Text => "string",
            XmlTypeCode.Token => "string",
            XmlTypeCode.Integer => "string",
            XmlTypeCode.Double => "double",
            XmlTypeCode.Short => "short",
            XmlTypeCode.PositiveInteger => "string",
            XmlTypeCode.Decimal => "decimal",
            XmlTypeCode.Float => "float",
            XmlTypeCode.Boolean => "bool",
            XmlTypeCode.Int => "int",
            XmlTypeCode.Time => "DateTime",
            XmlTypeCode.Byte => "byte",
            XmlTypeCode.GYear => "string",
            XmlTypeCode.AnyUri => "string",
            XmlTypeCode.Base64Binary => "byte[]",
            XmlTypeCode.Entity => "string",
            XmlTypeCode.GDay => "string",
            XmlTypeCode.GMonth => "string",
            XmlTypeCode.GMonthDay => "string",
            XmlTypeCode.GYearMonth => "string",
            XmlTypeCode.HexBinary => "byte[]",
            XmlTypeCode.Id => "string",
            XmlTypeCode.Idref => "string",
            XmlTypeCode.Language => "string",
            XmlTypeCode.Long => "long",
            XmlTypeCode.Name => "string",
            XmlTypeCode.NCName => "string",
            XmlTypeCode.NegativeInteger => "string",
            XmlTypeCode.NmToken => "string",
            XmlTypeCode.NormalizedString => "string",
            XmlTypeCode.NonNegativeInteger => "string",
            XmlTypeCode.NonPositiveInteger => "string",
            XmlTypeCode.Notation => "string",
            XmlTypeCode.QName => "System.Xml.XmlQualifiedName",
            XmlTypeCode.Duration => "string",
            XmlTypeCode.UnsignedByte => "byte",
            XmlTypeCode.UnsignedInt => "uint",
            XmlTypeCode.UnsignedLong => "ulong",
            XmlTypeCode.UnsignedShort => "ushort",
            _ => throw new NotImplementedException($"{st.Datatype?.TypeCode} is not implemented")
        };
    }

    private string? GetSummary(XmlSchemaElement? element)
    {
        if (element is null) return null;
        var items = element.Annotation?.Items ?? GetElementByName(element.QualifiedName)?.Annotation?.Items;
        if (items is null) return null;

        var sb = new StringBuilder();
        foreach (var item in items)
        {
            if (item is XmlSchemaDocumentation doc && doc.Markup is not null)
            {
                foreach (var markup in doc.Markup)
                {
                    sb.Append(markup?.InnerText);
                }
            }
        }
        return sb.ToString();
    }

    public XmlSchemaElement? GetElementByName(XmlQualifiedName? name)
    {
        if (name is not null)
        {
            foreach (var schemaObj in _schemaSet.Schemas())
            {
                if (schemaObj is XmlSchema schema)
                {
                    foreach (var elementObj in schema.Elements.Values)
                    {
                        if (elementObj is XmlSchemaElement element)
                        {
                            if (element.QualifiedName == name)
                            {
                                return element;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
    public XmlSchemaElement? GetElementByName(string? name)
    {
        if (name is not null)
        {
            foreach (var schemaObj in _schemaSet.Schemas())
            {
                if (schemaObj is XmlSchema schema)
                {
                    foreach (var elementObj in schema.Elements.Values)
                    {
                        if (elementObj is XmlSchemaElement element)
                        {
                            if (element.Name == name)
                            {
                                return element;
                            }
                        }
                    }
                }
            }
        }
        return null;
    }
}
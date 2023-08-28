namespace XsdLib.Utils;

using System.Xml.Schema;

public static class XmlTypeCodeExtentions
{
    public static string? ToDataType(XmlTypeCode? xmlTypeCode)
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

    public static string ToClrType(XmlTypeCode? typeCode)
        => typeCode switch
        {
            //See: https://learn.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlelementattribute.datatype?view=net-7.0#system-xml-serialization-xmlelementattribute-datatype
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
            _ => throw new NotImplementedException($"{typeCode} is not implemented")
        };

    /// <summary>
    /// Xml Integer types has unlimited range and must be treated as strings.
    /// Some xsds defines range restrictions that allows them to be treated as int or long instead
    /// </summary>
    public static XmlTypeCode? GetTypeCodeFromSimpleType(XmlSchemaSimpleType st)
        => st.Datatype?.TypeCode switch
        {
            XmlTypeCode.Integer => IntLongOrInteger(st),
            XmlTypeCode.PositiveInteger => IntLongOrInteger(st),
            XmlTypeCode.NonNegativeInteger => IntLongOrInteger(st),
            XmlTypeCode.NegativeInteger => IntLongOrInteger(st),
            XmlTypeCode.NonPositiveInteger => IntLongOrInteger(st),
            _ => st.Datatype?.TypeCode
        };

    public static XmlTypeCode IntLongOrInteger(XmlSchemaSimpleType st)
    {
        if (st.Content is XmlSchemaSimpleTypeRestriction restrictions)
        {
            var maxString = restrictions.Facets.OfType<XmlSchemaMaxExclusiveFacet>().FirstOrDefault()?.Value ??
                            restrictions.Facets.OfType<XmlSchemaMaxInclusiveFacet>().FirstOrDefault()?.Value;
            var minString = restrictions.Facets.OfType<XmlSchemaMinExclusiveFacet>().FirstOrDefault()?.Value ??
                            restrictions.Facets.OfType<XmlSchemaMinInclusiveFacet>().FirstOrDefault()?.Value;
            if (int.TryParse(maxString, out var _) && int.TryParse(minString, out var _))
            {
                return XmlTypeCode.Int;
            }
            if (long.TryParse(maxString, out var _) && long.TryParse(minString, out var _))
            {
                return XmlTypeCode.Long;
            }

        }
        return st.Datatype?.TypeCode ?? XmlTypeCode.None;
    }

}
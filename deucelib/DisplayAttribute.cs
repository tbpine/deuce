namespace deuce;

/// <summary>
/// Display this property on a web page
/// </summary>
public class DisplayAttribute : Attribute
{
    private readonly string _label;
    private readonly string? _format;
    private readonly Type?  _lookupType;

    public string Label { get=> _label; }
    public string? Format { get=> _format; }
    public Type? LookupType { get=> _lookupType; }

    /// <summary>
    /// Construct a display attribute with values
    /// </summary>
    /// <param name="label">Text to display</param>
    /// <param name="format">ToString format (if any)</param>
    /// <param name="lookup">True to lookup the property value (type dependant)</param>
    public DisplayAttribute(string label, string? format, Type? lookupType = null)
    {
        _label = label;
        _format = format;
        _lookupType = lookupType;
    }
}
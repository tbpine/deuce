
/// <summary>
/// Defines type look ups for constants 
/// in this application
/// </summary>
public interface ILookup
{
    Task<string?> GetLabel(object value, Type lookupType);
}
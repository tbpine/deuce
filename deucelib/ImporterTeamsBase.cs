
namespace deuce;
/// <summary>
/// Base class for importers. This class is not intended to be used directly, 
/// but rather as a base class for specific importers.
/// </summary>
public class ImporterTeamsBase : IImporterTeams
{
    public virtual List<Team> Load(Stream stream)
    {
        throw new NotImplementedException();
    }


}

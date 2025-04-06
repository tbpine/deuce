namespace deuce;
/// <summary>
/// Defines player/team importers for different file formats.
/// </summary>
public interface IImporterTeams
{
    List<Team> Load(Stream stream);
    
};
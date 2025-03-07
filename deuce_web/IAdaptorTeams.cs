using deuce;

/// <summary>
/// Creating teams 
/// </summary>
public interface IAdaptorTeams
{
    //From form values
    List<Team> Convert(IFormCollection form, Organization organization);
}
using deuce;

/// <summary>
/// Creating teams 
/// </summary>
public interface IFormReaderPlayers
{
    //From form values
    List<Team> Parse(IFormCollection form,  Tournament tournament);

}
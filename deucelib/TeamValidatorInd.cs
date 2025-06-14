namespace  deuce;

/// <summary>
/// For entry type "individual"
/// </summary>
public class TeamValidatorInd : TeamValidatorBase
{
    public override ResultTeamAction Check(List<Team> teams, Tournament tournament)
    {
        //Pass always
        return new ResultTeamAction(RetCodeTeamAction.Success, "");
    }
}
using deuce;
using System.Diagnostics;
using System.Text.RegularExpressions;


/// <summary>
/// Read form submittion for scoring
/// </summary>
class FormReaderScoring
{

    /// <summary>
    /// 
    /// </summary>
    public FormReaderScoring()
    {

    }

    /// <summary>
    /// Do the convertion of form values to
    /// teams
    /// </summary>
    /// <param name="form">Form reference</param>
    /// <param name="tournament">Tournament where player is registered</param>
    /// <param name="round">Round in the tournament</param>
    /// <returns>List of teams</returns>
    public List<Score> Parse(IFormCollection form, Tournament tournament, int roundIdx)
    {

        //Need schedule to get the matches
        //and permutaions
        if (tournament.Schedule is null) return new();

        List<deuce.Match> matches = new();

        foreach (var kp in form)
        {

            //Path format 

            //Ignore the action value
            if (kp.Key == "action") continue;

            var m = Regex.Match(kp.Key, @"^(home|away)_perm_(\d+)_match_(\d+)_set_(\d+)");

            if (m.Success)
            {
                string strHomeAway = m.Groups[1].Value;
                string strPermId = m.Groups[2].Value;
                string strMatchId = m.Groups[3].Value;
                string strSetId = m.Groups[4].Value;

                if (int.TryParse(strPermId, out int permId) &&
                    int.TryParse(strMatchId, out int matchId) &&
                    int.TryParse(strSetId, out int setId))
                {
                    var round = tournament.Schedule.GetRoundAtIndex(roundIdx);
                    var perm = round.Permutations.First(e => e.Id == permId);
                    deuce.Match match = perm.Matches.First(e => e.Id == matchId);
                    //Need a score parser
                    matches.Add(match);
                    
                }
            }
        }

        return matches;
    }


}
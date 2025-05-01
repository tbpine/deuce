using deuce;
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
    public List<Score> Parse(IFormCollection form,  int roundIdx, int tournamentId)
    {

        //Make a list of Score to return
        List<deuce.Score> scores = new();

        foreach (var kp in form)
        {

            //Path format home|away_perm_1_match_1_set_1=0

            //Ignore the action value
            if (kp.Key == "action") continue;

            var m = Regex.Match(kp.Key, @"^(home|away)_perm_(\d+)_match_(\d+)_set_(\d+)");

            if (m.Success)
            {
                string strHomeAway = m.Groups[1].Value;
                string strPermId = m.Groups[2].Value;
                string strMatchId = m.Groups[3].Value;
                string strSetId = m.Groups[4].Value;

                string? strScore = kp.Value.FirstOrDefault();
                int teamScore = int.TryParse(strScore, out teamScore) ? teamScore : 0;

                if (int.TryParse(strPermId, out int permId) &&
                    int.TryParse(strMatchId, out int matchId) &&
                    int.TryParse(strSetId, out int setId) &&
                    !string.IsNullOrEmpty(strScore))
                {

                    //Check if the score exists

                    var existingScore = scores.FirstOrDefault(e => e.Permutation == permId
                    && e.Match == matchId && e.Tournament == tournamentId);

                    if (existingScore is not null)
                    {
                        existingScore.Home = strHomeAway == "home" ? teamScore : existingScore.Home;
                        existingScore.Away = strHomeAway == "away" ? teamScore : existingScore.Away;
                        
                    }
                    else
                    {
                        Score score = new Score()
                        {
                            Id = 0,
                            Tournament = tournamentId,
                            Round = roundIdx,
                            Permutation = permId,
                            Match =matchId,
                            Home = strHomeAway == "home" ? teamScore : 0,
                            Away = strHomeAway == "away" ? teamScore : 0,
                            Set = setId
                        };
                        scores.Add(score);

                    }

                }
            }
        }

        return scores;
    }


}
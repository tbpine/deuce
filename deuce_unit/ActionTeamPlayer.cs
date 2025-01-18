using System.Net;

class ActionTeamPlayer
{
    public static void Clear(int tournamentId, bool clearAllPlayers = false)
    {
        string sql1 = $"DELETE FROM `team` where `tournament` = {tournamentId}";
        DirectSQL.Run(sql1);
        sql1 = $"DELETE FROM `team_player` where `tournament` = {tournamentId}";
        DirectSQL.Run(sql1);

        if (clearAllPlayers)
        {
            sql1 = $"DELETE FROM `player` WHERE organization = 1;";
            DirectSQL.Run(sql1);
        }
    }
}
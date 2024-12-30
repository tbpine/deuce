using System.Data.Common;
using iText.Forms.Fields.Merging;

namespace deuce.lib;

public class FactoryCreateDbRepo
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dbconn"></param>
    /// <param name="references"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Type doesn't have a dbrepo</exception>
    public static IDbRepo<T>? Create<T>(DbConnection dbconn, params object[] references) where T : new()
    {
        T obj = new();

        if (obj.GetType() == typeof(TournamentType)) return new DbRepoTournamentType(dbconn) as IDbRepo<T>;
        else if (obj.GetType() == typeof(Match)) return new DbRepoMatch(dbconn) as IDbRepo<T>;
        else if (obj.GetType() == typeof(Tournament)) return new DbRepoTournament(dbconn, references) as IDbRepo<T>;
        else if (obj.GetType() == typeof(Player)) return new DbRepoPlayer(dbconn, references) as IDbRepo<T>;
        else if (obj.GetType() == typeof(Team)) return new DbRepoTeam(dbconn) as IDbRepo<T>;
        else if (obj.GetType() == typeof(RecordSchedule)) return new DbRepoRecordSchedule(dbconn, references) as IDbRepo<T>;
        

        throw new ArgumentException("No DbRepo class for the specified type");
    }
}
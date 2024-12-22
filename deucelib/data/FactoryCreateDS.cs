using System.Data.Common;

namespace deuce.lib;

public class FactoryCreateDS
{
    public IDB<T>? Create<T>(DbConnection dbconn) where T : new()
    {
        T obj = new();

        if (obj.GetType() == typeof(TournamentType)) return new DSTournamentType(dbconn) as IDB<T>;

        return null;
    }
}
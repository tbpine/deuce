using System.Data.Common;

namespace deuce;

/// <summary>
/// The create factory pattern for classes
/// implementing IMatchFactory.
/// </summary>
public class FactoryDrawMaker
{
    /// <summary>
    /// Create a MatchFactory from it's type.
    /// </summary>
    /// <param name="t">Tournament details</param>
    /// <returns>A type defining IMatchFactory </returns>
    public IDrawMaker Create(Tournament t, IGameMaker gm)
    {

          switch (t.Type)
        {
            case 3: { return new DrawMakerKnockOutPlayoff(t, gm); }
            case 2: { return new DrawMakerKnockOut(t, gm); }
            case 4: { return new DrawMakerGroups(t, gm); }
            case 5: { return new DrawMakerSwiss(t, gm); }
            
        }

        return new DrawMakerRR(t, gm);
    }

    /// <summary>
    /// Create a scheduler for different tournament types.
    /// </summary>
    /// <param name="tType"> Tournament type</param>
    /// <param name="t">Tournament details</param>
    /// <param name="gm">Game maker</param>
    /// <returns>A scheduler for the specified tournament type</returns>
    public IDrawMaker Create(TournamentType tType, Tournament t, IGameMaker gm)
    {
          switch (tType.Id)
        {
            case 3: { return new DrawMakerKnockOutPlayoff(t, gm); }
            case 2: { return new DrawMakerKnockOut(t, gm); }
            case 4: { return new DrawMakerGroups(t, gm); }
            case 5: { return new DrawMakerSwiss(t, gm); }
            
        }

        return new DrawMakerRR(t, gm);

    }

    /// <summary>
    /// Create a DrawMaker with database connection support.
    /// </summary>
    /// <param name="dbConnection">Database connection object</param>
    /// <param name="t">Tournament details</param>
    /// <param name="gm">Game maker</param>
    /// <returns>A type implementing IDrawMaker</returns>
    public IDrawMaker Create(DbConnection dbConnection, Tournament t, IGameMaker gm)
    {
        // Store connection for potential future database operations
        // For now, create draw makers with existing pattern
        switch (t.Type)
        {
            case 3: { return new DrawMakerKnockOutPlayoff(t, gm); }
            case 2: { return new DrawMakerKnockOut(t, gm); }
            case 4: { return new DrawMakerGroups(t, gm); }
            case 5: { return new DrawMakerSwiss(dbConnection, t, gm); }
        }

        return new DrawMakerRR(t, gm);
    }

}
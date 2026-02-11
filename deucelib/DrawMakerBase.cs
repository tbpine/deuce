using System.Data.Common;

namespace deuce;

/// <summary>
/// Base class for creating tournament draws. Provides common functionality and defines the contract
/// for draw creation and handling changes during tournament progression.
/// </summary>
public class DrawMakerBase : IDrawMaker
{
    //------------------------------------
    // Internals
    //------------------------------------

    /// <summary>
    /// The tournament for which draws are being created.
    /// </summary>
    protected Tournament _tournament;

    /// <summary>
    /// List of teams participating in the tournament.
    /// </summary>
    protected List<Team> _teams = new();

    /// <summary>
    /// The size of groups used in round-robin or group-based tournament formats.
    /// </summary>
    protected int _groupSize = 4;

    /// <summary>
    /// Optional database connection for accessing score data.
    /// </summary>
    protected DbConnection? _dbConnection;


    /// <summary>
    /// Construct with dependencies.
    /// </summary>
    /// <param name="tournament">Tournament details</param>
    /// <param name="dbConnection">Optional database connection for score access</param>
    public DrawMakerBase(Tournament tournament, DbConnection? dbConnection = null)
    {
        _tournament = tournament;
        _dbConnection = dbConnection;
    }

    /// <summary>
    /// Creates a draw for the tournament based on the provided teams.
    /// This method must be implemented by derived classes.
    /// </summary>
    /// <returns>A Draw object representing the tournament schedule</returns>
    public virtual Draw Create()
    {
        throw new NotImplementedException("DrawMakerBase::Create not implemented");
    }

    /// <summary>
    /// Handles changes to the draw when scores are updated, allowing for dynamic
    /// tournament progression (e.g., advancing winners, updating brackets).
    /// This method must be implemented by derived classes.
    /// </summary>
    /// <param name="schedule">The current draw schedule</param>
    /// <param name="round">The current round number</param>
    /// <param name="previousRound">The previous round number</param>
    /// <param name="scores">List of scores from recent matches</param>
    public virtual void OnChange(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        throw new NotImplementedException("DrawMakerBase::OnChange not implemented");
    }

    /// <summary>
    /// Gets or sets the size of groups used in tournament formats that require grouping.
    /// Default value is 4.
    /// </summary>
    public int GroupSize { get { return _groupSize; } set { _groupSize = value; }}

    public virtual bool HasChanged(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        // If no database connection is available, compare with empty list (assume no changes)
        if (_dbConnection == null)
        {
            return scores.Count > 0;
        }

        try
        {
            // Get scores from database for this tournament and round
            var dbRepoScore = new DbRepoScore(_dbConnection);
            var filter = new Filter
            {
                TournamentId = _tournament.Id,
                Round = round
            };
            
            var dbScores = dbRepoScore.GetList(filter).Result;
            
            // If the count is different, something has changed
            if (dbScores.Count != scores.Count)
            {
                return true;
            }
            
            // Compare each score to see if any have changed
            foreach (var score in scores)
            {
                bool foundMatch = dbScores.Any(dbScore => 
                    dbScore.Tournament == score.Tournament &&
                    dbScore.Round == score.Round &&
                    dbScore.Permutation == score.Permutation &&
                    dbScore.Match == score.Match &&
                    dbScore.Set == score.Set &&
                    dbScore.Home == score.Home &&
                    dbScore.Away == score.Away
                );
                
                if (!foundMatch)
                {
                    return true; // Found a score that doesn't match database
                }
            }
            
            return false; // All scores match
        }
        catch (Exception)
        {
            // If there's any error accessing the database, assume changes have occurred
            return true;
        }
    }


}
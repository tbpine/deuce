using System.Runtime.InteropServices;

namespace deuce;

public class ScoringStats
{
    private int _matchesWon;
    private int _totalScore;

    private int _setsWon;
    private int _ranking;
    private Team _team = new Team();
    private List<Score> _scores = new();


    public int MatchesWon { get { return _matchesWon; } set { _matchesWon = value; } }
    public int TotalScore { get { return _totalScore; } set { _totalScore = value; } }
    public Team Team { get { return _team; } set { _team = value; } }
    public int SetsWon { get { return _setsWon; } set { _setsWon = value; } }
    
    public int Ranking { get { return _ranking; } set { _ranking = value; } }

    /// <summary>
    /// Default constructor for ScoringStats
    /// </summary>
    public ScoringStats()
    {

    }

    /// <summary>
    /// Constructor for ScoringStats that initializes with a team
    /// </summary>
    /// <param name="team"> The team to associate with the scoring stats</param>
    public ScoringStats(Team team)
    {
        _team = team;
    }

    /// <summary>
    /// Add a score to the scoring stats for a specific match.
    /// This method checks if the team is either the home or away team in the match,
    /// and updates the total score and matches won accordingly.
    /// </summary>
    /// <param name="match"> The match in which the score was achieved.</param>
    /// <param name="score"> The score to add.</param>
    public void AddScore(Score score, Match match)
    {
        bool isHomeTeam = match.Home.FirstOrDefault()?.Team?.Id == _team?.Id;
        _totalScore += isHomeTeam ? score.Home : score.Away;
        _scores.Add(score);
    }   

    public void Reset()
    {
        _matchesWon = 0;
        _totalScore = 0;
        _setsWon = 0;
        _scores.Clear();
    }   
}
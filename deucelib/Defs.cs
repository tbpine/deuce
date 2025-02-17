namespace deuce;

//Enumerations

/// <summary>
/// State of a tournament
/// </summary>
public enum TournamentStatus : uint
{
    Unknown = 0,
    New,
    Scheduled,

};



/// <summary>
/// State of a tournament
/// </summary>
public enum RetCodeScheduling : uint
{
    Unknown = 0,
    Success,
    Error,

};

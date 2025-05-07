namespace deuce;

//Enumerations

/// <summary>
/// State of a tournament
/// </summary>
public enum TournamentStatus : uint
{
    Unknown = 0,
    New,
    Start,

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


/// <summary>
/// State of a tournament
/// </summary>
public enum RetCodeTeamAction : uint
{
    Unknown = 0,
    Success,
    Error,
    Warning

};


/// <summary>
/// Entry types
/// </summary>
public enum EntryType : uint
{
    Unknown = 0,
    Team,
    Individual
};

/// <summary>
/// Account types
/// </summary>
public enum AccountType : uint
{
    Unknown = 0,
    Player,
    Organizer
};


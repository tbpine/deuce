namespace deuce;

/// <summary>
/// Used when constructing the schedule of a tournament.
/// </summary>
class StateBuilderDraw
{

    //Ensure to set initial value to negatives
    //for a state change on the first interation.
    
     public Permutation? Permutation { get; set; }
    public Round?  Round { get; set; }
    public Match? Match { get; set; }

    

    /// <summary>
    /// Empry constructor
    /// </summary>
    public StateBuilderDraw()
    {
        
    }
}
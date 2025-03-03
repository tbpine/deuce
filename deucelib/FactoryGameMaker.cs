namespace deuce;

/// <summary>
/// 
/// </summary>
public class FactoryGameMaker
{
    /// <summary>
    /// Creat a class that create games.
    /// </summary>
    /// <param name="sport">type of sport</param>
    /// <returns>IGameMaker implementer</returns>
    public IGameMaker Create(Sport? sport)
    {
        
        return new GameMakerTennis();
    }
}
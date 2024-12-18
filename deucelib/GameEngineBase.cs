using deuce.lib;

public class GameEngineBase 
{
    protected Tournament _tournament;
    
    public GameEngineBase(Tournament tournament)
    {
        _tournament = tournament;
    }

    public Dictionary<int,List<Game>> Run(List<Player> players)
    {
        FactoryGameEngine fac =new FactoryGameEngine();
        IGameEngine? ge = fac.Create(_tournament!);
        return ge?.Generate(players) ?? new(); 
    }

}
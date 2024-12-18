using deuce.lib;

public interface IGameEngine
{
    Dictionary<int,List<Game>> Generate(List<Player> players);
}
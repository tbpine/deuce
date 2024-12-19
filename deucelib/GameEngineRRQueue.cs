using System.Runtime.Versioning;

namespace deuce.lib; 

/// <summary>
/// Method use  two queues aligned
/// together. 
/// Every round, you move a queue 
/// in one direction to get a different
/// combination.
/// </summary>
class GameEngineRRQueue : GameEngineBase, IGameEngine
{
    
    public GameEngineRRQueue(Tournament t) : base(t)
    {

    }

    public Dictionary<int, List<Game>> Generate(List<Player> players)
    {
        return null;
    }
}
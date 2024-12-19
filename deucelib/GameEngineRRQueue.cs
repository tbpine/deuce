using System.Reflection.PortableExecutable;
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

    /// <summary>
    /// Construct with dependencies.
    /// </summary>
    /// <param name="t">Tournamnet object</param>
    /// <param name="players">List of players in the tournament</param>
    public GameEngineRRQueue(Tournament t, List<Player> players) : base(t)
    {
        _players = players;
    }

    public Dictionary<int, List<Game>> Generate(List<Player> players)
    {
        //Index players
        List<int> left = new();
        List<int> right = new();
        for (int i = 1; i <= _players.Count; i++)
        {
            _players[i].Index = i;
            left.Add(i);
            right.Add(i); 
        }
        //line up
        right.Reverse();

        //Rounds
        for(int i = 0; i < (_players.Count-1); i++)
        {
            for(int k=0; k<left.Count;k++)
            {
                int lhs = left[k];
                int rhs = right[k];
                RaiseGameCreatedEvent(i, lhs, rhs);

            }

            //Shift player;
            
        }
        

        return null;
    }
}
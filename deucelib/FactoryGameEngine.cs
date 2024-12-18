using deuce.lib;

public class FactoryGameEngine
{
    public IGameEngine? Create(Tournament t)
    {
        switch (t.Type?.Id??0)
        {
            case 1: { return new GameEngineRoundRobin(t); }
        }

        return null;
    }
}
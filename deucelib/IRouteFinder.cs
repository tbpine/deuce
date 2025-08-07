namespace deuce;

public interface IRouteFinder
{
    Match? FindDestMatch(Draw draw, Match start);
}

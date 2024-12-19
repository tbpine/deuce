class Player
{
    public List<Player> Opponents = new();

    public string Name { get; set; } = "";

    public override string ToString()
    {
        return Name; 
    }
}
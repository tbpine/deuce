namespace deuce;

public class TournamentVenue
{
    private int _id;
    private Tournament? _tournament;
    private string _street = "";
    private string _suburb = "";
    private string _state = "";
    private int _postCode;
    private string _country = "";
    public int Id { get => _id; set => _id = value; }
    public string Street { get => _street; set => _street = value; }
    public string Suburb { get => _suburb; set => _suburb = value; }
    public string State { get => _state; set => _state = value; }
    public int PostCode { get => _postCode; set => _postCode = value; }
    public string Country { get => _country; set => _country = value; }
    public Tournament? Tournament { get=>_tournament; set =>_tournament = value; }  

    /// <summary>
    /// Construct with specify values
    /// </summary>
    /// <param name="street">Street location</param>
    /// <param name="suburb">Suburb</param>
    /// <param name="state">State</param>
    /// <param name="postCode">Post code</param>
    /// <param name="country">Country</param>
    public TournamentVenue(string street, string suburb, string state, int postCode, string country)
    {
        _street = street;
        _suburb = suburb;
        _state = state;
        _postCode = postCode;
        _country = country;
    }

    /// <summary>
    /// Default constructor
    /// </summary>

    public TournamentVenue()
    {
    }

}
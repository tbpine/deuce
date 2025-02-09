using deuce;
using System.Data.Common;
using System.Diagnostics;
/// <summary>
/// Load tournament objects from the database
/// </summary>
public class DBTournamentGateway : ITournamentGateway
{

    private readonly IServiceProvider _serviceProvider;
    private readonly SessionProxy _sessionProxy;
    private readonly IConfiguration _config;

    public DBTournamentGateway(SessionProxy sessionProxy, IServiceProvider sp, IConfiguration config)
    {
        _sessionProxy = sessionProxy;
        _serviceProvider = sp;
        _config = config;
    }

    /// <summary>
    /// Load  tournament details from the browser session.
    /// </summary>
    /// <param name="dbconn">Database connectoin</param>
    /// <returns>Tournmanet instance</returns>
    public async Task<Tournament?> GetCurrentTournament()
    {

        var scope = _serviceProvider.CreateScope();
        //Calls the IAsyncDisposable interface
        await using var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        if (dbconn is  null)  return null;
        
        dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
        await dbconn.OpenAsync();

        //Check if there's a tournament saved
        int tourId = _sessionProxy?.TournamentId ?? 0;
        if (tourId < 1) return null;
        
        Debug.Print($"Select tournament #{tourId}");
        
        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1, Name = "" };

        //Load a the current tournament from the database
        //Create a scoped db connection.
        //Use a DBRepo to build the object
        DbRepoTournament dbRepoTour = new DbRepoTournament(dbconn, thisOrg);
        
        //Select the tournment.Returns in the first element
        //Create filter
        Filter tourFilter = new Filter() { TournamentId = tourId };
        List<Tournament> listOfTour = await dbRepoTour.GetList(tourFilter);
        //Close if it was created.

        

        return listOfTour.FirstOrDefault();


    }
}
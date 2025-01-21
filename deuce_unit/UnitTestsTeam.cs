namespace deuce_unit;
using deuce;
using MySql.Data.MySqlClient;
using System.Diagnostics;

[TestClass]
public class UnitTestsTeam
{
    public UnitTestsTeam() { }

    [TestMethod]
    public async Task sync_insert_teams()
    {
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id);

        //Make teams
        Team team1 = new Team() { Id = 0, Index = 0, Label = "test_team1" };
        Team team2 = new Team() { Id = 0, Index = 0, Label = "test_team2" };
        //Add players

        team1.AddPlayer(new Player() { Id = 0, First = "test_player1", Last = "", Index = 0, Ranking = 0d });
        team2.AddPlayer(new Player() { Id = 0, First = "test_player2", Last = "", Index = 0, Ranking = 0d });

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        List<Team> teams = [team1, team2];
        
        //Action
        await dbRepoTeam.Sync(teams);

        DbRepoRecordTeamPlayer dbRepoTeamPlayer= new(conn);
        var records = await dbRepoTeamPlayer.GetList(new Filter(){TournamentId = tournament.Id});

        //Assert
        Assert.IsTrue(records.Count == 2, "Incorrect number of players");
        foreach(var record in records)
            Assert.IsTrue(record.TeamId > 0 && record.PlayerId > 0 , record.FirstName + " " + record.LastName + "is incorrect");


    }
    
    [TestMethod]
    public async Task sync_update_team_detail()
    {
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id);

        //Make teams
        Team team1 = new Team() { Id = 0, Index = 0, Label = "test_team1" };
        Team team2 = new Team() { Id = 0, Index = 0, Label = "test_team2" };
        //Add players

        team1.AddPlayer(new Player() { Id = 0, First = "test_player1", Last = "", Index = 0, Ranking = 0d });
        team2.AddPlayer(new Player() { Id = 0, First = "test_player2", Last = "", Index = 0, Ranking = 0d });

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        List<Team> teams = [team1, team2];
        //Action
        await dbRepoTeam.Sync(teams);

        //Update 
        team1.Label = "team1 changed";

        await dbRepoTeam.Sync(teams);

        //Assert
        DbRepoRecordTeamPlayer dbRepoRTeamPlayer= new(conn);
        var records = await dbRepoRTeamPlayer.GetList(new Filter(){TournamentId = tournament.Id});
        var recTeam = records.Find(e=>e.TeamId == team1.Id);

        //Assert
        Assert.IsTrue(records.Count == 2, "Incorrect team size/players");
        Assert.IsNotNull(recTeam, "Team doesn't exist");
        Assert.IsTrue(recTeam.Team == "team1 changed", "Team label was not updated");



    }


    [TestMethod]
    public async Task sync_add_a_team()
    {
         
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id);

        //Make teams
        Team team1 = new Team() { Id = 0, Index = 0, Label = "test_team1" };
        Team team2 = new Team() { Id = 0, Index = 0, Label = "test_team2" };
        //Add players

        team1.AddPlayer(new Player() { Id = 0, First = "test_player1", Last = "", Index = 0, Ranking = 0d });
        team2.AddPlayer(new Player() { Id = 0, First = "test_player2", Last = "", Index = 0, Ranking = 0d });

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        List<Team> teams = [team1, team2];
        //Action
        await dbRepoTeam.Sync(teams);

        //Update 
        Team team3 = new Team() { Id = 0, Index = 0, Label = "test_team3" };
        team3.AddPlayer(new Player() { Id = 0, First = "test_player3", Last = "", Index = 0, Ranking = 0d });
        teams.Add(team3);

        await dbRepoTeam.Sync(teams);

        //Assert
        DbRepoRecordTeamPlayer dbRepoRTeamPlayer= new(conn);
        var records = await dbRepoRTeamPlayer.GetList(new Filter(){TournamentId = tournament.Id});
        var recTeam = records.Find(e=>e.TeamId == team3.Id);

        //Assert
        Assert.IsTrue(records.Count == 3, "Incorrect team size/players");
        Assert.IsNotNull(recTeam, "Team 3 wasn't added ");
        


    }

    
    [TestMethod]
    public async Task sync_remove_a_team()
    {
         
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id);

        //Make teams
        Team team1 = new Team() { Id = 0, Index = 0, Label = "test_team1" };
        Team team2 = new Team() { Id = 0, Index = 0, Label = "test_team2" };
        //Add players

        team1.AddPlayer(new Player() { Id = 0, First = "test_player1", Last = "", Index = 0, Ranking = 0d });
        team2.AddPlayer(new Player() { Id = 0, First = "test_player2", Last = "", Index = 0, Ranking = 0d });

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        List<Team> teams = [team1, team2];
        //Action
        await dbRepoTeam.Sync(teams);

        //Update 
        teams.Remove(team2);
        await dbRepoTeam.Sync(teams);

        //Assert
        DbRepoRecordTeamPlayer dbRepoRTeamPlayer= new(conn);
        var records = await dbRepoRTeamPlayer.GetList(new Filter(){TournamentId = tournament.Id});
        var recTeam = records.Find(e=>e.TeamId == team2.Id);
        //Assert
        Assert.IsTrue(records.Count == 1, "Incorrect team size/players");
        Assert.IsNull(recTeam, "Team 2 wasn't removed ");
        


    }

    [TestMethod]
    public async Task sync_change_a_player()
    {
  
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id);

        //Make teams
        Team team1 = new Team() { Id = 0, Index = 0, Label = "test_team1" };
        Team team2 = new Team() { Id = 0, Index = 0, Label = "test_team2" };
        //Add players

        team1.AddPlayer(new Player() { Id = 0, First = "test_player1", Last = "", Index = 0, Ranking = 0d });
        team2.AddPlayer(new Player() { Id = 0, First = "test_player2", Last = "", Index = 0, Ranking = 0d });

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        List<Team> teams = [team1, team2];
        //Action
        await dbRepoTeam.Sync(teams);

        //Update 
        //Add a new player
        string sql = $"insert into player values (1,'new player','',1,0.0,now(), now())";
        DirectSQL.Run(sql);
        team1.RemovePlayer(team1.Players.First());
        Player newPlayer = new Player() { Id  = 1, First = "new", Last="player", Index = 0};

        team1.AddPlayer(newPlayer);

        await dbRepoTeam.Sync(teams);

        //Assert
        DbRepoRecordTeamPlayer dbRepoRTeamPlayer= new(conn);
        var records = await dbRepoRTeamPlayer.GetList(new Filter(){TournamentId = tournament.Id});
        var recTeam = records.Find(e=>e.TeamId == team1.Id && e.PlayerId == 1);
        //Assert
        Assert.IsTrue(records.Count == 2, "Incorrect team size/players");
        Assert.IsNotNull(recTeam, "Team 1 player wasn't changed removed ");
        
    }

    [TestMethod]
    [DataRow(2,1)]
    [DataRow(2,2)]
    [DataRow(8,2)]
    public async Task sync_insert_teams_n_team_size(int noTeams, int teamSize)
    {
    //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id, true);
        
        //Collection of teams.
        List<Team> teams = new List<Team>();

        //Make teams of players 
        //given test parameters
        for(int i = 0; i < noTeams; i++)
        {
        //Make teams
            Team team = new Team() { Id = 0, Index = i, Label = $"test_team_{i+1}" };
            for(int j = 0; j < teamSize; j++)  
            {
                Player player = new Player() { Id = 0, First = "test", Last = $"player_{i*teamSize + j}", Index = j, Ranking=0d };
                team.AddPlayer(player);
                teams.Add(team);
            }
        
        }


        //Synchronoize (save) to the database.

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        
        //Action
        await dbRepoTeam.Sync(teams);

        //Load back the saved team
        DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer= new DbRepoRecordTeamPlayer(conn);
        Filter filterTeamPlayer = new() { ClubId = organization.Id, TournamentId = tournament.Id };
        //Load the test tournament
        var records = await dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
        //Print out teams
        for(int i = 0; i < records.Count; i++)
            Debug.WriteLine(records[i].ToString());

        //No changes move to asserts
        //Assert
        Assert.IsTrue(records.Count == noTeams * teamSize, "Incorrect team size/players");
        
    }

    
    [TestMethod]
    [DataRow(2,1)]
    public async Task sync_add_players_to_teams(int noTeams, int teamSize)
    {
        //Assigns
        Organization organization = new Organization() { Id = 1 };
        Tournament tournament = new() { Id = 1 };
        AssignTeamPlayer.Clear(tournament.Id, true);
        
        //Collection of teams.
        List<Team> teams = new List<Team>();

        //Make teams of players 
        //given test parameters
        for(int i = 0; i < noTeams; i++)
        {
        //Make teams
            Team team = new Team() { Id = 0, Index = i, Label = $"test_team_{i+1}" };
            for(int j = 0; j < teamSize; j++)  
            {
                Player player = new Player() { Id = 0, First = "test", Last = $"player_{i*teamSize + j}", Index = j, Ranking=0d };
                team.AddPlayer(player);
                teams.Add(team);
            }
        
        }


        //Synchronoize (save) to the database.

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        DbRepoTeam dbRepoTeam = new DbRepoTeam(conn, organization, tournament.Id);
        
        //Action
        await dbRepoTeam.Sync(teams);
        
        //Action: make changes here
        //Add players to an existing team

        //Load back the saved team
        DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer= new DbRepoRecordTeamPlayer(conn);
        //Load the test tournament
        Filter filterTeamPlayer = new Filter() { ClubId = organization.Id, TournamentId = tournament.Id };
        var records = await dbRepoRecordTeamPlayer.GetList(filterTeamPlayer);
        //Print out teams
        for(int i = 0; i < records.Count; i++)
            Debug.WriteLine(records[i].ToString());

        //No changes move to asserts
        //Assert
        Assert.IsTrue(records.Count == noTeams * teamSize, "Incorrect team size/players");
        
    }
}
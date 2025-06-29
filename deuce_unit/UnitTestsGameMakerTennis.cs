namespace deuce_unit;

using deuce;

using Moq;

[TestClass]
public class UnitTestsGameMakerTennis
{
    [TestMethod]
    public void get_winners_1_set_1_match()
    {
        //Assign
        //Make random players, with randome first, middle, last names
        var player1 = new Player { Id = 1, First = "John", Last = "Doe" };
        var player2 = new Player { Id = 2, First = "Jane", Last = "Smith" };

        //Make random teams
        var team1 = new Team { Id = 1, Label = "Team A" };
        var team2 = new Team { Id = 2, Label = "Team B" };
        //Add players to teams
        team1.AddPlayer(player1);
        team2.AddPlayer(player2);

        //create a random match
        var match = new deuce.Match { Id = 1, Round = 1 }; match.AddPairing(player1, player2);
        //Create a random permutation
        var permutation = new Permutation(1, new Team[] { team1, team2 });
        permutation.AddMatch(match);
        //Creata a score for the match
        var score = new Score { Id = 1, Tournament = 1, Round = 1, Set = 1, Match = match.Id, Away = 6, Home = 4, Permutation = permutation.Id };
        //Create a list of scores
        var scores = new List<Score> { score };
        //Mock the GameMakerTennis
        var fac = new FactoryGameMaker();
        var gameMaker = fac.Create(new Sport(1, "Tennis", "Tennis", "Tennis", ""));
        //Act

        List<Team> winners = gameMaker.GetWinners(permutation, scores);
        //Assert

        Assert.AreEqual(team2.Equals(winners[0]), true, "Team 2 should be the winner");


    }

    [TestMethod]
    public void get_winners_n_sets_1_match()
    {
        //Assign
        //Make random players, with randome first, middle, last names
        var player1 = new Player { Id = 1, First = "John", Last = "Doe" };
        var player2 = new Player { Id = 2, First = "Jane", Last = "Smith" };

        //Make random teams
        var team1 = new Team { Id = 1, Label = "Team A" };
        var team2 = new Team { Id = 2, Label = "Team B" };
        //Add players to teams
        team1.AddPlayer(player1);
        team2.AddPlayer(player2);

        //create a random match
        var match = new deuce.Match { Id = 1, Round = 1 }; match.AddPairing(player1, player2);
        //Create a random permutation
        var permutation = new Permutation(1, new Team[] { team1, team2 });
        permutation.AddMatch(match);
        //Creata a score for the match
        var score1 = new Score { Id = 1, Tournament = 1, Round = 1, Set = 1, Match = match.Id, Away = 6, Home = 4, Permutation = permutation.Id };
        var score2 = new Score { Id = 1, Tournament = 1, Round = 1, Set = 1, Match = match.Id, Away = 6, Home = 4, Permutation = permutation.Id };
        //Create a list of scores
        var scores = new List<Score> { score1, score2 };
        //Mock the GameMakerTennis
        var fac = new FactoryGameMaker();
        var gameMaker = fac.Create(new Sport(1, "Tennis", "Tennis", "Tennis", ""));
        //Act

        List<Team> winners = gameMaker.GetWinners(permutation, scores);
        //Assert

        Assert.AreEqual(team2.Equals(winners[0]), true, "Team 2 should be the winner");


    }


    [TestMethod]
    [DataRow(1, 1, 6, 1, 6)]
    [DataRow(2, 6, 1, 6, 1)]
    [DataRow(3, 1, 6, 6, 2)]
    [DataRow(4, 6, 1, 2, 6)]
    [DataRow(5, 6, 4, 4, 6)]
    public void get_winners_1_sets_n_match(int c, int s1, int s2, int s3, int s4)
    {
        //Assign
        //Make random players, with randome first, middle, last names
        var player1 = new Player { Id = 1, First = "John", Last = "Doe" };
        var player2 = new Player { Id = 2, First = "Jane", Last = "Smith" };

        //Make random teams
        var team1 = new Team { Id = 1, Label = "Team A" };
        var team2 = new Team { Id = 2, Label = "Team B" };
        //Add players to teams
        team1.AddPlayer(player1);
        team2.AddPlayer(player2);

        //create a random match
        var match1 = new deuce.Match { Id = 1, Round = 1 }; match1.AddPairing(player1, player2);
        var match2 = new deuce.Match { Id = 2, Round = 1 }; match2.AddPairing(player1, player2);
        //Create a random permutation
        var permutation = new Permutation(1, new Team[] { team1, team2 });
        permutation.AddMatch(match1);
        permutation.AddMatch(match2);
        //Creata a score for the match
        var score1 = new Score { Id = 1, Tournament = 1, Round = 1, Set = 1, Match = match1.Id, Away = s2, Home = s1, Permutation = permutation.Id };
        var score2 = new Score { Id = 1, Tournament = 1, Round = 1, Set = 1, Match = match2.Id, Away = s4, Home = s3, Permutation = permutation.Id };

        //Create a list of scores
        var scores = new List<Score> { score1, score2 };
        //Mock the GameMakerTennis
        var fac = new FactoryGameMaker();
        var gameMaker = fac.Create(new Sport(1, "Tennis", "Tennis", "Tennis", ""));
        //Act

        List<Team> winners = gameMaker.GetWinners(permutation, scores);
        //Assert
        if (c == 1)
            Assert.AreEqual(team2.Equals(winners[0]), true, "Team 2 should be the winner");
        else if (c == 2)
            Assert.AreEqual(team1.Equals(winners[0]), true, "Team 1 should be the winner");
        else if (c == 3)
            Assert.AreEqual(team2.Equals(winners[0]), true, "Team 2 should be the winner");
        else if (c == 4)
            Assert.AreEqual(team1.Equals(winners[0]), true, "Team 1 should be the winner");
        else if (c == 5)
        {
            Assert.AreEqual(team1.Equals(winners[0]), true, "Team 2 should be the winner");
            Assert.AreEqual(team2.Equals(winners[1]), true, "Team 1 should be the winner");
        }



    }

}

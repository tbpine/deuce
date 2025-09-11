namespace deuce;

class OrganizerGroupDefault : IOrganizerGroup
{
    public virtual void Assign(Tournament tournament,List<Team> teams, int groupSize)
    {
        
        //Assign groups top to bottom

         //Create groups
        int noGroups = (int)Math.Ceiling((double)teams.Count / groupSize);
        for (int i = 0; i < noGroups; i++)
        {
            Group group = new Group();
            //Start from A, B, C, ...
            group.Label = ((char)('A' + i)).ToString();
            //Split teams into groups
            for (int j = 0; j < groupSize; j++)
            {
                //Find the team index for this group position
                int teamIndex = i * groupSize + j;
                if (teamIndex < teams.Count) group.AddTeam(teams[teamIndex]);
            }
            //Add it to the tournament
            tournament.AddGroup(group);
        }

    }
}

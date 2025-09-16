using System.Threading.Tasks.Dataflow;

namespace deuce;

/// <summary>
/// Default implementation of IOrganizerGroup for organizing teams into groups.
/// This class assigns teams to groups in a tournament based on the specified group size.
/// </summary>
class OrganizerGroupDefault : IOrganizerGroup
{
    /// <summary>
    /// Assigns teams to groups in the tournament.
    /// Teams are distributed evenly across groups, starting from group A, B, C, etc.
    /// </summary>
    /// <param name="tournament">The tournament object to which groups will be added.</param>
    /// <param name="teams">The list of teams to be assigned to groups.</param>
    public virtual void Assign(Tournament tournament,List<Team> teams )
    {
        
        //Assign groups top to bottom
        int groupSize = tournament.GroupSize ?? 4;
        if (groupSize < 2) groupSize = 2;   

         //Create groups
        int noGroups = (int)Math.Ceiling((double)teams.Count / groupSize);
        for (int i = 0; i < noGroups; i++)
        {
            Group group = new Group();
            group.Size = groupSize;
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

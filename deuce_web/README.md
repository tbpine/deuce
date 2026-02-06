# TODOS
- [] Create tournament
    - [X] moving back and next creates multiple teams
    - [X] Format player page
    - [X] Team definition page
- [X] Fix the scoring sheet
- [X] Load teams
- [X] Disband team
    - [X] Show disbanded players in the list

- [X] fix issues with set entry sizing
- [X] Add bye teams for odd number of teams in a round robin in the players page.
- [X] Display scores for sets with bye teams
- [] Add orgainzations (clubs)
- [X] enable score capturing of 12 large sets
- [X] CRUD on round scores 
    - [X] Include  round indexes as part of score input names.
- [X] Add scores to the pdf printout.
- [] Generally, push alot more logic to the lib
    - [] Make a score keeper class to handle scores
- [] Because data flow uses data submittion, have classes 
that read form values. Like now, but use the factory creation 
pattern.
- [] Most forms use action for different work flows. Research MVC, to see
if it can be used.
- [] Change strings in the database to ntext. Get all lables from the database.
- [] Editing matches, perms and sets after the tournament has started. Have to remove
existing scores. If the current date has passed the start date, warn user that scores will be removed.
- [] Move razor pages to MVC
    - [X] Tournament wizard. Have a controller for each page. Hope that a base controller works.
- [X] Ask copilot to convert the remaining tournament wizard pages to MVC.
    - [X] Players add team function on the players page.
    - [X] Wizard controller should hold the viewmodel instance and set back
    page properties.
- [X] Move member's area to MVC
    - [X] Derive Controllers in the member's area from MemberController
- [X] Wizard testing for each screen.
 - [X] Scoring controller
- [] Don't need seperate controllers for team and player formats. Use one controller
and represent the appropreite view. Makes navigation easier.
- [] Byes for knockout. 2^Ceil(log(n)) - n ,  where n = number of players
- [] concentrate on uniting of tournament strategies
- [X] ~~Add a boolean to the DBLocationConnection class to mark transctions~~
- [X] ~~FIX the schedule builder when there's empty players. Each match needs number of players ?~~
- [] Review tournament wizard for KO of entry types teams or individual
- [X] ~~Players list is readonly when the entry type is individual. Remember that indiviaula tournament are a special case of team size one.~~
- ~~[X] Make groups of 1 for individual tournament~~
- [X] Scoring for KO tournament type
- [X] Draws for KO tournament type
    -[X] Pagenation
- [X] RR ranking
# 22 Jan 2026
- [X] ~~ swiss format tornament. Draws , scores and prints. ~~
# 04 Feb 2026
- [] Get a list of players for a tournament once it's created.
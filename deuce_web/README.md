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
    - [] Tournament wizard. Have a controller for each page. Hope that a base controller works.

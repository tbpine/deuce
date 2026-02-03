---
applyTo: '**'
---
## Tournament format

- Induviduals are represented by a team with one member.

## Add a page to the tournament wizard
- For the MVP pattern, use a controller derived from the "WizardController" class.
- Prefix the controller name with the letter "T" (for Tournament), e.g., "TDetailController". 
- In the "Views" folder, create view in a subfolder named after the controller without the "Controller" suffix, e.g., for "TDetailController", create views in "Views/TDetail".
- All views for the wizard uses the "_LayoutTournamentWizard"
- All views uses the ViewModelTournamentWizard as the view model. Add properties for page specific data.
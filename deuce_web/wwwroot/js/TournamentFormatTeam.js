function showHideElement(name, cont, val)
{
  let node = document.getElementById(name);
  let selected = node.options[node.selectedIndex].value;
  if (selected == val)
  {
      document.getElementById(cont).classList.remove("invisible")
      document.getElementById(cont).classList.add("visible")
      document.getElementById(cont).classList.remove("zero-height")
  }
  else
  {
      document.getElementById(cont).classList.remove("visible")
      document.getElementById(cont).classList.add("invisible")
      document.getElementById(cont).classList.add("zero-height")

  }

}


document.addEventListener("DOMContentLoaded", function(){
    let node = document.getElementById("Games");

    if (node.options[node.selectedIndex].value == "99")
      showHideElement('Games','fixed_game_2','99')

    node = document.getElementById("TeamSize");
    if (node.options[node.selectedIndex].value == "99")
      showHideElement('TeamSize','team_size_cont','99')
  
    node = document.getElementById("NoSingles");

    if (node.options[node.selectedIndex].value == "99")
      showHideElement('NoSingles','no_singles_cont','99')
    
    node = document.getElementById("NoDoubles");
    
    if (node.options[node.selectedIndex].value == "99")
      showHideElement('NoDoubles','no_doubles_cont','99')
});

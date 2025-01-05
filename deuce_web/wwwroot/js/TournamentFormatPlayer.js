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
    let node = document.getElementById("GamesPerSet");

    if (node.options[node.selectedIndex].value == "99")
      showHideElement('GamesPerSet','fixed_game_2','99')

    node = document.getElementById("TeamSize");
    if (node.options[node.selectedIndex].value == "99")
      showHideElement('TeamSize','team_size_cont','99')
  

});

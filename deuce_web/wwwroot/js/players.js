//------------------------------------
//Add a team
//------------------------------------
function addTeam()
{
    //Make options for the players

    const jsonPlayers =  document.getElementById("listOfPlayers").textContent;
    var players = JSON.parse(jsonPlayers);
    var strPlayers = "<option value=''></option>";
    players.Players.forEach(e => {
        strPlayers += `<option value='${e.Id}'>${e.Name}</option>`;
    });

    const div = document.createElement("div");
    div.innerHTML = `<div class="d-sm-flex align-items-center">
                    <div class="d-inline-flex position-relative z-2 pt-1 pb-2 ps-2 p-sm-0 ms-2 ms-sm-0 me-sm-2">
                      <div class="form-check position-relative z-1 fs-lg m-0">
                        <input type="checkbox" class="form-check-input" checked>
                      </div>
                      <span class="position-absolute top-0 start-0 w-100 h-100 bg-body border rounded d-sm-none"></span>
                    </div>
                    <article class="card w-100 p-2">
                      <div class="d-sm-none" style="margin-top: -44px"></div>
                      <div class="row row-cols-2 row-cols-sm-2 g-0 ">
                        <div class="col-1">
                          <label for="TeamName" class="form-label p-2">Team Name:</label>
                        </div>
                        <div class="col-2">
                          <input id="TeamName" class="form-control p-2 w-75"/>
                        </div>
                        
                      </div>
                      <div class="row g-0 mb-2">
                        <a class="nav-link position-relative px-0 pe-sm-2 py-2 me-4 ms-2" href="#!">
                          <i class="fi-user-plus fs-base me-2"></i>
                          <span class="hover-effect-underline stretched-link d-none d-md-inline">Add Player</span>
                         </a>
                      </div>
                      <div class="row row-cols-*-3 g-0 mb-2">
                        <div class="col-5 me-2"><span>Registered</span></div>
                        <div class="col-5 me-2"><span>New Player</span></div>
                        <div class="col-1"></div>
                        <div class="col-5 me-2">
                          <select id='player_select1' class="form-select" data-select='{"searchEnabled": true}' aria-label="Select Player">
                            ${strPlayers}
                            </select>
                        </div>
                        <div class="col-5 me-2">
                          <input class="form-control" placeholder="New Player" aria-placeholder="New Player"/>
                        </div>
                        <div class="col-1">
                          <a class="nav-link position-relative px-0 pe-sm-2 py-2 me-4 ms-2">
                            <i class="fi-close-circle fs-base me-2"></i>
                          </a>
                        </div>
                      </div>

                    </article>
                  </div>
    `;

    document.getElementById("publishedSelection").appendChild(div);
    const psel = new Choices(document.getElementById("player_select1"));
}

//------------------------------------
//Hide the new player text box
//------------------------------------

function showHideNewPlayer(selectid, inputid)
{
  //Get the selected value
  const nodeSelect = document.getElementById(selectid);
  if (nodeSelect == null) return;

  var selectedValue = nodeSelect.value;

  let isNewPlayer =  selectedValue == "";

  const node = document.getElementById(inputid);
  if (node != null)
  {
    if (isNewPlayer)
    {
      node.classList.remove("invisible");
      node.classList.remove("zero_height");
    }
    else
    {
      node.classList.add("invisible");
      node.classList.add("zero height");
    }

  }
  
}

//------------------------------------
//Add a team by posting
//------------------------------------
function addTeamPost()
{
  document.getElementById("action").value = "add_team";
  document.getElementById("page_form").submit();
}
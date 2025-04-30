
/**
 * Updates the current round value and submits the main form.
 *
 * This function sets the value of the hidden input field with the ID "current_round"
 * to the specified round value and then submits the form with the ID "mainForm".
 *
 * @param {number|string} round - The round value to set in the "current_round" input field.
 */
function changeRound(round)
{
    let n1 = document.getElementById("current_round");
    n1.setAttribute("value", round);
    
    document.getElementById("mainForm").submit();

}

function toogleExpansion(name)
{
    let n1 = document.getElementById(name);
    let c = n1.getAttribute("class");

    if (c === "fi-plus position-relative top-50 start-0 translate-middle-y") n1.setAttribute("class", "fi-minus position-relative top-50 start-0 translate-middle-y");
    else n1.setAttribute("class", "fi-plus position-relative top-50 start-0 translate-middle-y");
}


/**
 * Submits the main form with a specified action.
 *
 * This function sets the value of the hidden input field with the ID "action"
 * to the provided action string, and then submits the form with the ID "mainForm".
 *
 * @param {string} action - The action to be performed, which will be set as the value
 *                          of the "action" input field before form submission.
 */
function postAction(action)
{
    let form = document.getElementById("mainForm");
    document.getElementById("action").setAttribute("value", action);
    
    form.submit();
}
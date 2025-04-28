function func1(round)
{
    let n1 = document.getElementById("current_round");
    n1.setAttribute("value", round);
    
    document.getElementById("the_form").submit();

}

function toogleExpansion(name)
{
    let n1 = document.getElementById(name);
    let c = n1.getAttribute("class");

    if (c === "fi-plus position-relative top-50 start-0 translate-middle-y") n1.setAttribute("class", "fi-minus position-relative top-50 start-0 translate-middle-y");
    else n1.setAttribute("class", "fi-plus position-relative top-50 start-0 translate-middle-y");
}

function postAction(action)
{
    let form = document.getElementById("mainForm");
    form.action = action;
    form.submit();
}
public class NavItem
{
    public NavItem(string label, string res, bool isSel, bool isDis)
    {
        Label = label;
        Resource = res;
        IsSelected = isSel;
        IsDisabled = isDis;
    }
    public string Label {get;set;} = "";
    public string Resource {get;set;} = "";
    public bool IsSelected {get;set;}
    public bool IsDisabled {get;set;}  
};
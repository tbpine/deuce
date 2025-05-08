public class NavItem
{
    public NavItem(string label, string res, bool isSel, bool isDis)
    {
        Label = label;
        Resource = res;
        IsSelected = isSel;
        IsDisabled = isDis;
    }

    public NavItem(string label, string res, bool isSel, bool isDis, string icon)
    {
        Label = label;
        Resource = res;
        IsSelected = isSel;
        IsDisabled = isDis;
        Icon = icon;
    }

    public NavItem(string label, string res, bool isSel, bool isDis, string icon,
    string controller, string action)
    {
        Label = label;
        Resource = res;
        IsSelected = isSel;
        IsDisabled = isDis;
        Icon = icon;
        Controller = controller;
        Action = action;
    }

    public string Label { get; set; } = "";
    public string Resource { get; set; } = "";
    public bool IsSelected { get; set; }
    public bool IsDisabled { get; set; }
    public string Icon { get; set; } = "";
    public string Controller { get; set; } = String.Empty;
    public string Action { get; set; } = String.Empty;

    
};
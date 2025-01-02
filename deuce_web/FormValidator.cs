using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deuce_web;
public class FormValidator : IFormValidator
{
    private readonly ILogger<FormValidator> _log;
    private PageModel? _page;

    public PageModel? Page { get=>_page; set=>_page = value; }   

    public string? ErrorElement { get; set; }

    /// <summary>
    /// Dependency injection
    /// </summary>
    /// <param name="logger">Web log</param>
    /// <param name="page">Page to check</param>
    public FormValidator(ILogger<FormValidator> logger)
    {
        _log = logger;
        
    }

    /// <summary>
    /// Check form submitted values 
    /// </summary>
    /// <returns>True if values are valid</returns>
    /// <exception cref="ArgumentException">No page specified</exception>
    public bool Check(bool storeInSession, Predicate<string> predicate)
    {
        if (_page is null) throw new ArgumentException("Page is null");
        //Extract values from form submittion
        foreach (var keyPair in _page.Request.Form)
        {
            //Skip ignore entries
            if (predicate.Invoke(keyPair.Key)) continue;

            //Application values are encoded 
            //with this format "deuce_i_varname"
            var matched = Regex.Match(keyPair.Key, @"deuce_([isdf]{1})_(\w*)");
            if (matched.Groups.Count == 3)
            {
                //Validate value type
                string typeChar = matched.Groups[1].Value;
                //"i" = integer
                //"s" = string
                //"f" = double/float
                //"d" = date
                string variableName = matched.Groups[2].Value;
                Debug.WriteLine($"{keyPair.Key}|{typeChar}|{variableName}={keyPair.Value}");
                bool isCorrectType = ValidateType(typeChar, keyPair.Value, storeInSession , variableName);
                //Form has an incorrect value
                if (!isCorrectType)
                {
                    ErrorElement = keyPair.Key;
                    return false;
                } 
            }
        }

        return true;

    }

    /// <summary>
    /// True if a value is of a specified type
    /// </summary>
    /// <param name="typeString">Type indicator</param>
    /// <param name="val">Value to check</param>
    /// <param name="storeInSession">True to store the form value in the current session</param>
    /// <returns>True if the value is of type</returns>
    private bool ValidateType(string typeString, string? val, bool storeInSession, string varibleName)
    {

        if (val is null) return false;

        //Check "val" is the specifiy  native type
        if (String.Compare(typeString, "s", true) == 0)
        {
            bool isValidString = !String.IsNullOrEmpty(val);
            if (isValidString && storeInSession) _page?.HttpContext.Session.SetString(varibleName, val);
            return isValidString;
        }
        else if (String.Compare(typeString, "i", true) == 0)
        {
            int iVal = 0;
            bool isInt = int.TryParse(val, out iVal);
            if (isInt && storeInSession) _page?.HttpContext.Session.SetString(varibleName, val);
            return isInt;
        }
        else if (String.Compare(typeString, "f", true) == 0)
        {
            decimal fVal = 0;
            bool isDecimal = decimal.TryParse(val, out fVal);
            if (isDecimal && storeInSession) _page?.HttpContext.Session.SetString(varibleName, fVal.ToString());
            return isDecimal;
        }
        else if (String.Compare(typeString, "d", true) == 0)
        {
            DateTime dt = DateTime.Now;
            bool isDate = DateTime.TryParse(val, out dt);
            if (isDate && storeInSession) _page?.HttpContext.Session.SetString(varibleName, dt.ToString("yyyy-MM-dd"));
            return isDate;
        }

        //Failed validation
        return false;
    }
}
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using deuce_web.ext;
using Microsoft.AspNetCore.Mvc;

namespace deuce_web.ext;

/// <summary>
/// Various functions to handle
/// persistance between requests
/// </summary>
public static class PageModelExt
{
    /// <summary>
    /// Set properties of a page with
    /// session values
    /// </summary>
    /// <param name="page">PageModel with properties</param>
    /// <param name="formKeys">let of session names</param>
    public static void SetViewData(this PageModel page, params string[] formKeys)
    {
        //For all names find the corresponding page property
        //and values from the session
        foreach(string key in formKeys)
        {
            //Remove underscores
            if (page.ViewData.ContainsKey(key))
                page.ViewData[key] = page.HttpContext.Session.GetString(key);
            else
                page.ViewData.Add(key, page.HttpContext.Session.GetString(key));

        }

    }

    public static void AddSessionKeys(this PageModel page, params string[] formKeys)
    {
        //For all names find the corresponding page property
        //and values from the session
        foreach(string key in formKeys)
        {
            if (!page.HttpContext.Session.Keys.Contains(key)) 
                page.HttpContext.Session.SetString(key,"");

        }
    }

    public static bool IsCustomEntry(this PageModel page, string key)
    {
        if (page.Request.Form.ContainsKey(key))
        {
            string? strInt = page.Request.Form[key];
            int iVal = 0;
            bool isInt = int.TryParse(strInt?.ToString(), out iVal) ;
            return iVal == 99;
        }

        return false;
    }

    /// <summary>
    /// Save a page's properties to session
    /// </summary>
    /// <param name="page"></param>
    public static void SaveToSession(this PageModel page)
    {
        var properties = page.GetType().GetProperties(System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase );
        
        foreach(PropertyInfo prop in properties)
        {
            var attr = prop.GetCustomAttribute<BindPropertyAttribute>();
            if (attr is not null)
            {
                object? propertyValue  = prop.GetValue(page);
                if (prop.PropertyType == typeof(string))
                {
                    page.HttpContext.Session.SetString(prop.Name, propertyValue?.ToString()??String.Empty);           
                }
                else if (prop.PropertyType == typeof(int))
                {
                    int ival= int.TryParse(propertyValue?.ToString()??String.Empty, out ival) ? ival : 0;
                    page.HttpContext.Session.SetInt32(prop.Name, ival);           
                }
                
            }
        }

    }

    public static void LoadFromSession(this PageModel page)
    {
        var properties = page.GetType().GetProperties(System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase );
        
        foreach(PropertyInfo prop in properties)
        {
            var attr = prop.GetCustomAttribute<BindPropertyAttribute>();
            if (attr is not null && prop is not null)
            {
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(page,page.HttpContext.Session.GetString(prop.Name) ?? String.Empty);    
                               
                }
                else if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(page,page.HttpContext.Session.GetInt32(prop.Name)??0);
                }
                
            }
        }

     
    }

}
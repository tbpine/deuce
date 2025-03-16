using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deuce_web;
public class FormUtils 
{
    /// <summary>
    /// Print out form values
    /// </summary>
    /// <param name="form">Form reference</param>
    public static void DebugOut(IFormCollection form)    
    {
        foreach(var kp in form)
            Debug.Print($"{kp.Key}={kp.Value}");
    }
}
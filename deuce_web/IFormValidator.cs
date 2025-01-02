using Microsoft.AspNetCore.Mvc.RazorPages;

namespace deuce_web;
public interface IFormValidator
{
    bool Check(bool storeInSession, Predicate<string> predicate);
    PageModel? Page { get; set;}   
    string? ErrorElement { get; set; }
}
using deuce;
using deuce.ext;
using System.Text;
using System.Reflection;

/// <summary>
/// Transport Displat attribute to HTML
/// </summary>
public class DisplayToHTML
{
    private ILookup _lookup;
    public DisplayToHTML(ILookup lookup)=> _lookup = lookup;

/// <summary>
   /// For each property of an object marked with the DisplayAttr,
   /// create a row .
   /// </summary>
   /// <param name="obj">Object search</param>
   /// <returns></returns>
   public async Task<string> ExtractDisplayProperty(object obj)
   {
      //The HTML to return
      StringBuilder sbHTML = new();

      //Get a list of properties to display
      var arrayOfProps = obj.GetType().GetProperties();

      for (int i = 0; i < (arrayOfProps?.Length ?? 0); i++)
      {
         //Only display those attributes marked with "Display"

         var propInfo = arrayOfProps?[i];

         var attrDisplay = propInfo?.GetCustomAttribute(typeof(DisplayAttribute));

         if (attrDisplay is null) continue;


         //Grab label and format if needed
         var format = (attrDisplay as DisplayAttribute)?.Format;
         var label = (attrDisplay as DisplayAttribute)?.Label;
         var lookupType = (attrDisplay as DisplayAttribute)?.LookupType;

         string textDisplay = "";

         //If the property is id and requires looking up , 
         //then ask the lookup manager
         if (lookupType is not null)
         {
            int? iId = (int?)propInfo?.GetValue(obj);
            if (iId.HasValue)
               textDisplay = await _lookup.GetLabel(iId.HasValue ? iId.Value : 0, lookupType) ?? "";
         }
        
         else if (propInfo?.PropertyType == typeof(DateTime))
         {
            //If the property is a DateTime, then format it
            DateTime? dtValue = (DateTime?)propInfo?.GetValue(obj);
            textDisplay = dtValue.HasValue ? (dtValue.Value.Equals(DateTime.MinValue) ? "" : dtValue.Value.ToString(format)) : "";
         }
         else
         {
            //Find the property type
            textDisplay = propInfo?.Format(obj, format) ?? "";
         }
         sbHTML.AppendLine(@$"<tr><td>{label}</td><td>{textDisplay}</td></tr>");
         sbHTML.AppendLine();

      }

      //Return the HTML string
      return sbHTML.ToString();
   }
}
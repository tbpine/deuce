using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using deuce;
using System.Data.Common;

namespace deuce_web.Pages;


public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IServiceProvider _sc;

    public List<Interval> Intervals { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, IServiceProvider sc)
    {
        _logger = logger;
        _sc = sc;
    }

    public async Task<IActionResult> OnGet()
    {
        
        var scope = _sc.CreateScope();
        DbConnection dbconn = scope.ServiceProvider.GetService<DbConnection>()!;
        MySqlConnectionStringBuilder sb = new();
        sb.Server = "deuce-server.mysql.database.azure.com";
        sb.Port = 3306;
        sb.UserID = "kutmqpncub";
        sb.Password = "sUpsJEQXzxEQ5WZ";
        sb.Database = "deuce";
        sb.SslMode = MySqlSslMode.Required;
        sb.SslCa = "DigiCertGlobalRootG2.crt.pem";

        dbconn.ConnectionString = sb.ToString();
        await dbconn.OpenAsync();

        var cmd = dbconn.CreateCommand();
        cmd.CommandText = "SELECT * from `interval`;";
        cmd.CommandType = System.Data.CommandType.Text;

        var reader = await cmd.ExecuteReaderAsync();

        List<Interval> list = new();
        while (reader.Read())
        {
            list.Add(new Interval(reader.GetInt32(0), reader.GetString(1)));
        }

        Intervals = list;


        return Page();
    }
}

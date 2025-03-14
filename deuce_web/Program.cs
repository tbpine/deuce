using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.HttpOverrides;
using System.Data.Common;
using deuce_web;
using Microsoft.Extensions.Caching.Memory;
using deuce;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorPages();

//
//Store the client's session in memory: server side.
//
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();

builder.Services.AddSession(options => {
    //After 10 mins, if there's no the session 
    //is not accessed, then clear it and the
    //client will need a new cookie.
    options.Cookie.Name = "deuce.session";
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

});

builder.Services.AddScoped<DbConnection, DbConnectionLocal>();
builder.Services.AddScoped<IHandlerNavItems, HandlerNavItems>();
builder.Services.AddScoped<ISideMenuHandler, AccSideMenuHandler>();
builder.Services.AddScoped<DbRepoTournament>();
builder.Services.AddScoped<DbRepoTournamentFee>();
builder.Services.AddScoped<DbRepoTournamentDetail>();
builder.Services.AddScoped<DbRepoPlayer>();
builder.Services.AddScoped<DbRepoRecordTeamPlayer>();
builder.Services.AddScoped<DbRepoTeam>();
builder.Services.AddScoped<DbRepoTournamentList>();
builder.Services.AddScoped<DbRepoTournamentProps>();
builder.Services.AddScoped<DbRepoVenue>();
builder.Services.AddScoped<DbRepoTournamentStatus>();
builder.Services.AddScoped<DbRepoTournamentValidation>();
builder.Services.AddScoped<DbRepoCountry>();
builder.Services.AddScoped<IAdaptorTeams, AdaptorFormTeams>();
builder.Services.AddScoped<ITournamentGateway, DBTournamentGateway>();

//Repos


//Singletons
builder.Services.AddSingleton<IFormValidator, FormValidator>();
builder.Services.AddSingleton<SessionProxy>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<ICacheMaster, CacheMasterDefault>();
builder.Services.AddSingleton<ILookup, LookupCache>();
builder.Services.AddSingleton<DisplayToHTML>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();

app.Run();

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.HttpOverrides;
using System.Data.Common;
using deuce_web;
using Microsoft.Extensions.Caching.Memory;

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

builder.Services.AddScoped<DbConnection, MySqlConnection>();
builder.Services.AddScoped<IHandlerNavItems, HandlerNavItems>();
builder.Services.AddScoped<ISideMenuHandler, AccSideMenuHandler>();
//Singletons
builder.Services.AddSingleton<IFormValidator, FormValidator>();
builder.Services.AddSingleton<SessionProxy>();
builder.Services.AddSingleton<ITournamentGateway, DBTournamentGateway>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddSingleton<ICacheMaster, CacheMasterDefault>();




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

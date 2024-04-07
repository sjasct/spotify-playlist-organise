using Microsoft.AspNetCore.Mvc;
using PlaylistOrganiser;
using PlaylistOrganiser.Handlers;
using PlaylistOrganiser.Models.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<AuthHandler>();

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("Config"));

var host = builder.Build();
host.UseHttpsRedirection();

host.MapGet("/auth/init", ([FromServices] AuthHandler auth) => { return Results.Redirect(auth.GetAuthUrl().ToString()); });
host.MapGet("/auth/redirect", async ([FromServices] AuthHandler auth, string code) => { await auth.ExchangeCode(code); });

host.Run();
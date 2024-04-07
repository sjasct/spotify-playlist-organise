using Microsoft.AspNetCore.Mvc;
using PlaylistOrganiser.Extensions;
using PlaylistOrganiser.Factories;
using PlaylistOrganiser.Handlers;
using PlaylistOrganiser.Jobs;
using PlaylistOrganiser.Models.Configuration;
using Quartz;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddQuartz(q =>
{
    q.AddJobAndTrigger<PlaylistOrganiserJob>("PlaylistOrganiser");
});
builder.Services.AddQuartzServer(q => q.WaitForJobsToComplete = true);

builder.Services.AddSingleton<AuthHandler>();
builder.Services.AddScoped<SpotifyClientFactory>();

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("Config"));

var host = builder.Build();
host.UseHttpsRedirection();

host.MapGet("/auth/init", ([FromServices] AuthHandler auth) => { return Results.Redirect(auth.GetAuthUrl().ToString()); });
host.MapGet("/auth/redirect", async ([FromServices] AuthHandler auth, string code) => { await auth.ExchangeCode(code); });

host.Run();
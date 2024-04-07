using Microsoft.Extensions.Options;
using PlaylistOrganiser.Extensions;
using PlaylistOrganiser.Factories;
using PlaylistOrganiser.Models.Configuration;
using Quartz;
using SpotifyAPI.Web;

namespace PlaylistOrganiser.Jobs;

[DisallowConcurrentExecution]
public class PlaylistOrganiserJob : IJob
{
    private readonly ILogger<PlaylistOrganiserJob> _logger;
    private readonly SpotifyClientFactory _spotifyFactory;
    private readonly IOptions<AppConfig> _config;

    public PlaylistOrganiserJob(ILogger<PlaylistOrganiserJob> logger, SpotifyClientFactory spotifyFactory, IOptions<AppConfig> config)
    {
        _logger = logger;
        _spotifyFactory = spotifyFactory;
        _config = config;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var client = _spotifyFactory.CreateClient();

        if (client is null)
        {
            _logger.LogInformation("Client null, please auth");
            return;
        }

        var allOriginItems = new List<PlaylistTrack<FullTrack>>();
        foreach (var originId in _config.Value.Organiser.Origins)
        {
            allOriginItems.AddRange(await client.GetPlaylistItems(originId));
        }

        var combinedPlaylist = await client.GetPlaylistItems(_config.Value.Organiser.Combined);
        var archivePlaylist = await client.GetPlaylistItems(_config.Value.Organiser.Archive);
        var freshPlaylist = await client.GetPlaylistItems(_config.Value.Organiser.Fresh);

        var newItems = allOriginItems.Where(track => track.NotIn(combinedPlaylist));

        await client.AddToPlaylist(_config.Value.Organiser.Combined, newItems);
        await client.AddToPlaylist(_config.Value.Organiser.Archive, allOriginItems.Where(track => track.NotIn(archivePlaylist)));
        await client.RemoveFromPlaylist(_config.Value.Organiser.Combined, combinedPlaylist.Where(track => track.NotIn(allOriginItems)));
        
        _logger.LogInformation("Done");
    }
}
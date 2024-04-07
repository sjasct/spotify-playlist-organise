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

        var newItems = allOriginItems.Where(track => track.NotIn(combinedPlaylist)).ToList();

        await client.AddToPlaylist(_config.Value.Organiser.Combined, newItems);
        await client.AddToPlaylist(_config.Value.Organiser.Archive, allOriginItems.Where(track => track.NotIn(archivePlaylist)));
        await client.RemoveFromPlaylist(_config.Value.Organiser.Combined, combinedPlaylist.Where(track => track.NotIn(allOriginItems)));

        var freshItems = newItems.OrderByDescending(x => x.AddedAt).Where(x => x.NotIn(freshPlaylist)).ToList();

        if (freshItems.Count > _config.Value.Organiser.FreshLimit)
        {
            freshItems = freshItems.Take(_config.Value.Organiser.FreshLimit).ToList();
        }

        var unfreshItems = freshPlaylist.OrderBy(x => x.AddedAt).Take(freshItems.Count);

        await client.RemoveFromPlaylist(_config.Value.Organiser.Fresh, unfreshItems);
        await client.AddToPlaylist(_config.Value.Organiser.Fresh, freshItems);
        
        _logger.LogInformation("Done");
    }
}
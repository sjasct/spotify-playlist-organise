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
            var playlist = await client.Playlists.Get(originId);
            _logger.LogInformation($"Got basic info for playlist {originId} '{playlist.Name}'");

            if (playlist.Tracks is null)
            {
                _logger.LogInformation($"Playlist '{playlist.Name}' has no tracks");
                continue;
            }
            
            var items = await client.PaginateAll(playlist.Tracks);
            allOriginItems.AddRange(items.AsFullTracks());
        }

        var combinedPlaylist = await client.Playlists.Get(_config.Value.Organiser.Combined);
        _logger.LogInformation($"Got basic info for FULL playlist '{combinedPlaylist.Name}'");

        var combinedItems = combinedPlaylist.Tracks != null
            ? (await client.PaginateAll(combinedPlaylist.Tracks)).AsFullTracks()
            : new List<PlaylistTrack<FullTrack>>();

        foreach (var item in allOriginItems)
        {
            _logger.LogInformation($"Track ID: '{item.Track.Id}' Name: '{item.Track.Name}' Artists: {string.Join(',', item.Track.Artists.Select(x => x.Name))}");
        }

        var notInCombined = allOriginItems.Where(x => combinedItems.All(y => y.Track.Id != x.Track.Id)).ToList();

        if (notInCombined.Any())
        {
            foreach (var addChunk in notInCombined.Chunk(100))
            {
                await client.Playlists.AddItems(combinedPlaylist.Id,
                    new PlaylistAddItemsRequest(addChunk.Select(x => x.Track.Uri).ToList()));
            }
        }

        
    }
}
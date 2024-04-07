using PlaylistOrganiser.Factories;
using Quartz;
using SpotifyAPI.Web;

namespace PlaylistOrganiser.Jobs;

public class PlaylistOrganiserJob : IJob
{
    private readonly ILogger<PlaylistOrganiserJob> _logger;
    private readonly SpotifyClientFactory _spotifyFactory;

    public PlaylistOrganiserJob(ILogger<PlaylistOrganiserJob> logger, SpotifyClientFactory spotifyFactory)
    {
        _logger = logger;
        _spotifyFactory = spotifyFactory;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var client = _spotifyFactory.CreateClient();

        if (client is null)
        {
            _logger.LogInformation("Client null, please auth");
        }
        else
        {
            try
            {
                var user = await client.UserProfile.Current();
                _logger.LogInformation($"User {user.Email}");
                
                var playlist = await client.Playlists.Create(user.Id, new PlaylistCreateRequest(DateTime.UtcNow.ToString()));
                _logger.LogInformation($"Created playlist '{playlist.Name}' ({playlist.Id})");
            }
            catch (APIException ex)
            {
                _logger.LogError(ex, "Spotify threw an error");
            }
        }
    }
}
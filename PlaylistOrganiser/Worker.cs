using PlaylistOrganiser.Factories;
using SpotifyAPI.Web;

namespace PlaylistOrganiser;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly SpotifyClientFactory _spotifyFactory;

    public Worker(ILogger<Worker> logger, SpotifyClientFactory spotifyFactory)
    {
        _logger = logger;
        _spotifyFactory = spotifyFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
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
            
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(10000, stoppingToken);
        }
    }
}
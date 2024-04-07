using Microsoft.Extensions.Options;
using PlaylistOrganiser.Handlers;
using PlaylistOrganiser.Models.Configuration;
using SpotifyAPI.Web;

namespace PlaylistOrganiser.Factories;

public class SpotifyClientFactory(AuthHandler authHandler, IOptions<AppConfig> config)
{
    public SpotifyClient? CreateClient()
    {
        var tokens = authHandler.GetTokens();
        if (tokens is null)
        {
            return null;
        }

        return new SpotifyClient(SpotifyClientConfig
            .CreateDefault()
            .WithRetryHandler(new SimpleRetryHandler{RetryAfter = TimeSpan.FromSeconds(2), RetryTimes = 5})
            .WithAuthenticator(new AuthorizationCodeAuthenticator(config.Value.ApiCredentials.ClientId,
                config.Value.ApiCredentials.Secret, tokens)));
    }
}
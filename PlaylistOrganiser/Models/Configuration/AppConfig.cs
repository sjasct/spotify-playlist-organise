namespace PlaylistOrganiser.Models.Configuration;

public class AppConfig
{
    public required string BaseUrl { get; set; }
    public required SpotifyApiCredentials ApiCredentials { get; set; }
}